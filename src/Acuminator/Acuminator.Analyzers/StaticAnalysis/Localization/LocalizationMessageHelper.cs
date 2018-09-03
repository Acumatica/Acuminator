using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Acuminator.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.Localization
{
    internal class LocalizationMessageHelper
    {
        private const string _formatRegexString = @"(?<Par>{.+})";
        private static readonly Regex _formatRegex = new Regex(_formatRegexString, RegexOptions.CultureInvariant);
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
                _syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1053_ConcatenationPriorLocalization, _messageExpression.GetLocation()));
            }
        }

        private ITypeSymbol ReadMessageInfo()
        {
            Cancellation.ThrowIfCancellationRequested();

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

            IFieldSymbol messageMemberInfo = SemanticModel.GetSymbolInfo(messageMember, Cancellation).Symbol as IFieldSymbol;
            if (messageMemberInfo == null || !messageMemberInfo.IsConst || messageMemberInfo.DeclaredAccessibility != Accessibility.Public)
                return null;

            _messageMember = messageMemberInfo;
            return messageClassType;
        }

        private bool IsNonLocalizableMessageType(ITypeSymbol messageType)
        {
            Cancellation.ThrowIfCancellationRequested();

            ImmutableArray<AttributeData> attributes = messageType.GetAttributes();
            return attributes.All(a => !a.AttributeClass.Equals(Localization.PXLocalizableAttribute));
        }

        private bool IsIncorrectStringToFormat()
        {
            Cancellation.ThrowIfCancellationRequested();

            if (!_isFormatMethod)
                return false;

            Optional<object> constString = SemanticModel.GetConstantValue(_messageExpression, Cancellation);
            if (!constString.HasValue)
                return false;

            MatchCollection matchesValue = _formatRegex.Matches(constString.Value as string);

            return matchesValue.Count == 0;
        }

        private bool IsStringConcatenation()
        {
            Cancellation.ThrowIfCancellationRequested();

            bool isStringAddition = _messageExpression is BinaryExpressionSyntax && _messageExpression.IsKind(SyntaxKind.AddExpression);
            if (isStringAddition)
                return true;

            if (!(_messageExpression is InvocationExpressionSyntax i) || !(i.Expression is MemberAccessExpressionSyntax memberAccess) ||
                memberAccess.Expression == null || memberAccess.Name == null)
                return false;

            ITypeSymbol stringType = SemanticModel.GetTypeInfo(memberAccess.Expression, Cancellation).Type;
            if (stringType == null || stringType.SpecialType != SpecialType.System_String)
                return false;

            ISymbol stringMethod = SemanticModel.GetSymbolInfo(memberAccess.Name, Cancellation).Symbol;
            if (stringMethod == null || (!_pxContext.StringFormat.Contains(stringMethod) && !_pxContext.StringConcat.Contains(stringMethod)))
                return false;

            return true;
        }
    }
}
