#nullable enable

using System.Collections.Immutable;

using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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

			if (syntaxContext.Node is not InvocationExpressionSyntax invocationNode)
				return;

			SymbolInfo symbolInfo = syntaxContext.SemanticModel.GetSymbolInfo(invocationNode, syntaxContext.CancellationToken);
			var (isFormatMethod, isLocalizationMethod) = GetLocalizationMethodInfoFromSymbolInfo(pxContext, symbolInfo);

			if (!isLocalizationMethod)
				return;

			ExpressionSyntax? messageExpression = GetLocalizationMethodInvocationExpression(syntaxContext, invocationNode);

			if (messageExpression == null)
				return;

			var messageHelper = new LocalizationMessageHelper(syntaxContext, pxContext, messageExpression, isFormatMethod);
            messageHelper.ValidateMessage();
        }

        private ExpressionSyntax? GetLocalizationMethodInvocationExpression(SyntaxNodeAnalysisContext syntaxContext, 
																		   InvocationExpressionSyntax invocationNode)
        {
            ArgumentSyntax? messageArg = invocationNode.ArgumentList?.Arguments.FirstOrDefault();

            if (messageArg?.Expression == null)
                return null;

            ITypeSymbol messageArgType = syntaxContext.SemanticModel.GetTypeInfo(messageArg.Expression, syntaxContext.CancellationToken).Type;

            if (messageArgType == null || messageArgType.SpecialType != SpecialType.System_String)
                return null;

            return messageArg.Expression;
        }

        private (bool IsFormatMethod, bool IsLocalizationMethod) GetLocalizationMethodInfoFromSymbolInfo(PXContext pxContext, SymbolInfo symbolInfo)
        {
			if (symbolInfo.Symbol == null && symbolInfo.CandidateSymbols.IsEmpty)
				return (IsFormatMethod: false, IsLocalizationMethod: false);

			if (symbolInfo.Symbol is IMethodSymbol method)
				return GetLocalizationMethodInfoFromSymbol(method, pxContext);

			if (!symbolInfo.CandidateSymbols.IsDefaultOrEmpty)
			{
				foreach (IMethodSymbol candidate in symbolInfo.CandidateSymbols.OfType<IMethodSymbol>())
				{
					var (isFormatMethod, isLocalizationMethod) = GetLocalizationMethodInfoFromSymbol(candidate, pxContext);

					if (isLocalizationMethod)
						return (isFormatMethod, isLocalizationMethod);
				}
			}

            return (IsFormatMethod: false, IsLocalizationMethod: false);
        }

		private (bool IsFormatMethod, bool IsLocalizationMethod) GetLocalizationMethodInfoFromSymbol(IMethodSymbol method, PXContext pxContext)
		{
			if (IsFormatMethodSymbol(method, pxContext))
				return (IsFormatMethod: true, IsLocalizationMethod: true);

			return (IsFormatMethod: false, IsLocalizationMethod: IsLocalizationNonFormatMethodSymbol(method, pxContext));
		}

		private bool IsFormatMethodSymbol(IMethodSymbol method, PXContext pxContext) =>
			pxContext.Localization.PXMessagesFormatMethods.Contains(method) || 
			pxContext.Localization.PXLocalizerFormatMethods.Contains(method);

		private bool IsLocalizationNonFormatMethodSymbol(IMethodSymbol method, PXContext pxContext) =>
			 pxContext.Localization.PXMessagesSimpleMethods.Contains(method) ||
			 pxContext.Localization.PXLocalizerSimpleMethods.Contains(method);
    }
}
