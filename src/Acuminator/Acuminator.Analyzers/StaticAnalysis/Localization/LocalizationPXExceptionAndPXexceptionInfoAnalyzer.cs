
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
	public class LocalizationPXExceptionAndPXExceptionInfoAnalyzer : PXDiagnosticAnalyzer
	{
		private static readonly string[] _pxExceptionMessageArgNames =  
		{ 
			"message", 
			"format" 
		};

		private static readonly string[] _pxExceptionInfoMessageArgNames =
		{
			"messageFormat"
		};

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1050_HardcodedStringInLocalizationMethod,
				Descriptors.PX1051_NonLocalizableString,
				Descriptors.PX1053_ConcatenationPriorLocalization
			);

		public LocalizationPXExceptionAndPXExceptionInfoAnalyzer() : base()
		{
		}

		/// <summary>
		/// Constructor for tests.
		/// </summary>
		/// <param name="codeAnalysisSettings">The code analysis settings.</param>
		public LocalizationPXExceptionAndPXExceptionInfoAnalyzer(CodeAnalysisSettings codeAnalysisSettings) : base(codeAnalysisSettings)
		{
		}

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzePXExceptionAndPXExceptionInfoConstructorInvocations(syntaxContext, pxContext),
															 SyntaxKind.ObjectCreationExpression);
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzePXExceptionChainedConstructorCall(syntaxContext, pxContext),
															 SyntaxKind.ClassDeclaration);
		}

		private void AnalyzePXExceptionAndPXExceptionInfoConstructorInvocations(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (syntaxContext.Node is not ObjectCreationExpressionSyntax constructorCall || constructorCall.ArgumentList?.Arguments.Count is null or 0)
				return;

			ITypeSymbol? createdObjectType = syntaxContext.SemanticModel.GetTypeInfo(constructorCall, syntaxContext.CancellationToken).Type;

			if (createdObjectType == null)
				return;

			bool isLocalizableException = IsLocalizableException(createdObjectType, pxContext);
			bool isPXExceptionInfo = pxContext.Exceptions.PXExceptionInfo != null 
				? createdObjectType.InheritsFromOrEquals(pxContext.Exceptions.PXExceptionInfo)
				: false;

			if (!isLocalizableException && !isPXExceptionInfo)
				return;

			var symbol = syntaxContext.SemanticModel.GetSymbolOrFirstCandidate(constructorCall, syntaxContext.CancellationToken);

			if (symbol is not IMethodSymbol constructor)
				return;

			var (parameterNamesWithLocalizableData, validationContext) = isLocalizableException
				? (_pxExceptionMessageArgNames, ValidationContext.PXExceptionConstructorCall)
				: (_pxExceptionInfoMessageArgNames, ValidationContext.PXExceptionInfoConstructorCall);
			ExpressionSyntax? messageExpression = GetMessageExpression(constructor, constructorCall.ArgumentList, parameterNamesWithLocalizableData);

			if (messageExpression == null)
				return;

			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			var messageValidator = new LocalizationMessageValidator(syntaxContext, pxContext, validationContext);
			messageValidator.ValidateMessage(messageExpression, isFormatMethod: false);
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
			LocalizationMessageValidator? messageValidator = null;

			foreach (ConstructorInitializerSyntax constructorCall in baseOrThisConstructorCalls)
			{
				var symbol = syntaxContext.SemanticModel.GetSymbolOrFirstCandidate(constructorCall, syntaxContext.CancellationToken);

				if (symbol is not IMethodSymbol constructorSymbol)
					continue;

				ExpressionSyntax? messageExpression = GetMessageExpression(constructorSymbol, constructorCall.ArgumentList, _pxExceptionMessageArgNames);

				if (messageExpression == null)
					continue;

				messageValidator ??= new LocalizationMessageValidator(syntaxContext, pxContext, ValidationContext.PXExceptionBaseOrThisConstructorCall);
				messageValidator.ValidateMessage(messageExpression, isFormatMethod: false);
			}
		}

		private ExpressionSyntax? GetMessageExpression(IMethodSymbol constructor, ArgumentListSyntax args, string[] parametersWithLocalizableText)
		{
			var constructorParameters = constructor.Parameters;

			if (constructorParameters.IsDefaultOrEmpty)
				return null;

			ArgumentsToParametersMapping? argumentsToParametersMapping = constructor.MapArgumentsToParameters(args);

			if (argumentsToParametersMapping == null)
				return null;

			for (int argIndex = 0; argIndex < args.Arguments.Count; argIndex++)
			{
				IParameterSymbol mappedParameter = argumentsToParametersMapping.Value.GetMappedParameter(constructor, argIndex);

				if (parametersWithLocalizableText.Contains(mappedParameter.Name, StringComparer.Ordinal))
				{
					var argument = args.Arguments[argIndex];
					return argument.Expression;
				}
			}

			return null;
		}

		private bool IsLocalizableException(ITypeSymbol type, PXContext pxContext) =>
			type.InheritsFromOrEquals(pxContext.Exceptions.PXException) &&
		   !type.InheritsFromOrEquals(pxContext.Exceptions.PXBaseRedirectException);
	}
}