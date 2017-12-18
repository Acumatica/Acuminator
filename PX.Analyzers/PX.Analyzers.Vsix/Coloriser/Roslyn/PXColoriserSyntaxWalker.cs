using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace PX.Analyzers.Coloriser
{
	public partial class PXRoslynColorizerTagger : PXColorizerTaggerBase
	{
		protected class PXColoriserSyntaxWalker : CSharpSyntaxWalker
		{
            private const string varKeyword = "var";

            private readonly PXRoslynColorizerTagger tagger;
			private readonly ParsedDocument document;
            private int braceLevel;
            private bool isInsideBqlCommand;

			public PXColoriserSyntaxWalker(PXRoslynColorizerTagger aTagger, ParsedDocument parsedDocument) : base(SyntaxWalkerDepth.Node)
			{
				aTagger.ThrowOnNull(nameof(aTagger));
				parsedDocument.ThrowOnNull(nameof(parsedDocument));

				tagger = aTagger;
				document = parsedDocument;
			}

			public override void VisitIdentifierName(IdentifierNameSyntax node)
			{
                string nodeText = node.Identifier.ValueText;
                TextSpan span = node.Span;

                if (IsVar(nodeText) /* || SearchIdentifierInSymbolsCache(span, node, nodeText)*/)
                    return;
              
                ITypeSymbol typeSymbol = document.SemanticModel.GetSymbolInfo(node).Symbol as ITypeSymbol;

                if (typeSymbol == null)
                {
                    base.VisitIdentifierName(node);
                    return;
                }
               
				if (typeSymbol.IsDAC())
				{
                    //AddTagAndCacheIt(nodeText, TypeNames.IBqlTable, span, tagger.Provider.DacType);		
                    AddTag(span, tagger.Provider.DacType);		
                }
                else if (typeSymbol.IsDacField())
				{
                    //var parent = node.Parent;

                    //if (parent.Kind() == SyntaxKind.QualifiedName)
                    //{                       
                    //    AddTagAndCacheIt(parent.ToString(), TypeNames.IBqlField, span, tagger.Provider.FieldType);
                    //}
                    //else
                    //{
                    //    AddTag(span, tagger.Provider.FieldType);
                    //}

                    AddTag(span, tagger.Provider.FieldType);
                }
                else if (typeSymbol.IsBqlConstant())
                {
                    // AddTagAndCacheIt(nodeText, TypeNames.Constant, span, tagger.Provider.BqlConstantEndingType);
                    AddTag(span, tagger.Provider.BqlConstantEndingType);
                }
                else if (typeSymbol.IsBqlOperator())
                {
                    //AddTagAndCacheIt(nodeText, TypeNames.IBqlCreator, span, tagger.Provider.BqlOperatorType);
                    AddTag(span, tagger.Provider.BqlOperatorType);
                }
			}

            public override void VisitGenericName(GenericNameSyntax genericNode)
            {
                var identifier = genericNode.Identifier;  //The property hides the creation of new node and some other overhead. Must be cautious with Roslyn properties
                string nodeText = identifier.ValueText;
                TextSpan span = identifier.Span;

                //if (SearchGenericNameInSymbolsCache(span, nodeText))
                //{
                //    base.VisitGenericName(genericNode);
                //    return;
                //}
                   
                ITypeSymbol typeSymbol = document.SemanticModel.GetSymbolInfo(genericNode).Symbol as ITypeSymbol;
              
                if (typeSymbol == null)
                {
                    base.VisitGenericName(genericNode);
                    return;
                }

                if (typeSymbol.IsBqlCommand())
                {
                    isInsideBqlCommand = true;
                    AddTag(span, tagger.Provider.BqlOperatorType);
                   // AddTagAndCacheIt(nodeText, TypeNames.BqlCommand, span, tagger.Provider.BqlOperatorType);
                    base.VisitGenericName(genericNode);
                    isInsideBqlCommand = false;
                    return;
                }
				else if (typeSymbol.IsBqlParameter())
                {
                    AddTag(span, tagger.Provider.BqlParameterType);
                    //AddTagAndCacheIt(nodeText, TypeNames.IBqlParameter, span, tagger.Provider.BqlParameterType);
                }
                else if (typeSymbol.IsBqlOperator())
                {
                    AddTag(span, tagger.Provider.BqlOperatorType);
                    //AddTagAndCacheIt(nodeText, TypeNames.IBqlCreator, span, tagger.Provider.BqlOperatorType);
                }
			           
                base.VisitGenericName(genericNode);
            }

            public override void VisitQualifiedName(QualifiedNameSyntax node)
            {
                string nodeText = node.ToString();
                TextSpan leftSpan = node.Left.Span;
                TextSpan rightSpan = node.Right.Span;

                //if (SearchQualifiedNameInSymbolsCache(leftSpan, rightSpan, nodeText))
                //{
                //    base.VisitQualifiedName(node);
                //    return;
                //}

                ITypeSymbol typeSymbol = document.SemanticModel.GetSymbolInfo(node).Symbol as ITypeSymbol;

                if (typeSymbol == null)
                {
                    base.VisitQualifiedName(node);
                    return;
                }

                if (typeSymbol.IsBqlConstant())
                {
                    AddTag(leftSpan, tagger.Provider.BqlConstantPrefixType);
                    AddTag(rightSpan, tagger.Provider.BqlConstantEndingType);

                    //document.SymbolsCache.AddNodeToCache(nodeText, TypeNames.Constant);                  
                }

                base.VisitQualifiedName(node);
            }
          
            public override void VisitTypeArgumentList(TypeArgumentListSyntax node)
            {
                if (!isInsideBqlCommand)
                {
                    base.VisitTypeArgumentList(node);
                    return;
                }

                braceLevel++;
                
                if (braceLevel <= Constants.MaxBraceLevel)
                {
                    IClassificationType braceClassificationType = tagger.Provider.BraceTypeByLevel[braceLevel];
                    AddTag(node.LessThanToken.Span, braceClassificationType);
                    AddTag(node.GreaterThanToken.Span, braceClassificationType);
                }


                base.VisitTypeArgumentList(node);

                braceLevel--;
            }

            #region Visit XML comments methods
            public override void VisitXmlCrefAttribute(XmlCrefAttributeSyntax node)
            {
                return;  //To prevent coloring in XML comments don't call base method
            }

            public override void VisitXmlComment(XmlCommentSyntax node)
            {
                return;  //To prevent coloring in XML comments don't call base method
            }

            public override void VisitCrefBracketedParameterList(CrefBracketedParameterListSyntax node)
            {
                return;  //To prevent coloring in XML comments don't call base method
            }

            public override void VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node)
            {
                return;  //To prevent coloring in XML comments don't call base method
            }
            #endregion


            //private bool SearchIdentifierInSymbolsCache(TextSpan span, IdentifierNameSyntax node, string nodeText)
            //{
            //    var parentKind = node.Parent.Kind();

            //    switch (parentKind)
            //    {
            //        case SyntaxKind.InvocationExpression:
            //        case SyntaxKind.VariableDeclarator:
            //        case SyntaxKind.SimpleMemberAccessExpression:
            //        case SyntaxKind.QueryBody:
            //        case SyntaxKind.SimpleAssignmentExpression:
            //        case SyntaxKind.PointerMemberAccessExpression:
            //            return false;
            //    }
                 
            //    if (document.SymbolsCache.IsDAC(nodeText))
            //    {
            //        AddTag(span, tagger.Provider.DacType);
            //    }
            //    else if (document.SymbolsCache.IsDacField(nodeText))
            //    {
            //        AddTag(span, tagger.Provider.FieldType);
            //    }
            //    else if (document.SymbolsCache.IsBqlConstant(nodeText))
            //    {
            //        AddTag(span, tagger.Provider.BqlConstantEndingType);
            //    }
            //    else if (document.SymbolsCache.IsBqlOperator(nodeText))
            //    {
            //        AddTag(span, tagger.Provider.BqlOperatorType);
            //    }
            //    else
            //        return false;

            //    return true;
            //}

            //private bool SearchGenericNameInSymbolsCache(TextSpan span, string nodeText)
            //{
            //    if (document.SymbolsCache.IsBqlCommand(nodeText) || document.SymbolsCache.IsBqlOperator(nodeText))
            //    {
            //        AddTag(span, tagger.Provider.BqlOperatorType);
            //    }
            //    else if (document.SymbolsCache.IsBqlParameter(nodeText))
            //    {
            //        AddTag(span, tagger.Provider.BqlParameterType);
            //    }
            //    else
            //        return false;

            //    return true;
            //}

            //private bool SearchQualifiedNameInSymbolsCache(TextSpan leftSpan, TextSpan rightSpan, string nodeText)
            //{
            //    if (document.SymbolsCache.IsBqlConstant(nodeText))
            //    {
            //        AddTag(leftSpan, tagger.Provider.BqlConstantPrefixType);
            //        AddTag(rightSpan, tagger.Provider.BqlConstantEndingType);
            //    }
            //    else
            //        return false;

            //    return true;
            //}

            private void AddTag(TextSpan span, IClassificationType classificationType)
			{
				ITagSpan<IClassificationTag> tag = span.ToTagSpan(tagger.Cache, classificationType);

				if (tag != null)
					tagger.TagsList.Add(tag);
			}

            //private void AddTagAndCacheIt(string cachedText, string tagTypeName, TextSpan span, IClassificationType classificationType)
            //{
            //    ITagSpan<IClassificationTag> tag = span.ToTagSpan(tagger.Cache, classificationType);

            //    if (tag != null)
            //    {
            //        tagger.TagsList.Add(tag);
            //        document.SymbolsCache.AddNodeToCache(cachedText, tagTypeName);
            //    }
            //}

            /// <summary>
            /// More optimized
            /// </summary>
            /// <param name="nodeText">The node text.</param>
            /// <returns/>         
            private bool IsVar(string nodeText) => nodeText == varKeyword;
        }	
	}
}
