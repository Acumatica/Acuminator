using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities;
using PX.Data;


namespace Acuminator.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public partial class BqlParameterMismatchAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams,
                                  Descriptors.PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(c => AnalyzeNode(c, pxContext), SyntaxKind.InvocationExpression);
		}

		private static void AnalyzeNode(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			if (syntaxContext.CancellationToken.IsCancellationRequested || !(syntaxContext.Node is InvocationExpressionSyntax invocationNode) ||
				invocationNode.ContainsDiagnostics)
			{
				return;
			}

			var symbolInfo = syntaxContext.SemanticModel.GetSymbolInfo(invocationNode);

			if (!(symbolInfo.Symbol is IMethodSymbol methodSymbol) || !IsValidMethodGeneralCheck(methodSymbol, pxContext))
				return;

			if (methodSymbol.IsStatic)
				AnalyzeStaticInvocation(methodSymbol, pxContext, syntaxContext, invocationNode);
			else
				AnalyzeInstanceInvocation(methodSymbol, pxContext, syntaxContext, invocationNode);
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

			if (parameters.IsDefaultOrEmpty || !parameters[parameters.Length - 1].IsParams)
				return false;

			return methodSymbol.ContainingType.IsBqlCommand(pxContext) && IsValidReturnType(methodSymbol, pxContext);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsValidReturnType(IMethodSymbol methodSymbol, PXContext pxContext)
		{
			if (methodSymbol.ReturnsVoid)
				return false;

			var returnType = methodSymbol.ReturnType;
			return returnType.ImplementsInterface(pxContext.IPXResultsetType) ||
				   returnType.InheritsFrom(pxContext.PXResult) ||
				   returnType.ImplementsInterface(pxContext.IBqlTableType);
		}

		private static void AnalyzeStaticInvocation(IMethodSymbol methodSymbol, PXContext pxContext, SyntaxNodeAnalysisContext syntaxContext,
													InvocationExpressionSyntax invocationNode)
		{
			int? argsCount = GetBqlArgumentsCount(methodSymbol, pxContext, syntaxContext, invocationNode);

			if (argsCount == null || syntaxContext.CancellationToken.IsCancellationRequested)
				return;

			ParametersCounterSyntaxWalker walker = new ParametersCounterSyntaxWalker(syntaxContext, pxContext);

			if (!walker.CountParametersInNode(invocationNode))
				return;

			VerifyBqlArgumentsCount(argsCount.Value, walker.ParametersCounter, syntaxContext, invocationNode, methodSymbol);
		}

		private static void AnalyzeInstanceInvocation(IMethodSymbol methodSymbol, PXContext pxContext, SyntaxNodeAnalysisContext syntaxContext,
													  InvocationExpressionSyntax invocationNode)
		{
			ExpressionSyntax accessExpression = invocationNode.GetAccessNodeFromInvocationNode();

			if (accessExpression == null || syntaxContext.CancellationToken.IsCancellationRequested)
				return;

			int? argsCount = GetBqlArgumentsCount(methodSymbol, pxContext, syntaxContext, invocationNode);

			if (argsCount == null || syntaxContext.CancellationToken.IsCancellationRequested)
				return;
	
			ITypeSymbol containingType = GetContainingTypeForInstanceCall(pxContext, syntaxContext, accessExpression);

			if (containingType == null)
				return;

			ParametersCounterSymbolWalker walker = new ParametersCounterSymbolWalker(syntaxContext, pxContext);

			if (!walker.CountParametersInTypeSymbol(containingType))
				return;

			VerifyBqlArgumentsCount(argsCount.Value, walker.ParametersCounter, syntaxContext, invocationNode, methodSymbol);
		}

		/// <summary>
		/// Gets bql arguments count which goes for bql parameters. If <c>null</c> then the number couldn't be calculated by static analysis, stop diagnostic.
		/// </summary>
		/// <param name="methodSymbol">The method symbol.</param>
		/// <param name="pxContext">Acumatica-specific context.</param>
		/// <param name="syntaxContext">Syntax context.</param>
		/// <param name="invocationNode">The invocation node.</param>
		/// <returns/>
		private static int? GetBqlArgumentsCount(IMethodSymbol methodSymbol, PXContext pxContext, SyntaxNodeAnalysisContext syntaxContext,
												 InvocationExpressionSyntax invocationNode)
		{
			var parameters = methodSymbol.Parameters;
			var bqlArgsParam = parameters[parameters.Length - 1];
			var argumentsList = invocationNode.ArgumentList.Arguments;
			var argumentPassedViaName = argumentsList.FirstOrDefault(a => a.NameColon?.Name?.Identifier.ValueText == bqlArgsParam.Name);

			if (argumentPassedViaName != null)
				return GetBqlArgumentsCountWhenCouldBePassedAsArray(argumentPassedViaName, syntaxContext, pxContext);

			var nonBqlArgsParametersCount = methodSymbol.Parameters.Length - 1;   //The last one parameter is params array for BQL args
			int argsCount = argumentsList.Count - nonBqlArgsParametersCount;

			if (argsCount == 1)
				return GetBqlArgumentsCountWhenCouldBePassedAsArray(argumentsList[argumentsList.Count - 1], syntaxContext, pxContext);

			return argsCount;
		}

		private static int? GetBqlArgumentsCountWhenCouldBePassedAsArray(ArgumentSyntax argumentWhichCanBeArray, 
																		 SyntaxNodeAnalysisContext syntaxContext,
																		 PXContext pxContext)
		{
			if (syntaxContext.CancellationToken.IsCancellationRequested)
				return null;

			TypeInfo typeInfo = syntaxContext.SemanticModel.GetTypeInfo(argumentWhichCanBeArray.Expression, syntaxContext.CancellationToken);
			ITypeSymbol typeSymbol = typeInfo.Type;

			if (typeSymbol == null)
				return 0;
			else if (typeSymbol.TypeKind != TypeKind.Array)
				return 1;

			if (argumentWhichCanBeArray.Expression is IdentifierNameSyntax arrayVariable)
			{
				var elementsCountResolver = new ArrayElemCountLocalVariableResolver(syntaxContext, pxContext, arrayVariable);
				return elementsCountResolver.GetArrayElementsCount();
			}

			return RoslynSyntaxUtils.TryGetSizeOfSingleDimensionalNonJaggedArray(argumentWhichCanBeArray.Expression, syntaxContext.SemanticModel,
																			     syntaxContext.CancellationToken);
		}

		private static ITypeSymbol GetContainingTypeForInstanceCall(PXContext pxContext, SyntaxNodeAnalysisContext syntaxContext,
																	ExpressionSyntax accessExpression)
		{
			TypeInfo typeInfo = syntaxContext.SemanticModel.GetTypeInfo(accessExpression, syntaxContext.CancellationToken);
			ITypeSymbol containingType = typeInfo.ConvertedType ?? typeInfo.Type;

			if (containingType == null || !containingType.IsBqlCommand(pxContext))
				return null;

			if (!containingType.IsAbstract && !containingType.IsCustomBqlCommand(pxContext))
				return containingType;

			if (!(accessExpression is IdentifierNameSyntax identifierNode) || syntaxContext.CancellationToken.IsCancellationRequested)   
				return null;                                                 //Should exclude everything except local variable. For example expressions like "this.var.Select()" should be excluded

			BqlLocalVariableTypeResolver resolver = new BqlLocalVariableTypeResolver(syntaxContext, pxContext, identifierNode);
			return resolver.ResolveVariableType();
		}

		private static void VerifyBqlArgumentsCount(int argsCount, ParametersCounter parametersCounter, SyntaxNodeAnalysisContext syntaxContext,
													InvocationExpressionSyntax invocationNode, IMethodSymbol methodSymbol)
		{
			if (syntaxContext.CancellationToken.IsCancellationRequested || !parametersCounter.IsCountingValid)
				return;

			int maxCount = parametersCounter.OptionalParametersCount + parametersCounter.RequiredParametersCount;
			int minCount = parametersCounter.RequiredParametersCount;

            if (argsCount < minCount || argsCount > maxCount)
            {
                Location location = GetLocation(invocationNode);

                if (parametersCounter.OptionalParametersCount == 0)
                {
                    syntaxContext.ReportDiagnostic(
                        Diagnostic.Create(Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams, location,
                                          methodSymbol.Name, parametersCounter.RequiredParametersCount));
                }
                else
                {
                    syntaxContext.ReportDiagnostic(
                        Diagnostic.Create(Descriptors.PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams, location,
                                          methodSymbol.Name, minCount, maxCount));
                }
            }
		}

		private static Location GetLocation(InvocationExpressionSyntax invocationNode)
		{
			if (invocationNode.Expression is MemberAccessExpressionSyntax memberAccessNode)
			{
				return memberAccessNode.Name?.GetLocation() ?? invocationNode.GetLocation();
			}
			else if (invocationNode.Expression is MemberBindingExpressionSyntax memberBindingNode)
			{
				return memberBindingNode.Name?.GetLocation() ?? invocationNode.GetLocation();
			}

			return invocationNode.GetLocation();
		}
	}
}
