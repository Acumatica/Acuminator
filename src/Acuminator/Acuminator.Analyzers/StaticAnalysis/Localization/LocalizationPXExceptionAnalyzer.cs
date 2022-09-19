#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Utilities.Roslyn.Semantic.ArgumentsToParametersMapping;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.Localization
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class LocalizationPXExceptionAnalyzer : PXDiagnosticAnalyzer
	{
		private static readonly string[] _messageArgNames = new[] { "message", "format" };

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1050_HardcodedStringInLocalizationMethod,
				Descriptors.PX1051_NonLocalizableString,
				Descriptors.PX1053_ConcatenationPriorLocalization
			);

		public LocalizationPXExceptionAnalyzer() : base()
		{
		}

		/// <summary>
		/// Constructor for tests.
		/// </summary>
		/// <param name="codeAnalysisSettings">The code analysis settings.</param>
		public LocalizationPXExceptionAnalyzer(CodeAnalysisSettings codeAnalysisSettings) : base(codeAnalysisSettings)
		{
		}

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzePXExceptionConstructorInvocation(syntaxContext, pxContext),
															 SyntaxKind.ObjectCreationExpression);
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzePXExceptionChainedConstructorCall(syntaxContext, pxContext),
															 SyntaxKind.ClassDeclaration);
		}

		private void AnalyzePXExceptionChainedConstructorCall(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (syntaxContext.Node is not ClassDeclarationSyntax exceptionClassDeclaration)
				return;

			INamedTypeSymbol exceptionType = syntaxContext.SemanticModel.GetDeclaredSymbol(exceptionClassDeclaration, syntaxContext.CancellationToken);

			if (exceptionType == null || !IsLocalizableException(exceptionType, pxContext))
				return;

			var baseOrThisConstructorCalls = exceptionClassDeclaration.DescendantNodes()
																	  .OfType<ConstructorInitializerSyntax>()
																	  .Where(constructorCall => constructorCall.ArgumentList?.Arguments.Count is > 0);

			foreach (ConstructorInitializerSyntax constructorCall in baseOrThisConstructorCalls)
			{
				var symbol = syntaxContext.SemanticModel.GetSymbolOrFirstCandidate(constructorCall, syntaxContext.CancellationToken);

				if (symbol is not IMethodSymbol constructorSymbol)
					continue;

				ExpressionSyntax? messageExpression = GetMessageExpression(constructorSymbol, constructorCall.ArgumentList);

				if (messageExpression == null)
					continue;

				var messageHelper = new LocalizationMessageHelper(syntaxContext, pxContext, messageExpression, isFormatMethod: false);
				messageHelper.ValidateMessage();
			}
		}

		private ExpressionSyntax? GetMessageExpression(IMethodSymbol constructor, ArgumentListSyntax args)
		{
			var constructorParameters = constructor.Parameters;

			if (constructorParameters.IsDefaultOrEmpty)
				return null;

			ArgumentSyntax? messageArg = null;

			foreach (ArgumentSyntax argument in args.Arguments)
			{
				IParameterSymbol? parameter = argument.DetermineParameter(constructorParameters, allowParams: false);

				if (_messageArgNames.Contains(parameter?.Name, StringComparer.Ordinal))
				{
					messageArg = argument;
					break;
				}
			}

			if (messageArg?.Expression == null)
				return null;

			return messageArg.Expression;
		}

		private void AnalyzePXExceptionConstructorInvocation(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (syntaxContext.Node is not ObjectCreationExpressionSyntax constructorCall || constructorCall.ArgumentList?.Arguments.Count is null or 0)
				return;

			ITypeSymbol? exceptionType = syntaxContext.SemanticModel.GetTypeInfo(constructorCall, syntaxContext.CancellationToken).Type;

			if (exceptionType == null || !IsLocalizableException(exceptionType, pxContext))
				return;

			var symbol = syntaxContext.SemanticModel.GetSymbolOrFirstCandidate(constructorCall, syntaxContext.CancellationToken);

			if (symbol is not IMethodSymbol constructor)
				return;

			syntaxContext.CancellationToken.ThrowIfCancellationRequested();
			ExpressionSyntax? messageExpression = GetMessageExpression(constructor, constructorCall.ArgumentList);

			if (messageExpression == null)
				return;

			var messageHelper = new LocalizationMessageHelper(syntaxContext, pxContext, messageExpression, isFormatMethod: false);
			messageHelper.ValidateMessage();
		}

		private bool IsLocalizableException(ITypeSymbol type, PXContext pxContext) =>
			type.InheritsFromOrEquals(pxContext.Exceptions.PXException) &&
		   !type.InheritsFromOrEquals(pxContext.Exceptions.PXBaseRedirectException);
	}
}