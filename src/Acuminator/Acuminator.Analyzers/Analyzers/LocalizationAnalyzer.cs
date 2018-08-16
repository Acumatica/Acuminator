using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Acuminator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LocalizationAnalyzer : PXDiagnosticAnalyzer
    {
        private const string _formatRegExp = @"(?<Par>{(\w+|:+)})";
        private SyntaxNodeAnalysisContext _syntaxContext;
        private PXContext _pxContext;
        private InvocationExpressionSyntax _invocationNode;
        private bool _isFormatMethod;
        private ExpressionSyntax _messageExpression;
        ITypeSymbol _messageType;
        ISymbol _messageMember;

        private SemanticModel SemanticModel => _syntaxContext.SemanticModel;
        private CancellationToken Cancellation => _syntaxContext.CancellationToken;
        private LocalizationTypes Localization => _pxContext.Localization;

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
            _invocationNode = syntaxContext.Node as InvocationExpressionSyntax;
            if (_invocationNode == null || syntaxContext.CancellationToken.IsCancellationRequested)
                return;

            _syntaxContext = syntaxContext;
            _pxContext = pxContext;

            if (!IsLocalizationMethodInvocation())
                return;

            bool isHardcodedMessage = _messageExpression is LiteralExpressionSyntax;
            if (isHardcodedMessage)
            {
                syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1050_HardcodedStringInLocalizationMethod, _messageExpression.GetLocation()));
                return;
            }

            ReadMessageInfo();
            if (_messageType == null || _messageMember == null)
                return;

            if (IsNonLocalizableMessageType())
            {
                syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1051_NonLocalizableString, _messageExpression.GetLocation()));
            }

            if (IsIncorrectStringToFormat())
            {
                syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1052_IncorrectStringToFormat, _messageExpression.GetLocation()));
            }
        }

        private bool IsIncorrectStringToFormat()
        {
            if (!_isFormatMethod || Cancellation.IsCancellationRequested)
                return false;

            Optional<object> constString = SemanticModel.GetConstantValue(_messageExpression);
            if (!constString.HasValue)
                return false;

            Regex regex = new Regex(_formatRegExp, RegexOptions.CultureInvariant);
            MatchCollection matchesValue = regex.Matches(constString.Value as string);

            return matchesValue.Count == 0;
        }

        private void ReadMessageInfo()
        {
            if (Cancellation.IsCancellationRequested)
                return;

            if (!(_messageExpression is MemberAccessExpressionSyntax memberAccess) ||
                !(memberAccess.Name is IdentifierNameSyntax messageMember))
                return;
            
            ITypeSymbol messageClassType = memberAccess.Expression
                                           .DescendantNodesAndSelf()
                                           .OfType<IdentifierNameSyntax>()
                                           .Select(i => SemanticModel.GetTypeInfo(i, Cancellation).Type)
                                           .Where(t => t != null && t.IsReferenceType)
                                           .FirstOrDefault();
            if (messageClassType == null)
                return;

            ISymbol messageMemberInfo = SemanticModel.GetSymbolInfo(messageMember, Cancellation).Symbol;
            if (messageMemberInfo == null || messageMemberInfo.Kind != SymbolKind.Field || !messageMemberInfo.IsStatic ||
                messageMemberInfo.DeclaredAccessibility != Accessibility.Public)
                return;

            _messageType = messageClassType;
            _messageMember = messageMemberInfo;
        }

        private bool IsNonLocalizableMessageType()
        {
            if (Cancellation.IsCancellationRequested)
                return false;

            ImmutableArray<AttributeData> attributes = _messageType.GetAttributes();
            return attributes.All(a => !a.AttributeClass.Equals(Localization.PXLocalizableAttribute));
        }

        private bool IsLocalizationMethodInvocation()
        {
            if (Cancellation.IsCancellationRequested)
                return false;

            SymbolInfo symbolInfo = SemanticModel.GetSymbolInfo(_invocationNode, Cancellation);
            if (symbolInfo.Symbol == null && symbolInfo.CandidateSymbols.IsEmpty)
                return false;

            if (!IsLocalizationSymbolInfo(symbolInfo))
                return false;

            ArgumentSyntax messageArg = _invocationNode.ArgumentList?.Arguments.FirstOrDefault();
            if (messageArg == null || messageArg.Expression == null)
                return false;

            ITypeSymbol messageArgType = SemanticModel.GetTypeInfo(messageArg.Expression).Type;
            if (messageArgType == null || messageArgType.SpecialType != SpecialType.System_String)
                return false;

            _messageExpression = messageArg.Expression;
            return true;
        }

        private bool IsLocalizationSymbolInfo(SymbolInfo symbolInfo)
        {
            if (Cancellation.IsCancellationRequested)
                return false;

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
            if (Cancellation.IsCancellationRequested)
                return false;

            _isFormatMethod = Localization.PXMessagesFormatMethods.Contains(s) ||
                              Localization.PXLocalizerFormatMethods.Contains(s);
            if (_isFormatMethod)
                return true;

            return Localization.PXMessagesSimpleMethods.Contains(s) ||
                   Localization.PXLocalizerSimpleMethods.Contains(s);
        }
    }
}
