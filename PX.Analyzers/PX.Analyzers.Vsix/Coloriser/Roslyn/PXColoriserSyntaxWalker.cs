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
			private readonly PXRoslynColorizerTagger tagger;
			private readonly ParsedDocument document;

			public PXColoriserSyntaxWalker(PXRoslynColorizerTagger aTagger, ParsedDocument parsedDocument) : base(SyntaxWalkerDepth.StructuredTrivia)
			{
				aTagger.ThrowOnNull(nameof(aTagger));
				parsedDocument.ThrowOnNull(nameof(parsedDocument));

				tagger = aTagger;
				document = parsedDocument;
			}

			public override void VisitIdentifierName(IdentifierNameSyntax node)
			{
				ITypeSymbol typeSymbol = document.SemanticModel.GetSymbolInfo(node).Symbol as ITypeSymbol;

                if (typeSymbol == null)
                {
                    base.VisitIdentifierName(node);
                    return;
                }

				if (typeSymbol.IsDAC())
				{
					AddTag(node.Span, tagger.Provider.DacType);				
				}
				else if (typeSymbol.IsDacField())
				{
					AddTag(node.Span, tagger.Provider.FieldType);				
				} 
			}

            public override void VisitGenericName(GenericNameSyntax genericNode)
            {    
                ITypeSymbol typeSymbol = document.SemanticModel.GetSymbolInfo(genericNode).Symbol as ITypeSymbol;
              
                if (typeSymbol == null)
                {
                    base.VisitGenericName(genericNode);
                    return;
                }

				if (typeSymbol.IsBqlParameter())
                {
                    AddTag(genericNode.Identifier.Span, tagger.Provider.BqlParameterType);
                }
                else if (typeSymbol.IsBqlCommand() || typeSymbol.IsBqlOperator())
                {
                    AddTag(genericNode.Identifier.Span, tagger.Provider.BqlOperatorType);
                }
			           
                base.VisitGenericName(genericNode);
            }

            public override void VisitQualifiedName(QualifiedNameSyntax node)
            {
                ITypeSymbol typeSymbol = document.SemanticModel.GetSymbolInfo(node).Symbol as ITypeSymbol;

                if (typeSymbol == null)
                {
                    base.VisitQualifiedName(node);
                    return;
                }

                if (typeSymbol.IsBqlConstant())
                {
                    AddTag(node.Left.Span, tagger.Provider.BqlConstantPrefixType);
                    AddTag(node.Right.Span, tagger.Provider.BqlConstantEndingType);
                }

                base.VisitQualifiedName(node);
            }

            private void AddTag(TextSpan span, IClassificationType classificationType)
			{
				ITagSpan<IClassificationTag> tag = span.ToTagSpan(tagger.Cache, classificationType);

				if (tag != null)
					tagger.TagsList.Add(tag);
			}
        }	
	}
}
