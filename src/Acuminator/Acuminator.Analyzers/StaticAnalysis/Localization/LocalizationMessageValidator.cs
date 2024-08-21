﻿
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Symbols;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.Localization
{
	internal enum ValidationContext
	{
		LocalizationMethodCall,
		PXExceptionConstructorCall,
		PXExceptionInfoConstructorCall,
		PXExceptionBaseOrThisConstructorCall
	}

	internal class LocalizationMessageValidator
	{
		private static readonly Regex _formatRegex = new Regex(@"(?<Par>{.+})", RegexOptions.CultureInvariant | RegexOptions.Compiled);

		private readonly SyntaxNodeAnalysisContext _syntaxContext;
		private readonly PXContext _pxContext;
		private readonly ValidationContext _validationContext;

		private SemanticModel SemanticModel => _syntaxContext.SemanticModel;

		private CancellationToken Cancellation => _syntaxContext.CancellationToken;

		public LocalizationMessageValidator(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext,
											ValidationContext validationContext)
		{
			_syntaxContext = syntaxContext;
			_pxContext = pxContext.CheckIfNull();
			_validationContext = validationContext;	
		}

		public void ValidateMessage(ExpressionSyntax? messageExpression, bool isFormatMethod)
		{
			if (messageExpression == null)
				return;

			Cancellation.ThrowIfCancellationRequested();

			if (!CheckThatMessageIsNotHardCodedString(messageExpression))
				return;

			ISymbol? messageSymbolWithStringType = GetMessageSymbol(messageExpression);

			if (messageSymbolWithStringType == null || messageSymbolWithStringType is IMethodSymbol)
			{
				if (IsStringConcatenation(messageExpression, messageSymbolWithStringType))
				{
					_syntaxContext.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1053_ConcatenationPriorLocalization, messageExpression.GetLocation()),
						_pxContext.CodeAnalysisSettings);
				}
				else
					CheckThatMessageIsNotMethodCallPassedToBaseConstructor(messageSymbolWithStringType, messageExpression);

				return;
			}

			if (messageSymbolWithStringType.ContainingType == null || IsAllowedMessageSymbol(messageSymbolWithStringType))
				return;

			Cancellation.ThrowIfCancellationRequested();

			if (!IsLocalizableMessagesContainer(messageSymbolWithStringType.ContainingType))
			{
				_syntaxContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1051_NonLocalizableString, messageExpression.GetLocation()),
					_pxContext.CodeAnalysisSettings);
			}

			Cancellation.ThrowIfCancellationRequested();

			if (!CheckThatMessageSymbolIsFieldWithConstant(messageExpression, messageSymbolWithStringType))
				return;

			if (IsIncorrectStringToFormat(messageExpression, isFormatMethod))
			{
				_syntaxContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1052_IncorrectStringToFormat, messageExpression.GetLocation()),
					_pxContext.CodeAnalysisSettings);
			}
		}

		private bool CheckThatMessageIsNotHardCodedString(ExpressionSyntax messageExpression)
		{
			if (messageExpression is LiteralExpressionSyntax or InterpolatedStringExpressionSyntax)
			{
				_syntaxContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1050_HardcodedStringInLocalizationMethod, messageExpression.GetLocation()),
					_pxContext.CodeAnalysisSettings);

				return false;
			}

			return true;
		}

		private ISymbol? GetMessageSymbol(ExpressionSyntax messageExpression)
		{
			var messageSymbolInfo = SemanticModel.GetSymbolInfo(messageExpression, Cancellation);

			if (messageSymbolInfo.Symbol != null)
			{
				return SymbolHasStringType(messageSymbolInfo.Symbol)
					? messageSymbolInfo.Symbol
					: null;
			}

			if (messageSymbolInfo.CandidateSymbols.IsDefaultOrEmpty)
				return null;

			return messageSymbolInfo.CandidateSymbols.Where(SymbolHasStringType).FirstOrDefault();
		}

		private bool SymbolHasStringType(ISymbol? symbol) =>
			symbol switch
			{
				IFieldSymbol { Type: { SpecialType: SpecialType.System_String } } => true,
				IPropertySymbol { Type: { SpecialType: SpecialType.System_String } } => true,
				IMethodSymbol { ReturnType: { SpecialType: SpecialType.System_String } } => true,
				_ => false
			};

		private bool IsStringConcatenation(ExpressionSyntax messageExpression, ISymbol? messageSymbolWithStringType)
		{
			Cancellation.ThrowIfCancellationRequested();

			bool isStringAddition = messageExpression is BinaryExpressionSyntax && messageExpression.IsKind(SyntaxKind.AddExpression);
			if (isStringAddition)
				return true;

			if (messageExpression is not InvocationExpressionSyntax invocation ||
				invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
				memberAccess.Expression == null || memberAccess.Name == null)
			{
				return false;
			}

			return messageSymbolWithStringType != null &&
				(_pxContext.SystemTypes.String.StringFormat.Contains(messageSymbolWithStringType) ||
				 _pxContext.SystemTypes.String.StringConcat.Contains(messageSymbolWithStringType));
		}

		private void CheckThatMessageIsNotMethodCallPassedToBaseConstructor(ISymbol? messageSymbolWithStringType, ExpressionSyntax messageExpression)
		{
			if (messageSymbolWithStringType is IMethodSymbol method &&
				_validationContext == ValidationContext.PXExceptionBaseOrThisConstructorCall)
			{
				var location = messageExpression.GetLocation();

				_syntaxContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1051_NonLocalizableString, location),
					_pxContext.CodeAnalysisSettings);

				_syntaxContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1050_NonConstFieldStringInLocalizationMethod, location),
					_pxContext.CodeAnalysisSettings);
			}
		}

		private bool IsAllowedMessageSymbol(ISymbol messageSymbolWithStringType)
		{
			if (messageSymbolWithStringType is not IPropertySymbol messageProperty)
				return false;

			return IsPXExceptionMessageProperty(messageProperty) || IsPXExceptionInfoMessageFormatProperty(messageProperty);
		}

		private bool IsPXExceptionMessageProperty(IPropertySymbol messageProperty)
		{
			if (messageProperty.Name != PropertyNames.Exception.Message ||
				_pxContext.Exceptions.PXException == null || _pxContext.Exceptions.PXException_Message == null)
			{
				return false;
			}

			return messageProperty.ContainingType.InheritsFromOrEquals(_pxContext.Exceptions.PXException) &&
				   messageProperty.GetOverriddenAndThis()
								  .Contains(_pxContext.Exceptions.PXException_Message);
		}

		private bool IsPXExceptionInfoMessageFormatProperty(IPropertySymbol messageProperty)
		{
			if (messageProperty.Name != PropertyNames.Exception.MessageFormat ||
				_pxContext.Exceptions.PXExceptionInfo == null || _pxContext.Exceptions.PXExceptionInfo_MessageFormat == null)
			{
				return false;
			}

			return messageProperty.ContainingType.InheritsFromOrEquals(_pxContext.Exceptions.PXExceptionInfo) &&
				   messageProperty.Equals(_pxContext.Exceptions.PXExceptionInfo_MessageFormat);
		}

		private bool IsLocalizableMessagesContainer(ITypeSymbol messageContainerType)
		{
			ImmutableArray<AttributeData> attributes = messageContainerType.GetAttributes();
			return attributes.IsDefaultOrEmpty
				? false
				: attributes.Any(a => a.AttributeClass.Equals(_pxContext.Localization.PXLocalizableAttribute));
		}

		private bool CheckThatMessageSymbolIsFieldWithConstant(ExpressionSyntax messageExpression, ISymbol messageSymbolWithStringType)
		{
			if (messageSymbolWithStringType is not IFieldSymbol field || !field.IsConst)
			{
				_syntaxContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1050_NonConstFieldStringInLocalizationMethod, messageExpression.GetLocation()),
					_pxContext.CodeAnalysisSettings);

				return false;
			}

			return true;
		}

		private bool IsIncorrectStringToFormat(ExpressionSyntax messageExpression, bool isFormatMethod)
		{
			if (!isFormatMethod)
				return false;

			Optional<object> constString = SemanticModel.GetConstantValue(messageExpression, Cancellation);

			if (!constString.HasValue || constString.Value is not string stringConstant)
				return false;

			MatchCollection matchesValue = _formatRegex.Matches(stringConstant);

			return matchesValue.Count == 0;
		}
	}
}
