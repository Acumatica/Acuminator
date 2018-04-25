using System;
using System.Collections.Immutable;
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
			ImmutableArray.Create(Descriptors.PX1015_PXBqlParametersMismatch);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(c => AnalyzeNode(c, pxContext), SyntaxKind.InvocationExpression);
		}

		private static void AnalyzeNode(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			if (syntaxContext.CancellationToken.IsCancellationRequested || !(syntaxContext.Node is InvocationExpressionSyntax invocationNode))
				return;

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
			(int argsCount, bool stopDiagnostic) = GetBqlArgumentsCount(methodSymbol, pxContext, syntaxContext, invocationNode);

			if (stopDiagnostic || syntaxContext.CancellationToken.IsCancellationRequested)
				return;

			ParametersCounterSyntaxWalker walker = new ParametersCounterSyntaxWalker(syntaxContext, pxContext);
			walker.Visit(invocationNode);
			VerifyBqlArgumentsCount(argsCount, walker.ParametersCounter, syntaxContext, invocationNode, methodSymbol);
		}

		private static void AnalyzeInstanceInvocation(IMethodSymbol methodSymbol, PXContext pxContext, SyntaxNodeAnalysisContext syntaxContext,
													  InvocationExpressionSyntax invocationNode)
		{
			if (!(invocationNode.Expression is MemberAccessExpressionSyntax memberAccessNode) ||
				memberAccessNode.OperatorToken.Kind() != SyntaxKind.DotToken ||
				syntaxContext.CancellationToken.IsCancellationRequested)
			{
				return;
			}

			(int argsCount, bool stopDiagnostic) = GetBqlArgumentsCount(methodSymbol, pxContext, syntaxContext, invocationNode);

			if (stopDiagnostic || syntaxContext.CancellationToken.IsCancellationRequested)
				return;

			TypeInfo typeInfo = syntaxContext.SemanticModel.GetTypeInfo(memberAccessNode.Expression, syntaxContext.CancellationToken);
			ITypeSymbol containingType = typeInfo.ConvertedType ?? typeInfo.Type;

			if (containingType == null)
				return;

			ParametersCounterSymbolWalker walker = new ParametersCounterSymbolWalker(syntaxContext, pxContext);
			walker.Visit(containingType);
			VerifyBqlArgumentsCount(argsCount, walker.ParametersCounter, syntaxContext, invocationNode, methodSymbol);
		}

		private static (int ArgsCount, bool StopDiagnostic) GetBqlArgumentsCount(IMethodSymbol methodSymbol, PXContext pxContext,
																				 SyntaxNodeAnalysisContext syntaxContext,
																				 InvocationExpressionSyntax invocationNode)
		{
			var parameters = methodSymbol.Parameters;
			var bqlArgsParam = parameters[parameters.Length - 1];
			var argumentsList = invocationNode.ArgumentList.Arguments;

			var argumentPassedViaName = argumentsList.FirstOrDefault(a => a.NameColon?.Name?.Identifier.ValueText == bqlArgsParam.Name);

			if (argumentPassedViaName != null)
				return GetBqlArgumentsCountWhenCouldBePassedAsArray(argumentPassedViaName, syntaxContext);

			var nonBqlArgsParametersCount = methodSymbol.Parameters.Length - 1;   //The last one parameter is params array for BQL args
			int argsCount = argumentsList.Count - nonBqlArgsParametersCount;

			if (argsCount == 1)
				return GetBqlArgumentsCountWhenCouldBePassedAsArray(argumentsList[argumentsList.Count - 1], syntaxContext);

			return (argsCount, StopDiagnostic: false);
		}

		private static (int ArgsCount, bool StopDiagnostic) GetBqlArgumentsCountWhenCouldBePassedAsArray(ArgumentSyntax argumentPassedViaName,
																										 SyntaxNodeAnalysisContext syntaxContext)
		{
			TypeInfo typeInfo = syntaxContext.SemanticModel.GetTypeInfo(argumentPassedViaName.Expression, syntaxContext.CancellationToken);
			ITypeSymbol typeSymbol = typeInfo.Type;

			if (typeSymbol == null)
				return (0, false);
			else if (typeSymbol.IsValueType || typeSymbol.TypeKind != TypeKind.Array)
				return (1, false);
			
			switch (argumentPassedViaName.Expression)
			{
				case InitializerExpressionSyntax initializerExpression when initializerExpression.Kind() == SyntaxKind.ArrayInitializerExpression:
					return (initializerExpression.Expressions.Count, false);
				case ArrayCreationExpressionSyntax arrayCreationNode:
					return (arrayCreationNode.Initializer.Expressions.Count, false);
				case ImplicitArrayCreationExpressionSyntax arrayImplicitCreationNode:
					return (arrayImplicitCreationNode.Initializer.Expressions.Count, false);
				default:
					return (0, StopDiagnostic: true);
			}		
		}

		private static void VerifyBqlArgumentsCount(int argsCount, ParametersCounter parametersCounter, SyntaxNodeAnalysisContext syntaxContext,
													InvocationExpressionSyntax invocationNode, IMethodSymbol methodSymbol)
		{
			if (!parametersCounter.IsCountingValid)
				return;

			int maxCount = parametersCounter.OptionalParametersCount + parametersCounter.RequiredParametersCount;
			int minCount = parametersCounter.RequiredParametersCount;

			if (argsCount < minCount || argsCount > maxCount)
			{
				Location location = GetLocation(invocationNode);
				syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1015_PXBqlParametersMismatch, location, methodSymbol.Name));
			}
		}

		private static Location GetLocation(InvocationExpressionSyntax invocationNode)
		{
			var memberAccessNode = invocationNode.ChildNodes()
												 .OfType<MemberAccessExpressionSyntax>()
												 .FirstOrDefault();

			if (memberAccessNode == null)
				return invocationNode.GetLocation();

			var methodIdentifierNode = memberAccessNode.ChildNodes()
													   .OfType<IdentifierNameSyntax>()
													   .LastOrDefault();
			
			return methodIdentifierNode?.GetLocation() ?? invocationNode.GetLocation();
		}
	}
}
