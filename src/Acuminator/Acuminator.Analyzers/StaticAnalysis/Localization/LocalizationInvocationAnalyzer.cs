using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Symbols;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Threading;

namespace Acuminator.Analyzers.StaticAnalysis.Localization
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LocalizationInvocationAnalyzer : PXDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create
            (
                Descriptors.PX1050_HardcodedStringInLocalizationMethod,
                Descriptors.PX1051_NonLocalizableString,
                Descriptors.PX1052_IncorrectStringToFormat,
                Descriptors.PX1053_ConcatenationPriorLocalization
            );

		public LocalizationInvocationAnalyzer() : base() { }

		/// <summary>
		/// Constructor accepting code analysis settings for tests.
		/// </summary>
		/// <param name="codeAnalysisSettings">The code analysis settings.</param>
		public LocalizationInvocationAnalyzer(CodeAnalysisSettings codeAnalysisSettings) : base(codeAnalysisSettings)
		{
		}

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeLocalizationMethodInvocation(syntaxContext, pxContext), SyntaxKind.InvocationExpression);
        }

        private void AnalyzeLocalizationMethodInvocation(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (!(syntaxContext.Node is InvocationExpressionSyntax invocationNode))
				return;

			SymbolInfo symbolInfo = syntaxContext.SemanticModel.GetSymbolInfo(invocationNode, syntaxContext.CancellationToken);
			var (isFormatMethod, isLocalizationMethod) = GetLocalizationMethodSymbolInfo(pxContext, symbolInfo);

			if (!isLocalizationMethod)
				return;

			ExpressionSyntax messageExpression = GetLocalizationMethodInvocationExpression(syntaxContext, invocationNode);

			if (messageExpression == null)
				return;

			LocalizationMessageHelper messageHelper = new LocalizationMessageHelper(syntaxContext, pxContext, messageExpression, isFormatMethod);
            messageHelper.ValidateMessage();
        }

        private ExpressionSyntax GetLocalizationMethodInvocationExpression(SyntaxNodeAnalysisContext syntaxContext, 
																		   InvocationExpressionSyntax invocationNode)
        {
            ArgumentSyntax messageArg = invocationNode.ArgumentList?.Arguments.FirstOrDefault();

            if (messageArg?.Expression == null)
                return null;

            ITypeSymbol messageArgType = syntaxContext.SemanticModel.GetTypeInfo(messageArg.Expression, syntaxContext.CancellationToken).Type;

            if (messageArgType == null || messageArgType.SpecialType != SpecialType.System_String)
                return null;

            return messageArg.Expression;
        }

        private (bool IsFormatMethod, bool IsLocalizationMethod) GetLocalizationMethodSymbolInfo(PXContext pxContext, SymbolInfo symbolInfo)
        {
			if (symbolInfo.Symbol == null && symbolInfo.CandidateSymbols.IsEmpty)
				return (IsFormatMethod: false, IsLocalizationMethod: false);

			if (symbolInfo.Symbol != null)
            {
				bool isFormatMethod = IsFormatMethodSymbol(pxContext, symbolInfo.Symbol);

				if (isFormatMethod)
					return (IsFormatMethod: true, IsLocalizationMethod: true);

				bool isNonFormatLocalizationMethod = IsLocalizationNonFormatMethodSymbol(pxContext, symbolInfo.Symbol);
				return (IsFormatMethod: false, IsLocalizationMethod: isNonFormatLocalizationMethod);
            }

            foreach (ISymbol candidate in symbolInfo.CandidateSymbols)
            {
				bool isFormatMethod = IsFormatMethodSymbol(pxContext, candidate);
				if (isFormatMethod)
					return (IsFormatMethod: true, IsLocalizationMethod: true);

				bool isNonFormatLocalizationMethod = IsLocalizationNonFormatMethodSymbol(pxContext, candidate);
				if (isNonFormatLocalizationMethod)
					return (IsFormatMethod: false, IsLocalizationMethod: true);
            }

            return (IsFormatMethod: false, IsLocalizationMethod: false);
        }

		private bool IsFormatMethodSymbol(PXContext pxContext, ISymbol symbol) =>
			pxContext.Localization.PXMessagesFormatMethods.Contains(symbol) || 
			pxContext.Localization.PXLocalizerFormatMethods.Contains(symbol);

		private bool IsLocalizationNonFormatMethodSymbol(PXContext pxContext, ISymbol symbol) =>
			 pxContext.Localization.PXMessagesSimpleMethods.Contains(symbol) ||
			 pxContext.Localization.PXLocalizerSimpleMethods.Contains(symbol);
    }
}
