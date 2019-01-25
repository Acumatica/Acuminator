using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;


namespace Acuminator.Vsix.Coloriser
{
	public partial class PXRoslynColorizerTagger : PXColorizerTaggerBase
	{
		protected class PXColoriserSyntaxWalker : CSharpSyntaxWalker
		{
            private const string VarKeyword = "var";

            private long _visitedNodesCounter = 0;
            private readonly PXRoslynColorizerTagger _tagger;
			private readonly ParsedDocument _document;
            private int _braceLevel;
            private int _attributeLevel;                  
            private readonly CancellationToken _cancellationToken;

            private long _bqlDeepnessLevel;

            private bool IsInsideBqlCommand => _bqlDeepnessLevel > 0;

            public PXColoriserSyntaxWalker(PXRoslynColorizerTagger tagger, ParsedDocument parsedDocument, CancellationToken cToken) :
                                      base(SyntaxWalkerDepth.Node)
			{
				tagger.ThrowOnNull(nameof(tagger));
				parsedDocument.ThrowOnNull(nameof(parsedDocument));

				_tagger = tagger;
				_document = parsedDocument;
                _cancellationToken = cToken;
            }

            public async override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (_tagger.Provider.Package.ColorOnlyInsideBQL && !IsInsideBqlCommand)
                {
                    if (!_cancellationToken.IsCancellationRequested)
                        base.VisitIdentifierName(node);

                    return;
                }

                string nodeText = node.Identifier.ValueText;
                TextSpan span = node.Span;

                if (_cancellationToken.IsCancellationRequested || IsVar(nodeText))
                    return;

                ITypeSymbol typeSymbol = await GetTypeSymbolFromIdentifierNode(node);

                if (typeSymbol == null)
                {
                    if (!_cancellationToken.IsCancellationRequested)                    
                        base.VisitIdentifierName(node);                 

                    return;
                }

                if (_cancellationToken.IsCancellationRequested)
                    return;

				if (typeSymbol is ITypeParameterSymbol typeParameterSymbol)
				{
					AnalyzeTypeParameterNode(node, typeParameterSymbol);

					if (!_cancellationToken.IsCancellationRequested)
						base.VisitIdentifierName(node);

					return;
				}

				ColorIdentifierTypeSymbol(typeSymbol, span, isTypeParameter: false);

                if (!_cancellationToken.IsCancellationRequested)
                    base.VisitIdentifierName(node);

                UpdateCodeEditorIfNecessary();				
            }

            public async override void VisitGenericName(GenericNameSyntax genericNode)
            {
                if (_cancellationToken.IsCancellationRequested)
                    return;

				SemanticModel semanticModel = await _document.SemanticModelAsync(_cancellationToken);
                ITypeSymbol typeSymbol = semanticModel.GetSymbolInfo(genericNode).Symbol as ITypeSymbol;
              
                if (typeSymbol == null)
                {
                    if (!_cancellationToken.IsCancellationRequested)
                        base.VisitGenericName(genericNode);

                    return;
                }

                if (_cancellationToken.IsCancellationRequested)
                    return;

                if (genericNode.IsUnboundGenericName)
                {
                    typeSymbol = typeSymbol.OriginalDefinition;
                }

                PXCodeType? coloredCodeType = typeSymbol.GetCodeTypeFromGenericName();
                IClassificationType classificationType = coloredCodeType.HasValue 
                    ? _tagger.Provider[coloredCodeType.Value]
                    : null;

                if (classificationType == null)
                {
                    if (!_cancellationToken.IsCancellationRequested)
                        base.VisitGenericName(genericNode);

                    UpdateCodeEditorIfNecessary();
                    return;
                }

                if (coloredCodeType.Value == PXCodeType.BqlCommand)           
                    ColorAndOutlineBqlCommandBeginning(genericNode, classificationType);
                else
                    ColorAndOutlineBqlPartsAndPXActions(genericNode, typeSymbol, classificationType, coloredCodeType.Value);

                UpdateCodeEditorIfNecessary();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ColorAndOutlineBqlCommandBeginning(GenericNameSyntax genericNode, IClassificationType classificationType)
            {
                try
                {
                    _bqlDeepnessLevel++;
                    var typeArgumentList = genericNode.TypeArgumentList;

                    if (typeArgumentList.Arguments.Count > 1)
                    {
                        AddOutliningTagToBQL(typeArgumentList.Span);
                    }

                    AddClassificationTag(genericNode.Identifier.Span, classificationType);

                    if (!_cancellationToken.IsCancellationRequested)
                        base.VisitGenericName(genericNode);
                }
                finally
                {
                    _bqlDeepnessLevel--;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ColorAndOutlineBqlPartsAndPXActions(GenericNameSyntax genericNode, ITypeSymbol typeSymbol, 
															 IClassificationType classificationType, PXCodeType coloredCodeType)
            {
                if (_tagger.Provider.Package.ColorOnlyInsideBQL && !IsInsideBqlCommand)
                {
                    if (!_cancellationToken.IsCancellationRequested)
                        base.VisitGenericName(genericNode);

                    return;
                }
                                 
                switch (coloredCodeType)
                {
                    case PXCodeType.BqlOperator:
                        ColorAndOutlineBqlOperator(genericNode, typeSymbol, classificationType);
                        return;
                    case PXCodeType.BqlParameter:
                        ColorBqlParameter(genericNode, typeSymbol, classificationType);
                        return;
                    case PXCodeType.PXAction:
                        {
                            if (_tagger.Provider.Package.PXActionColoringEnabled)
                                AddClassificationTag(genericNode.Identifier.Span, classificationType);

                            if (!_cancellationToken.IsCancellationRequested)
                                base.VisitGenericName(genericNode);

                            return;
                        }
                }                           
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ColorAndOutlineBqlOperator(GenericNameSyntax genericNode, ITypeSymbol typeSymbol, IClassificationType classificationType)
            {
                if (_cancellationToken.IsCancellationRequested)
                    return;

                try
                {
                    _bqlDeepnessLevel++;

                    if (_tagger.Provider.Package.UseBqlOutlining && _tagger.Provider.Package.UseBqlDetailedOutlining)
                    {
                        TextSpan? outliningSpan = typeSymbol.GetBqlOperatorOutliningTextSpan(genericNode);

                        if (outliningSpan.HasValue)
                        {
                            AddOutliningTagToBQL(outliningSpan.Value);
                        }
                    }

                    AddClassificationTag(genericNode.Identifier.Span, classificationType);

                    if (!_cancellationToken.IsCancellationRequested)
                        base.VisitGenericName(genericNode);
                }
                finally
                {
                    _bqlDeepnessLevel--;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ColorBqlParameter(GenericNameSyntax genericNode, ITypeSymbol typeSymbol, IClassificationType classificationType)
            {
                try
                {
                    _bqlDeepnessLevel++;
                    AddClassificationTag(genericNode.Identifier.Span, classificationType);

                    if (!_cancellationToken.IsCancellationRequested)
                        base.VisitGenericName(genericNode);                
                }
                finally
                {
                    _bqlDeepnessLevel--;
                }
            }

            public async override void VisitQualifiedName(QualifiedNameSyntax node)
            {
                if (_cancellationToken.IsCancellationRequested)
                    return;

                if (_tagger.Provider.Package.ColorOnlyInsideBQL && !IsInsideBqlCommand)
                {
                    if (!_cancellationToken.IsCancellationRequested)
                        base.VisitQualifiedName(node);

                    return;
                }

				var semanticModel = await _document.SemanticModelAsync(_cancellationToken);

				string nodeText = node.ToString();
                TextSpan leftSpan = node.Left.Span;
                TextSpan rightSpan = node.Right.Span;    
                ITypeSymbol typeSymbol = semanticModel.GetSymbolInfo(node).Symbol as ITypeSymbol;

                if (typeSymbol == null)
                {
                    if (!_cancellationToken.IsCancellationRequested)
                        base.VisitQualifiedName(node);

                    return;
                }

                if (_cancellationToken.IsCancellationRequested)
                    return;

                if (typeSymbol.IsBqlConstant())
                {
                    AddClassificationTag(leftSpan, _tagger.Provider[PXCodeType.BQLConstantPrefix]);
                    AddClassificationTag(rightSpan, _tagger.Provider[PXCodeType.BQLConstantEnding]);
                }

                if (!_cancellationToken.IsCancellationRequested)
                    base.VisitQualifiedName(node);

                UpdateCodeEditorIfNecessary();
            }
          
            public override void VisitTypeArgumentList(TypeArgumentListSyntax node)
            {
                if (_cancellationToken.IsCancellationRequested)
                    return;

                if (_bqlDeepnessLevel == 0)
                {
                    if (!_cancellationToken.IsCancellationRequested)
                        base.VisitTypeArgumentList(node);

                    return;
                }

				IClassificationType braceClassificationType = _tagger.Provider[_braceLevel];

				try
				{				
					_braceLevel = (_braceLevel + 1) % ColoringConstants.MaxBraceLevel;

					if (braceClassificationType != null && !_cancellationToken.IsCancellationRequested)
					{
						AddClassificationTag(node.LessThanToken.Span, braceClassificationType);
						AddClassificationTag(node.GreaterThanToken.Span, braceClassificationType);
					}

					if (!_cancellationToken.IsCancellationRequested)
						base.VisitTypeArgumentList(node);
				}
				finally
				{
					_braceLevel = _braceLevel == 0
						? ColoringConstants.MaxBraceLevel - 1 
						: _braceLevel - 1;
				}

                UpdateCodeEditorIfNecessary();
            }

            public override void VisitAttributeList(AttributeListSyntax node)
            {
                if (_cancellationToken.IsCancellationRequested)
                    return;

                _attributeLevel++;

                try
                {
                    if (_attributeLevel <= OutliningConstants.MaxAttributeOutliningLevel)
                    {
                        AddOutliningTagToAttribute(node);
                    }

                    if (!_cancellationToken.IsCancellationRequested)
                    {
                        base.VisitAttributeList(node);
                    }
                }
                finally
                {
                    _attributeLevel--;
                }
            }

            public override void Visit(SyntaxNode node)
            {
                if (_cancellationToken.IsCancellationRequested)
                    return;

                if (_visitedNodesCounter < long.MaxValue)
                    _visitedNodesCounter++;

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

			private async Task<ITypeSymbol> GetTypeSymbolFromIdentifierNode(SyntaxNode node)
			{
				var semanticModel = await _document.SemanticModelAsync(_cancellationToken);
				var symbolInfo = semanticModel.GetSymbolInfo(node);

				ISymbol symbol = symbolInfo.Symbol;

				if (symbol == null && symbolInfo.CandidateSymbols.Length == 1)
				{
					symbol = symbolInfo.CandidateSymbols[0];
				}

				return symbol as ITypeSymbol;
			}

			private void AnalyzeTypeParameterNode(IdentifierNameSyntax node, ITypeParameterSymbol typeParameterSymbol)
			{
				if (typeParameterSymbol.ConstraintTypes.Length == 0)
					return;

				foreach (ITypeSymbol constraintType in typeParameterSymbol.ConstraintTypes)
				{
					ColorIdentifierTypeSymbol(constraintType, node.Span, isTypeParameter: true);
				}
			}

			private void ColorIdentifierTypeSymbol(ITypeSymbol typeSymbol, TextSpan span, bool isTypeParameter)
			{
				PXCodeType? coloredCodeType = typeSymbol.GetColoringTypeFromIdentifier(skipValidation: isTypeParameter, 
																							checkItself: isTypeParameter);
				IClassificationType classificationType = coloredCodeType.HasValue
					? _tagger.Provider[coloredCodeType.Value]
					: null;

				if (classificationType == null || 
				   (coloredCodeType.Value == PXCodeType.PXGraph && !_tagger.Provider.Package.PXGraphColoringEnabled) ||
				   (coloredCodeType.Value == PXCodeType.PXAction && !_tagger.Provider.Package.PXActionColoringEnabled))
					return;

				AddClassificationTag(span, classificationType);
			}

            private void AddClassificationTag(TextSpan span, IClassificationType classificationType)
            {
                ITagSpan<IClassificationTag> tag = span.ToClassificationTagSpan(_tagger.Snapshot, classificationType);
                _tagger.ClassificationTagsCache.AddTag(tag);
            }

            private void AddOutliningTagToBQL(TextSpan span)
            {
                if (!_tagger.Provider.Package.UseBqlOutlining)
                    return;

                ITagSpan<IOutliningRegionTag> tag = span.ToOutliningTagSpan(_tagger.Snapshot);
                _tagger.OutliningsTagsCache.AddTag(tag);
            }
         
            private void AddOutliningTagToAttribute(AttributeListSyntax attributeListNode)
            {
                if (!_tagger.Provider.Package.UseBqlOutlining || attributeListNode.Attributes.Count > 1)
                    return;

                AttributeSyntax attribute = attributeListNode.ChildNodes()
                                                             .OfType<AttributeSyntax>()
                                                             .FirstOrDefault();

                if (attribute?.ArgumentList?.Arguments == null || attribute.ArgumentList.Arguments.Count == 0 || 
                    _cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                string collapsedText = GetAttributeName(attribute);
                ITagSpan<IOutliningRegionTag> tag = attributeListNode.Span.ToOutliningTagSpan(_tagger.Snapshot, collapsedText);
                _tagger.OutliningsTagsCache.AddTag(tag);
            }

            private string GetAttributeName(AttributeSyntax attribute)
            {
                foreach (SyntaxNode childNode in attribute.ChildNodes())
                {
                    if (_cancellationToken.IsCancellationRequested)
                        return null;

                    switch (childNode)
                    {
                        case IdentifierNameSyntax attributeName:
                            {
                                return $"[{attributeName.Identifier.ValueText}]";
                            }
                        case QualifiedNameSyntax qualifiedName:
                            {
                                string identifierText = _tagger.Snapshot.GetText(qualifiedName.Span);
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
                if (_visitedNodesCounter <= ColoringConstants.ChunkSize || _cancellationToken.IsCancellationRequested)
                    return;

                _visitedNodesCounter = 0;               
                _tagger.RaiseTagsChanged();
            }

            /// <summary>
            /// Check if node is var keyword
            /// </summary>
            /// <param name="nodeText">The node text.</param>
            /// <returns/>         
            private bool IsVar(string nodeText) => nodeText == VarKeyword;           
        }
    }
}
