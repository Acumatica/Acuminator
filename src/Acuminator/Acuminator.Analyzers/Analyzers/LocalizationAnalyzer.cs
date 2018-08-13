using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LocalizationAnalyzer : PXDiagnosticAnalyzer
    {
        private const string _pxMessagesClass = "PX.Data.PXMessages";
        private const string _pxLocalizerClass = "PX.Data.PXLocalizer";

        private readonly string[] _pxMessagesMethods = new[]
        {
            "Localize",
            "LocalizeNoPrefix",
            "LocalizeFormat",
            "LocalizeFormatNoPrefix",
            "LocalizeFormatNoPrefixNLA"
        };

        private readonly string[] _pxLocalizerMethods = new[]
        {
            "Localize",
            "LocalizeFormat"
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create
            (
                Descriptors.PX1050_HardcodedStringInLocalizationMethod,
                Descriptors.PX1051_NonLocalizableString,
                Descriptors.PX1052_IncorrectStringToFormat,
                Descriptors.PX1053_ConcatinationPriorLocalization,
                Descriptors.PX1054_HardcodedStringInPXExceptionConstructor
            );

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeLocalizationMethodInvocation(syntaxContext, pxContext), SyntaxKind.InvocationExpression);
        }

        private void AnalyzeLocalizationMethodInvocation(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            if (!(syntaxContext.Node is InvocationExpressionSyntax node) || syntaxContext.CancellationToken.IsCancellationRequested)
                return;

            if (!IsLocalizationMethodInvocation(node, syntaxContext, out ExpressionSyntax messageExpression))
                return;

            bool isHardcodedMessage = messageExpression is LiteralExpressionSyntax;
            if (isHardcodedMessage)
            {
                syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1050_HardcodedStringInLocalizationMethod, messageExpression.GetLocation()));
            }
        }

        private bool IsLocalizationMethodInvocation(InvocationExpressionSyntax node, SyntaxNodeAnalysisContext syntaxContext, out ExpressionSyntax messageExpression)
        {
            messageExpression = null;

            if (syntaxContext.CancellationToken.IsCancellationRequested)
                return false;

            SymbolInfo symbolInfo = syntaxContext.SemanticModel.GetSymbolInfo(node, syntaxContext.CancellationToken);
            if (symbolInfo.Symbol == null)
            {
                if (symbolInfo.CandidateSymbols.IsEmpty)
                    return false;

                //TODO: Workaround - to refactor after migration to Roslyn for VS2017
            }

            ISymbol symbol = syntaxContext.SemanticModel.GetSymbolInfo(node, syntaxContext.CancellationToken).Symbol;
            if (symbol == )

            if (symbol == null || symbol.Kind != SymbolKind.Method)
                return false;

            string typeName = symbol.ContainingType?.ToDisplayString();
            string methodName = symbol.Name;
            if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(methodName))
                return false;

            bool isMessagesMethod = _pxMessagesClass.Equals(typeName, StringComparison.Ordinal) &&
                                    _pxMessagesMethods.Any(m => m.Equals(methodName, StringComparison.Ordinal));
            bool isLocalizerMethod = _pxLocalizerClass.Equals(typeName, StringComparison.Ordinal) &&
                                     _pxLocalizerMethods.Any(m => m.Equals(methodName, StringComparison.Ordinal));
            if (!isMessagesMethod && !isLocalizerMethod)
                return false;

            ArgumentSyntax messageArg = node.ArgumentList?.Arguments.FirstOrDefault();
            if (messageArg == null || messageArg.Expression == null)
                return false;

            ITypeSymbol messageArgType = syntaxContext.SemanticModel.GetTypeInfo(messageArg.Expression).Type;
            if (messageArgType == null || messageArgType.SpecialType != SpecialType.System_String)
                return false;

            messageExpression = messageArg.Expression;
            return true;
        }
    }
}
