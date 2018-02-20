using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;
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
            private int attributeLevel;
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
                if (tagger.Provider.Package.ColorOnlyInsideBQL && !isInsideBqlCommand)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        base.VisitIdentifierName(node);

                    return;
                }

                string nodeText = node.Identifier.ValueText;
                TextSpan span = node.Span;

                if (cancellationToken.IsCancellationRequested || IsVar(nodeText))
                    return;

                ITypeSymbol typeSymbol = document.SemanticModel.GetSymbolInfo(node).Symbol as ITypeSymbol;

                if (typeSymbol == null)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        base.VisitIdentifierName(node);
                    }

                    return;
                }

                if (cancellationToken.IsCancellationRequested)
                    return;

                ColoredCodeType? coloredCodeType = typeSymbol.GetColoringTypeFromIdentifier();
                IClassificationType classificationType = coloredCodeType.HasValue
                    ? tagger.Provider[coloredCodeType.Value]
                    : null;

                if (classificationType != null && 
                   (coloredCodeType.Value != ColoredCodeType.PXGraph || tagger.Provider.Package.PXGraphColoringEnabled))
                {                    
                    AddClassificationTag(span, classificationType);
                }

                UpdateCodeEditorIfNecessary();
            }

            public override void VisitGenericName(GenericNameSyntax genericNode)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                                       
                ITypeSymbol typeSymbol = document.SemanticModel.GetSymbolInfo(genericNode).Symbol as ITypeSymbol;
              
                if (typeSymbol == null)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        base.VisitGenericName(genericNode);

                    return;
                }

                if (cancellationToken.IsCancellationRequested)
                    return;

                ColoredCodeType? coloredCodeType = typeSymbol.GetColoringTypeFromGenericName();
                IClassificationType classificationType = coloredCodeType.HasValue 
                    ? tagger.Provider[coloredCodeType.Value]
                    : null;

                if (classificationType == null)
                {
                    UpdateCodeEditorIfNecessary();
                    return;
                }

                if (coloredCodeType.Value == ColoredCodeType.BqlCommand)
                {
                    try
                    {
                        isInsideBqlCommand = true;
                        var typeArgumentList = genericNode.TypeArgumentList;

                        if (typeArgumentList.Arguments.Count > 1)
                        {
                            AddOutliningTagToBQL(typeArgumentList.Span);
                        }

                        AddClassificationTag(genericNode.Identifier.Span, classificationType);

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            base.VisitGenericName(genericNode);
                        }
                    }
                    finally
                    {
                        isInsideBqlCommand = false;
                    }                              
                }
                else
                {
                    ColorBqlPartsAndPXActions(genericNode, typeSymbol, classificationType, coloredCodeType.Value);
                }

                UpdateCodeEditorIfNecessary();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ColorBqlPartsAndPXActions(GenericNameSyntax genericNode, ITypeSymbol typeSymbol, IClassificationType classificationType, 
                                                   ColoredCodeType coloredCodeType)
            {
                if (tagger.Provider.Package.ColorOnlyInsideBQL && !isInsideBqlCommand)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        base.VisitGenericName(genericNode);

                    return;
                }
                                 
                switch (coloredCodeType)
                {
                    case ColoredCodeType.BqlOperator:
                        {
                            TextSpan? outliningSpan = typeSymbol.GetBqlOperatorOutliningTextSpan(genericNode);

                            if (outliningSpan.HasValue)
                            {
                                AddOutliningTagToBQL(outliningSpan.Value);
                            }

                            AddClassificationTag(genericNode.Identifier.Span, classificationType);
                            break;
                        }
                    case ColoredCodeType.BqlParameter:
                        {
                            AddClassificationTag(genericNode.Identifier.Span, classificationType);
                            break;
                        }
                    case ColoredCodeType.PXAction:
                        {
                            if (tagger.Provider.Package.PXActionColoringEnabled)
                                AddClassificationTag(genericNode.Identifier.Span, classificationType);

                            break;
                        }
                }

                if (!cancellationToken.IsCancellationRequested)
                    base.VisitGenericName(genericNode);              
            }

            public override void VisitQualifiedName(QualifiedNameSyntax node)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (tagger.Provider.Package.ColorOnlyInsideBQL && !isInsideBqlCommand)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        base.VisitQualifiedName(node);

                    return;
                }

                string nodeText = node.ToString();
                TextSpan leftSpan = node.Left.Span;
                TextSpan rightSpan = node.Right.Span;              
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
                    AddClassificationTag(leftSpan, tagger.Provider[ColoredCodeType.BQLConstantPrefix]);
                    AddClassificationTag(rightSpan, tagger.Provider[ColoredCodeType.BQLConstantEnding]);
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

                try
                {
                    if (braceLevel <= ColoringConstants.MaxBraceLevel && !cancellationToken.IsCancellationRequested)
                    {
                        IClassificationType braceClassificationType = tagger.Provider[braceLevel];

                        if (braceClassificationType != null)
                        {
                            AddClassificationTag(node.LessThanToken.Span, braceClassificationType);
                            AddClassificationTag(node.GreaterThanToken.Span, braceClassificationType);
                        }
                    }

                    if (!cancellationToken.IsCancellationRequested)
                        base.VisitTypeArgumentList(node);
                }
                finally
                {
                    braceLevel--;
                }

                UpdateCodeEditorIfNecessary();
            }

            public override void VisitAttributeList(AttributeListSyntax node)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                attributeLevel++;

                try
                {
                    if (attributeLevel <= OutliningConstants.MaxAttributeOutliningLevel)
                    {
                        AddOutliningTagToAttribute(node);
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        base.VisitAttributeList(node);
                    }
                }
                finally
                {
                    attributeLevel--;
                }
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

            private void AddOutliningTagToBQL(TextSpan span)
            {
                if (!tagger.Provider.Package.UseBqlOutlining)
                    return;

                ITagSpan<IOutliningRegionTag> tag = span.ToOutliningTagSpan(tagger.Snapshot);
                tagger.OutliningsTagsCache.AddTag(tag);
            }
         
            private void AddOutliningTagToAttribute(AttributeListSyntax attributeListNode)
            {
                if (!tagger.Provider.Package.UseBqlOutlining)
                    return;

                AttributeSyntax attribute = attributeListNode.ChildNodes()
                                                             .OfType<AttributeSyntax>()
                                                             .FirstOrDefault();

                if (attribute?.ArgumentList?.Arguments == null || attribute.ArgumentList.Arguments.Count == 0 || 
                    cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                string collapsedText = GetAttributeName(attribute);
                ITagSpan<IOutliningRegionTag> tag = attributeListNode.Span.ToOutliningTagSpan(tagger.Snapshot, collapsedText);
                tagger.OutliningsTagsCache.AddTag(tag);
            }

            private string GetAttributeName(AttributeSyntax attribute)
            {
                foreach (SyntaxNode childNode in attribute.ChildNodes())
                {
                    if (cancellationToken.IsCancellationRequested)
                        return null;

                    switch (childNode)
                    {
                        case IdentifierNameSyntax attributeName:
                            {
                                return $"[{attributeName.Identifier.ValueText}]";
                            }
                        case QualifiedNameSyntax qualifiedName:
                            {
                                string identifierText = tagger.Snapshot.GetText(qualifiedName.Span);
                                return !identifierText.IsNullOrWhiteSpace()
                                    ? $"[{identifierText}]"
                                    : null;
                            }
                    }
                }

                return null;
            }

            private void UpdateCodeEditorIfNecessary()
            {
                if (visitedNodesCounter <= ColoringConstants.ChunkSize || cancellationToken.IsCancellationRequested)
                    return;

                visitedNodesCounter = 0;               
                tagger.RaiseTagsChanged();
            }

            /// <summary>
            /// Check if node is var keyword
            /// </summary>
            /// <param name="nodeText">The node text.</param>
            /// <returns/>         
            private bool IsVar(string nodeText) => nodeText == varKeyword;           
        }
    }
}
