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
    internal class LocalizationMessageHelper
    {
        private const string _formatRegExp = @"(?<Par>{(\w+|:+)})";
        private readonly SyntaxNodeAnalysisContext _syntaxContext;
        private readonly PXContext _pxContext;
        private readonly ExpressionSyntax _messageExpression;
        private readonly bool _isFormatMethod;
        private ISymbol _messageMember;

        private SemanticModel SemanticModel => _syntaxContext.SemanticModel;
        private CancellationToken Cancellation => _syntaxContext.CancellationToken;
        private LocalizationTypes Localization => _pxContext.Localization;

        public LocalizationMessageHelper(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, ExpressionSyntax messageExpression, bool isFormatMethod)
        {
            _syntaxContext = syntaxContext;
            _pxContext = pxContext;
            _messageExpression = messageExpression;
            _isFormatMethod = isFormatMethod;
        }

        public void ValidateMessage()
        {
            bool isHardcodedMessage = _messageExpression is LiteralExpressionSyntax;
            if (isHardcodedMessage)
            {
                _syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1050_HardcodedStringInLocalizationMethod, _messageExpression.GetLocation()));
                return;
            }

            ITypeSymbol messageType = ReadMessageInfo();
            if (messageType != null && _messageMember != null)
            {
                if (IsNonLocalizableMessageType(messageType))
                {
                    _syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1051_NonLocalizableString, _messageExpression.GetLocation()));
                }

                if (IsIncorrectStringToFormat())
                {
                    _syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1052_IncorrectStringToFormat, _messageExpression.GetLocation()));
                }
            }
            else if (IsStringConcatenation())
            {
                _syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1053_ConcatinationPriorLocalization, _messageExpression.GetLocation()));
            }
        }

        private ITypeSymbol ReadMessageInfo()
        {
            if (Cancellation.IsCancellationRequested)
                return null;

            if (!(_messageExpression is MemberAccessExpressionSyntax memberAccess) ||
                !(memberAccess.Name is IdentifierNameSyntax messageMember))
                return null;

            ITypeSymbol messageClassType = memberAccess.Expression
                                           .DescendantNodesAndSelf()
                                           .OfType<IdentifierNameSyntax>()
                                           .Select(i => SemanticModel.GetTypeInfo(i, Cancellation).Type)
                                           .Where(t => t != null && t.IsReferenceType)
                                           .FirstOrDefault();
            if (messageClassType == null)
                return null;

            ISymbol messageMemberInfo = SemanticModel.GetSymbolInfo(messageMember, Cancellation).Symbol;
            if (messageMemberInfo == null || messageMemberInfo.Kind != SymbolKind.Field || !messageMemberInfo.IsStatic ||
                messageMemberInfo.DeclaredAccessibility != Accessibility.Public)
                return null;

            _messageMember = messageMemberInfo;
            return messageClassType;
        }

        private bool IsNonLocalizableMessageType(ITypeSymbol messageType)
        {
            if (Cancellation.IsCancellationRequested)
                return false;

            ImmutableArray<AttributeData> attributes = messageType.GetAttributes();
            return attributes.All(a => !a.AttributeClass.Equals(Localization.PXLocalizableAttribute));
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

        private bool IsStringConcatenation()
        {
            if (Cancellation.IsCancellationRequested)
                return false;

            bool isStringAddition = _messageExpression is BinaryExpressionSyntax && _messageExpression.IsKind(SyntaxKind.AddExpression);
            if (isStringAddition)
                return true;

            if (!(_messageExpression is InvocationExpressionSyntax i) || !(i.Expression is MemberAccessExpressionSyntax memberAccess) ||
                !(memberAccess.Expression is PredefinedTypeSyntax predefinedType) || memberAccess.Name == null)
                return false;

            ITypeSymbol stringType = SemanticModel.GetTypeInfo(predefinedType, Cancellation).Type;
            if (stringType == null || stringType.SpecialType != SpecialType.System_String)
                return false;

            ISymbol stringMethod = SemanticModel.GetSymbolInfo(memberAccess.Name, Cancellation).Symbol;
            if (stringMethod == null || (!_pxContext.StringFormat.Contains(stringMethod) && !_pxContext.StringConcat.Contains(stringMethod)))
                return false;

            return true;
        }
    }
}
