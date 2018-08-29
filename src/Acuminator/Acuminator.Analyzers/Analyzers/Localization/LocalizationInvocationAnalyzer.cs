using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Threading;

namespace Acuminator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LocalizationInvocationAnalyzer : PXDiagnosticAnalyzer
    {
        private SyntaxNodeAnalysisContext _syntaxContext;
        private PXContext _pxContext;
        private InvocationExpressionSyntax _invocationNode;
        private bool _isFormatMethod;
        private ExpressionSyntax _messageExpression;

        private SemanticModel SemanticModel => _syntaxContext.SemanticModel;
        private CancellationToken Cancellation => _syntaxContext.CancellationToken;
        private LocalizationTypes Localization => _pxContext.Localization;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create
            (
                Descriptors.PX1050_HardcodedStringInLocalizationMethod,
                Descriptors.PX1051_NonLocalizableString,
                Descriptors.PX1052_IncorrectStringToFormat,
                Descriptors.PX1053_ConcatenationPriorLocalization
            );

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeLocalizationMethodInvocation(syntaxContext, pxContext), SyntaxKind.InvocationExpression);
        }

        private void AnalyzeLocalizationMethodInvocation(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            Cancellation.ThrowIfCancellationRequested();

            _invocationNode = syntaxContext.Node as InvocationExpressionSyntax;
            if (_invocationNode == null)
                return;

            _syntaxContext = syntaxContext;
            _pxContext = pxContext;

            if (!IsLocalizationMethodInvocation())
                return;

            LocalizationMessageHelper messageHelper = new LocalizationMessageHelper(syntaxContext, pxContext, _messageExpression, _isFormatMethod);
            messageHelper.ValidateMessage();
        }

        private bool IsLocalizationMethodInvocation()
        {
            Cancellation.ThrowIfCancellationRequested();

            SymbolInfo symbolInfo = SemanticModel.GetSymbolInfo(_invocationNode, Cancellation);
            if (symbolInfo.Symbol == null && symbolInfo.CandidateSymbols.IsEmpty)
                return false;

            if (!IsLocalizationSymbolInfo(symbolInfo))
                return false;

            ArgumentSyntax messageArg = _invocationNode.ArgumentList?.Arguments.FirstOrDefault();
            if (messageArg == null || messageArg.Expression == null)
                return false;

            ITypeSymbol messageArgType = SemanticModel.GetTypeInfo(messageArg.Expression, Cancellation).Type;
            if (messageArgType == null || messageArgType.SpecialType != SpecialType.System_String)
                return false;

            _messageExpression = messageArg.Expression;
            return true;
        }

        private bool IsLocalizationSymbolInfo(SymbolInfo symbolInfo)
        {
            Cancellation.ThrowIfCancellationRequested();

            if (symbolInfo.Symbol != null)
            {
                return IsLocalizationSymbol(symbolInfo.Symbol);
            }

            foreach (ISymbol s in symbolInfo.CandidateSymbols)
            {
                if (IsLocalizationSymbol(s))
                    return true;
            }

            return false;
        }

        private bool IsLocalizationSymbol(ISymbol s)
        {
            Cancellation.ThrowIfCancellationRequested();

            _isFormatMethod = Localization.PXMessagesFormatMethods.Contains(s) ||
                              Localization.PXLocalizerFormatMethods.Contains(s);
            if (_isFormatMethod)
                return true;

            return Localization.PXMessagesSimpleMethods.Contains(s) ||
                   Localization.PXLocalizerSimpleMethods.Contains(s);
        }
    }
}
