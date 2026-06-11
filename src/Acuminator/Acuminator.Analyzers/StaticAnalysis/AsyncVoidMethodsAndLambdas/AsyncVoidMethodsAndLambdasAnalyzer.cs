using System;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.AsyncVoidMethodsAndLambdas
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public partial class AsyncVoidMethodsAndLambdasAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
			ImmutableArray.Create(Descriptors.PX1038_AsyncVoidMethod);

		public AsyncVoidMethodsAndLambdasAnalyzer() : this(null)
		{ }

		public AsyncVoidMethodsAndLambdasAnalyzer(CodeAnalysisSettings? codeAnalysisSettings) : base(codeAnalysisSettings)
		{ }

		protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(c => AnalyzeMethodOrLambdaDeclarationNode(c, pxContext), 
										SyntaxKind.MethodDeclaration, 
										SyntaxKind.ParenthesizedLambdaExpression, SyntaxKind.SimpleLambdaExpression,
										SyntaxKind.AnonymousMethodExpression);
		}

		private static void AnalyzeMethodOrLambdaDeclarationNode(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			switch (syntaxContext.Node)
			{
				case MethodDeclarationSyntax methodDeclaration:
					AnalyzeMethodDeclaration(syntaxContext, methodDeclaration, pxContext);
					return;

				// common base class for lambdas and anonymous methods
				case AnonymousFunctionExpressionSyntax anonymousFunctionExpression:
					AnalyzeLambdaDeclaration(syntaxContext, pxContext, anonymousFunctionExpression);
					return;
			}
		}

		private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext syntaxContext, MethodDeclarationSyntax methodDeclaration, PXContext pxContext)
		{
			if (!methodDeclaration.IsVoidMethod())
				return;

			if (methodDeclaration.IsAsync())
			{
				ReportAsyncVoidMethod(syntaxContext, pxContext, methodDeclaration);
				return;
			}

			if (methodDeclaration.IsPartial())
				AnalyzePartialMethod(syntaxContext, pxContext, methodDeclaration);
		}

		private static void AnalyzePartialMethod(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, MethodDeclarationSyntax methodDeclaration)
		{
			// For partial methods one of the declarations may be async and the other not.
			// So, both declarations need to be checked.
			// Thus, we need to resort to a bit more expensive symbol analysis.
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(methodDeclaration, syntaxContext.CancellationToken);

			if (methodSymbol == null)
				return;

			if (methodSymbol.IsAsync)
			{
				ReportAsyncVoidMethod(syntaxContext, pxContext, methodDeclaration);
				return;
			}

			var otherPartOfPartialMethod = methodSymbol.IsPartialDefinition
				? methodSymbol.PartialImplementationPart
				: methodSymbol.PartialDefinitionPart;

			if (otherPartOfPartialMethod != null && otherPartOfPartialMethod.IsAsync)
				ReportAsyncVoidMethod(syntaxContext, pxContext, methodDeclaration);
		}

		private static void ReportAsyncVoidMethod(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, 
												  MethodDeclarationSyntax methodDeclaration)
		{
			var location = methodDeclaration.ReturnType.GetLocation();

			syntaxContext.ReportDiagnosticWithSuppressionCheck(
								Diagnostic.Create(Descriptors.PX1038_AsyncVoidMethod, location),
								pxContext.CodeAnalysisSettings);
		}

		private static void AnalyzeLambdaDeclaration(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext,
													 AnonymousFunctionExpressionSyntax lambdaOrAnonymousDelegateDeclaration)
		{
			if (!lambdaOrAnonymousDelegateDeclaration.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword))
				return;

			var lambdaMethodSymbol = syntaxContext.SemanticModel.GetSymbolOrBestCandidate(lambdaOrAnonymousDelegateDeclaration, 
																						   syntaxContext.CancellationToken) as IMethodSymbol;
			if (lambdaMethodSymbol == null || !lambdaMethodSymbol.ReturnsVoid)
				return;

			var location = lambdaOrAnonymousDelegateDeclaration.AsyncKeyword.GetLocation().NullIfLocationKindIsNone() ??
							   lambdaOrAnonymousDelegateDeclaration.GetLocation();

			syntaxContext.ReportDiagnosticWithSuppressionCheck(
								Diagnostic.Create(Descriptors.PX1038_AsyncVoidLambdasAndAnonymousDelegates, location),
								pxContext.CodeAnalysisSettings);
		}
	}
}
