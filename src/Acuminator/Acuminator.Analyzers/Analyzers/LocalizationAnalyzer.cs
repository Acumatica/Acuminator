using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LocalizationAnalyzer : PXDiagnosticAnalyzer
    {
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

            if (!IsLocalizationMethodInvocation(node, syntaxContext, pxContext, out ExpressionSyntax messageExpression))
                return;

            bool isHardcodedMessage = messageExpression is LiteralExpressionSyntax;
            if (isHardcodedMessage)
            {
                syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1050_HardcodedStringInLocalizationMethod, messageExpression.GetLocation()));
                return;
            }

            if (IsNonLocalizableMessage(messageExpression, syntaxContext, pxContext))
            {
                syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1051_NonLocalizableString, messageExpression.GetLocation()));
            }
        }

        private bool IsNonLocalizableMessage(ExpressionSyntax messageExpression, SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            if (syntaxContext.CancellationToken.IsCancellationRequested)
                return false;

            if (!(messageExpression is MemberAccessExpressionSyntax memberAccess) ||
                !(memberAccess.Name is IdentifierNameSyntax messageMember))
                return false;

            ITypeSymbol messageClassType = memberAccess.Expression
                                           .DescendantNodesAndSelf()
                                           .OfType<IdentifierNameSyntax>()
                                           .Select(i => syntaxContext.SemanticModel.GetTypeInfo(i, syntaxContext.CancellationToken).Type)
                                           .Where(t => t != null && t.IsReferenceType)
                                           .FirstOrDefault();
            if (messageClassType == null)
                return false;

            ISymbol messageMemberInfo = syntaxContext.SemanticModel.GetSymbolInfo(messageMember, syntaxContext.CancellationToken).Symbol;
            if (messageMemberInfo == null || messageMemberInfo.Kind != SymbolKind.Field || !messageMemberInfo.IsStatic ||
                messageMemberInfo.DeclaredAccessibility != Accessibility.Public)
                return false;

            ImmutableArray<AttributeData> attributes = messageClassType.GetAttributes();
            return attributes.All(a => !a.AttributeClass.Equals(pxContext.Localization.PXLocalizableAttribute));
        }

        private bool IsLocalizationMethodInvocation(InvocationExpressionSyntax node, SyntaxNodeAnalysisContext syntaxContext,
            PXContext pxContext, out ExpressionSyntax messageExpression)
        {
            messageExpression = null;

            if (syntaxContext.CancellationToken.IsCancellationRequested)
                return false;

            SymbolInfo symbolInfo = syntaxContext.SemanticModel.GetSymbolInfo(node, syntaxContext.CancellationToken);
            if (symbolInfo.Symbol == null && symbolInfo.CandidateSymbols.IsEmpty)
                return false;

            if (!IsLocalizationSymbol(symbolInfo, syntaxContext, pxContext))
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

        private bool IsLocalizationSymbol(SymbolInfo symbolInfo, SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            if (syntaxContext.CancellationToken.IsCancellationRequested)
                return false;

            bool isLocalization(ISymbol s) => pxContext.Localization.PXMessagesSimpleMethods.Contains(s) ||
                                              pxContext.Localization.PXMessagesFormatMethods.Contains(s) ||
                                              pxContext.Localization.PXLocalizerSimpleMethods.Contains(s) ||
                                              pxContext.Localization.PXLocalizerFormatMethods.Contains(s);
            if (symbolInfo.Symbol != null)
            {
                return isLocalization(symbolInfo.Symbol);
            }

            return symbolInfo.CandidateSymbols.Any(c => isLocalization(c));
        }
    }
}
