﻿
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

using Acuminator.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.BqlParameterMismatch
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public partial class BqlParameterMismatchAnalyzer : PXDiagnosticAnalyzer
	{
		private const string SearchMethodName = "Search";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams,
								  Descriptors.PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams);

		public BqlParameterMismatchAnalyzer() : this(null)
		{ }

		public BqlParameterMismatchAnalyzer(CodeAnalysisSettings? codeAnalysisSettings) : base(codeAnalysisSettings)
		{ }

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(c => AnalyzeNode(c, pxContext), SyntaxKind.ClassDeclaration);
		}

		private static void AnalyzeNode(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (syntaxContext.Node is not ClassDeclarationSyntax classDeclaration || classDeclaration.Members.Count == 0)
				return;

			var invocationNodes = classDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();

			foreach (InvocationExpressionSyntax invocationNode in invocationNodes)
			{
				var symbolInfo = syntaxContext.SemanticModel.GetSymbolInfo(invocationNode, syntaxContext.CancellationToken);

				if (symbolInfo.Symbol is not IMethodSymbol methodSymbol || !IsValidMethodGeneralCheck(methodSymbol, pxContext))
					continue;

				if (methodSymbol.IsStatic)
					AnalyzeStaticInvocation(methodSymbol, pxContext, syntaxContext, invocationNode);
				else
					AnalyzeInstanceInvocation(methodSymbol, pxContext, syntaxContext, invocationNode);
			}		
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsValidMethodGeneralCheck(IMethodSymbol methodSymbol, PXContext pxContext)
		{
			if (methodSymbol.IsExtern)
				return false;

			switch (methodSymbol.MethodKind)
			{
				case MethodKind.DelegateInvoke:
				case MethodKind.Ordinary:
				case MethodKind.DeclareMethod:
				case MethodKind.ReducedExtension:
					break;
				default:
					return false;
			}

			var parameters = methodSymbol.Parameters;
			var viewType   = methodSymbol.ContainingType;

			if (parameters.IsDefaultOrEmpty || !parameters[parameters.Length - 1].IsParams ||
				viewType == null || !viewType.IsBqlCommand(pxContext) || IsFbqlViewType(viewType) ||
				!IsValidReturnType(methodSymbol, pxContext))
			{
				return false;
			}

			return !pxContext.PXSelectExtensionSymbols.IsDefined || 
				   !viewType.InheritsFromOrEqualsGeneric(pxContext.PXSelectExtensionSymbols.Type);
		}

		private static bool IsFbqlViewType(ITypeSymbol viewType) =>
			viewType.ContainingNamespace?.ToString() == NamespaceNames.PXDataBqlFluent;

		private static bool IsValidReturnType(IMethodSymbol methodSymbol, PXContext pxContext)
		{
			if (methodSymbol.ReturnsVoid)
				return false;
			else if (IsPXUpdateMethod(methodSymbol))
				return true;

			var returnType = methodSymbol.ReturnType;
			return returnType.ImplementsInterface(pxContext.IPXResultsetType) ||
				   returnType.InheritsFrom(pxContext.PXResult) ||
				   returnType.IsDAC(pxContext);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsPXUpdateMethod(IMethodSymbol methodSymbol) =>
			methodSymbol.IsStatic && methodSymbol.ReturnType.SpecialType == SpecialType.System_Int32 &&
			methodSymbol.ContainingType?.IsStatic == true &&
			TypeNames.PXUpdateBqlTypes.Contains(methodSymbol.ContainingType.Name);

		private static void AnalyzeStaticInvocation(IMethodSymbol methodSymbol, PXContext pxContext, SyntaxNodeAnalysisContext syntaxContext,
													InvocationExpressionSyntax invocation)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();
			ExpressionSyntax? accessExpression = invocation.GetAccessNodeFromInvocationNode();

			if (accessExpression == null)
				return;

			ITypeSymbol? callerStaticType = syntaxContext.SemanticModel.GetTypeInfo(accessExpression, syntaxContext.CancellationToken).Type;

			if (callerStaticType == null)
				return;

			if (IsFbqlViewType(callerStaticType))
				return;
			else if (callerStaticType.IsCustomBqlCommand(pxContext))
			{
				AnalyzeDerivedBqlStaticCall(methodSymbol, invocation, pxContext, syntaxContext);
				return;
			}

			int? argsCount = GetBqlArgumentsCount(methodSymbol, pxContext, syntaxContext, invocation);

			if (argsCount == null)
				return;

			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			var walker = new ParametersCounterSyntaxWalker(syntaxContext, pxContext);

			if (!walker.CountParametersInNode(invocation))
				return;

			VerifyBqlArgumentsCount(argsCount.Value, walker.ParametersCounter, syntaxContext, invocation, methodSymbol, pxContext);
		}

		private static void AnalyzeDerivedBqlStaticCall(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation, PXContext pxContext,
														SyntaxNodeAnalysisContext syntaxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			INamedTypeSymbol containingType = methodSymbol.ContainingType;	

			if (containingType.IsUnboundGenericType || !containingType.IsBqlCommand() || containingType.IsCustomBqlCommand(pxContext))
				return;

			int? argsCount = GetBqlArgumentsCount(methodSymbol, pxContext, syntaxContext, invocation);

			if (argsCount == null)
				return;

			syntaxContext.CancellationToken.ThrowIfCancellationRequested();
			ParametersCounterSymbolWalker walker = new ParametersCounterSymbolWalker(syntaxContext, pxContext);

			if (!walker.CountParametersInTypeSymbol(containingType))
				return;

			VerifyBqlArgumentsCount(argsCount.Value, walker.ParametersCounter, syntaxContext, invocation, methodSymbol, pxContext);
		}

		private static void AnalyzeInstanceInvocation(IMethodSymbol methodSymbol, PXContext pxContext, SyntaxNodeAnalysisContext syntaxContext,
													  InvocationExpressionSyntax invocation)
		{
			ExpressionSyntax? accessExpression = invocation.GetAccessNodeFromInvocationNode();

			if (accessExpression == null)
				return;

			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			int? argsCount = GetBqlArgumentsCount(methodSymbol, pxContext, syntaxContext, invocation);

			if (argsCount == null)
				return;

			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			ITypeSymbol? containingType = GetContainingTypeForInstanceCall(pxContext, syntaxContext, invocation, accessExpression);

			if (containingType == null)
				return;

			var walker = new ParametersCounterSymbolWalker(syntaxContext, pxContext);

			if (!walker.CountParametersInTypeSymbol(containingType))
				return;

			VerifyBqlArgumentsCount(argsCount.Value, walker.ParametersCounter, syntaxContext, invocation, methodSymbol, pxContext);
		}

		/// <summary>
		/// Gets bql arguments count which goes for bql parameters. If <c>null</c> then the number couldn't be calculated by static analysis, stop diagnostic.
		/// </summary>
		/// <param name="methodSymbol">The method symbol.</param>
		/// <param name="pxContext">Acumatica-specific context.</param>
		/// <param name="syntaxContext">Syntax context.</param>
		/// <param name="invocation">The invocation node.</param>
		/// <returns/>
		private static int? GetBqlArgumentsCount(IMethodSymbol methodSymbol, PXContext pxContext, SyntaxNodeAnalysisContext syntaxContext,
												 InvocationExpressionSyntax invocation)
		{
			var parameters = methodSymbol.Parameters;
			var bqlArgsParam = parameters[parameters.Length - 1];
			var argumentsList = invocation.ArgumentList.Arguments;
			var argumentPassedViaName = argumentsList.FirstOrDefault(a => a.NameColon?.Name?.Identifier.ValueText == bqlArgsParam.Name);
			int searchArgsCount;

			if (argumentPassedViaName != null)
			{
				int? possibleArgsCount = GetBqlArgumentsCountWhenCouldBePassedAsArray(argumentPassedViaName, invocation, syntaxContext, pxContext);

				if (possibleArgsCount == null)
					return null;

				searchArgsCount = GetSearchMethodArgumentsCount(possibleArgsCount.Value);
				return searchArgsCount + possibleArgsCount;
			}

			var nonBqlArgsParametersCount = methodSymbol.Parameters.Length - 1;   //The last one parameter is params array for BQL args
			int argsCount = argumentsList.Count - nonBqlArgsParametersCount;

			if (argsCount == 1)
			{
				int? possibleArgsCount = GetBqlArgumentsCountWhenCouldBePassedAsArray(argumentsList[argumentsList.Count - 1], invocation, syntaxContext,
																					  pxContext);
				if (possibleArgsCount == null)
					return null;

				argsCount = possibleArgsCount.Value;
			}

			searchArgsCount = GetSearchMethodArgumentsCount(argsCount);
			return searchArgsCount + argsCount;

			//******************Local Function************************************
			int GetSearchMethodArgumentsCount(int bqlArgsCount)
			{
				if (methodSymbol.Name != SearchMethodName)
					return 0;

				return methodSymbol.IsStatic 
					? argumentsList.Count - 1 - bqlArgsCount
					: argumentsList.Count - bqlArgsCount;
			}
		}

		private static int? GetBqlArgumentsCountWhenCouldBePassedAsArray(ArgumentSyntax argumentWhichCanBeArray, InvocationExpressionSyntax invocation,
																		 SyntaxNodeAnalysisContext syntaxContext,
																		 PXContext pxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			TypeInfo typeInfo = syntaxContext.SemanticModel.GetTypeInfo(argumentWhichCanBeArray.Expression, syntaxContext.CancellationToken);
			ITypeSymbol? typeSymbol = typeInfo.Type;

			if (typeSymbol == null)
				return 0;
			else if (typeSymbol.TypeKind != TypeKind.Array)
				return 1;

			if (argumentWhichCanBeArray.Expression is IdentifierNameSyntax arrayVariable)
			{
				var elementsCountResolver = new ArrayElemCountLocalVariableResolver(syntaxContext, pxContext, invocation, arrayVariable);
				return elementsCountResolver.GetArrayElementsCount();
			}

			return RoslynSyntaxUtils.TryGetSizeOfSingleDimensionalNonJaggedArray(argumentWhichCanBeArray.Expression, syntaxContext.SemanticModel,
																				 syntaxContext.CancellationToken);
		}

		private static ITypeSymbol? GetContainingTypeForInstanceCall(PXContext pxContext, SyntaxNodeAnalysisContext syntaxContext,
																	InvocationExpressionSyntax invocation, ExpressionSyntax accessExpression)
		{
			TypeInfo typeInfo = syntaxContext.SemanticModel.GetTypeInfo(accessExpression, syntaxContext.CancellationToken);
			ITypeSymbol? containingType = typeInfo.ConvertedType ?? typeInfo.Type;

			if (containingType == null || !containingType.IsBqlCommand(pxContext) || containingType.IsCustomBqlCommand(pxContext) ||
				IsFbqlViewType(containingType))
			{
				return null;
			}

			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (accessExpression is not IdentifierNameSyntax identifierNode)
				return null;
	
			var resolver = new BqlLocalVariableTypeResolver(syntaxContext, pxContext, invocation, identifierNode);

			return containingType.IsAbstract 
				? resolver.ResolveVariableType()
				: resolver.CheckForBqlModifications()
					? containingType
					: null;
		}

		private static void VerifyBqlArgumentsCount(int argsCount, ParametersCounter parametersCounter, SyntaxNodeAnalysisContext syntaxContext,
													InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol, PXContext pxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (!parametersCounter.IsCountingValid)
				return;

			int searchMethodParametersCount = 0;

			if (methodSymbol.Name == SearchMethodName && methodSymbol.IsGenericMethod)
			{
				searchMethodParametersCount = methodSymbol.TypeParameters.Length;
			}

			int maxCount = parametersCounter.OptionalParametersCount + parametersCounter.RequiredParametersCount + searchMethodParametersCount;
			int minCount = parametersCounter.RequiredParametersCount + searchMethodParametersCount;

			if (argsCount < minCount || argsCount > maxCount)
			{
				Location location = invocation.GetMethodNameLocation();

				if (parametersCounter.OptionalParametersCount == 0)
				{
					syntaxContext.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(
						Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams,
						location, methodSymbol.Name, minCount), pxContext.CodeAnalysisSettings);
				}
				else
				{
					syntaxContext.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(
						Descriptors.PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams,
						location, methodSymbol.Name, minCount, maxCount), pxContext.CodeAnalysisSettings);
				}
			}
		}	
	}
}
