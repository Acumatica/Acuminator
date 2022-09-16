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
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzePXExceptionCtorInitializer(syntaxContext, pxContext),
															 SyntaxKind.ClassDeclaration);
		}

		private void AnalyzePXExceptionCtorInitializer(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (syntaxContext.Node is not ClassDeclarationSyntax classDeclaration)
				return;

			INamedTypeSymbol type = syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclaration, syntaxContext.CancellationToken);

			if (type == null || !IsLocalizableException(type, pxContext))
				return;

			var baseOrThisConstructorCalls = classDeclaration.DescendantNodes()
															 .OfType<ConstructorInitializerSyntax>()
															 .Where(constructorCall => constructorCall.ArgumentList != null);

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

			if (syntaxContext.Node is not ObjectCreationExpressionSyntax construtorCall || construtorCall.ArgumentList == null)
				return;

			ITypeSymbol? type = syntaxContext.SemanticModel.GetTypeInfo(construtorCall, syntaxContext.CancellationToken).Type;

			if (type == null || !IsLocalizableException(type, pxContext))
				return;

			var symbol = syntaxContext.SemanticModel.GetSymbolOrFirstCandidate(construtorCall, syntaxContext.CancellationToken);

			if (symbol is not IMethodSymbol constructor)
				return;

			syntaxContext.CancellationToken.ThrowIfCancellationRequested();
			ExpressionSyntax? messageExpression = GetMessageExpression(constructor, construtorCall.ArgumentList);

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