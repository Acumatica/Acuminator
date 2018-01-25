﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using PX.Analyzers.Vsix.Utilities;


namespace PX.Analyzers.Coloriser
{
	public partial class PXRoslynColorizerTagger : PXColorizerTaggerBase
	{
		protected class PXColoriserSyntaxWalker : CSharpSyntaxWalker
		{
            private const string varKeyword = "var";

            private long visitedNodesCounter = 0;
            private readonly PXRoslynColorizerTagger tagger;
			private readonly ParsedDocument document;
            private int braceLevel;
            private bool isInsideBqlCommand;
            private readonly CancellationToken cancellationToken;

			public PXColoriserSyntaxWalker(PXRoslynColorizerTagger aTagger, ParsedDocument parsedDocument, CancellationToken cToken) :
                                      base(SyntaxWalkerDepth.Node)
			{
				aTagger.ThrowOnNull(nameof(aTagger));
				parsedDocument.ThrowOnNull(nameof(parsedDocument));

				tagger = aTagger;
				document = parsedDocument;
                cancellationToken = cToken;
            }

			public override void VisitIdentifierName(IdentifierNameSyntax node)
			{
                string nodeText = node.Identifier.ValueText;
                TextSpan span = node.Span;

                if (cancellationToken.IsCancellationRequested || IsVar(nodeText) /* || SearchIdentifierInSymbolsCache(span, node, nodeText)*/)
                    return;
              
                ITypeSymbol typeSymbol = document.SemanticModel.GetSymbolInfo(node).Symbol as ITypeSymbol;

                if (typeSymbol == null)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        base.VisitIdentifierName(node);

                    return;
                }

                if (cancellationToken.IsCancellationRequested)
                    return;

				if (typeSymbol.IsDAC())
				{
                    //AddTagAndCacheIt(nodeText, TypeNames.IBqlTable, span, tagger.Provider.DacType);		
                    AddClassificationTag(span, tagger.Provider.DacType);		
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

                    AddClassificationTag(span, tagger.Provider.FieldType);
                }
                else if (typeSymbol.IsDacExtension())
                {
                    AddClassificationTag(span, tagger.Provider.DacExtensionType);
                }
                else if (typeSymbol.IsBqlConstant())
                {
                    // AddTagAndCacheIt(nodeText, TypeNames.Constant, span, tagger.Provider.BqlConstantEndingType);
                    AddClassificationTag(span, tagger.Provider.BqlConstantEndingType);
                }
                else if (typeSymbol.IsBqlOperator())
                {
                    //AddTagAndCacheIt(nodeText, TypeNames.IBqlCreator, span, tagger.Provider.BqlOperatorType);
                    AddClassificationTag(span, tagger.Provider.BqlOperatorType);
                }

                UpdateCodeEditorIfNecessary();
            }

            public override void VisitGenericName(GenericNameSyntax genericNode)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

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
                    if (!cancellationToken.IsCancellationRequested)
                        base.VisitGenericName(genericNode);

                    return;
                }

                if (cancellationToken.IsCancellationRequested)
                    return;

                if (typeSymbol.IsBqlCommand())
                {
                    isInsideBqlCommand = true;
                    AddClassificationTag(span, tagger.Provider.BqlOperatorType);
                    // AddTagAndCacheIt(nodeText, TypeNames.BqlCommand, span, tagger.Provider.BqlOperatorType);

                    if (!cancellationToken.IsCancellationRequested)
                        base.VisitGenericName(genericNode);

                    isInsideBqlCommand = false;
                    return;
                }
				else if (typeSymbol.IsBqlParameter())
                {
                    AddClassificationTag(span, tagger.Provider.BqlParameterType);
                    //AddTagAndCacheIt(nodeText, TypeNames.IBqlParameter, span, tagger.Provider.BqlParameterType);
                }
                else if (typeSymbol.IsBqlOperator())
                {
                    AddClassificationTag(span, tagger.Provider.BqlOperatorType);
                    //AddTagAndCacheIt(nodeText, TypeNames.IBqlCreator, span, tagger.Provider.BqlOperatorType);
                }

                if (!cancellationToken.IsCancellationRequested)
                    base.VisitGenericName(genericNode);

                UpdateCodeEditorIfNecessary();
            }

            public override void VisitQualifiedName(QualifiedNameSyntax node)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

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
                    if (!cancellationToken.IsCancellationRequested)
                        base.VisitQualifiedName(node);

                    return;
                }

                if (cancellationToken.IsCancellationRequested)
                    return;

                if (typeSymbol.IsBqlConstant())
                {
                    AddClassificationTag(leftSpan, tagger.Provider.BqlConstantPrefixType);
                    AddClassificationTag(rightSpan, tagger.Provider.BqlConstantEndingType);

                    //document.SymbolsCache.AddNodeToCache(nodeText, TypeNames.Constant);                  
                }

                if (!cancellationToken.IsCancellationRequested)
                    base.VisitQualifiedName(node);

                UpdateCodeEditorIfNecessary();
            }
          
            public override void VisitTypeArgumentList(TypeArgumentListSyntax node)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (!isInsideBqlCommand)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        base.VisitTypeArgumentList(node);

                    return;
                }

                braceLevel++;

                if (cancellationToken.IsCancellationRequested)
                    return;

                AddOutliningTag(node.Span);

                if (braceLevel <= Constants.MaxBraceLevel && !cancellationToken.IsCancellationRequested &&
                    tagger.Provider.BraceTypeByLevel.TryGetValue(braceLevel, out IClassificationType braceClassificationType))
                {
                    AddClassificationTag(node.LessThanToken.Span, braceClassificationType);
                    AddClassificationTag(node.GreaterThanToken.Span, braceClassificationType);
                }

                if (!cancellationToken.IsCancellationRequested)
                    base.VisitTypeArgumentList(node);

                braceLevel--;
                UpdateCodeEditorIfNecessary();
            }

            public override void Visit(SyntaxNode node)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (visitedNodesCounter < long.MaxValue)
                    visitedNodesCounter++;

                base.Visit(node);
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


            private void AddClassificationTag(TextSpan span, IClassificationType classificationType)
            {
                ITagSpan<IClassificationTag> tag = span.ToClassificationTagSpan(tagger.Snapshot, classificationType);
                tagger.ClassificationTagsCache.AddTag(tag);
            }

            private void AddOutliningTag(TextSpan span)
            {
                ITagSpan<IOutliningRegionTag> tag = span.ToOutliningTagSpan(tagger.Snapshot);
                tagger.OutliningsTagsCache.AddTag(tag);
            }

            private void UpdateCodeEditorIfNecessary()
            {
                if (visitedNodesCounter <= Constants.ChunkSize || cancellationToken.IsCancellationRequested)
                    return;

                visitedNodesCounter = 0;
                tagger.ClassificationTagsCache.PersistIntermediateResult();
                tagger.OutliningsTagsCache.PersistIntermediateResult();
                tagger.RaiseTagsChanged();
            }

            /// <summary>
            /// Check if node is var keyword
            /// </summary>
            /// <param name="nodeText">The node text.</param>
            /// <returns/>         
            private bool IsVar(string nodeText) => nodeText == varKeyword;

            #region Commented methods for symbol's cache optimization



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



            //private void AddTagAndCacheIt(string cachedText, string tagTypeName, TextSpan span, IClassificationType classificationType)
            //{
            //    ITagSpan<IClassificationTag> tag = span.ToTagSpan(tagger.Cache, classificationType);

            //    if (tag != null)
            //    {
            //        tagger.TagsList.Add(tag);
            //        document.SymbolsCache.AddNodeToCache(cachedText, tagTypeName);
            //    }
            //}
            #endregion
        }
    }
}
