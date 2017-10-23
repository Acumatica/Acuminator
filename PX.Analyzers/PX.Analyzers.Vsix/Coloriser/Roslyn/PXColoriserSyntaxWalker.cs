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
    public class PXColoriserSyntaxWalker : CSharpSyntaxWalker
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

        public override void VisitGenericName(GenericNameSyntax genericNode)
        {
            ITypeSymbol typeSymbol = document.SemanticModel.GetSymbolInfo(genericNode).Symbol as ITypeSymbol;

            if (!typeSymbol.IsBqlCommand(document.PXContext))
                return;


        }
    }
}
