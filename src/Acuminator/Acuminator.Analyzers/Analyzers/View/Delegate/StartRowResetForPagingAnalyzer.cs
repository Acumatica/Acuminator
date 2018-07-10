using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PX.Data;
using Acuminator.Utilities;


namespace Acuminator.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class StartRowResetForPagingAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1010_StartRowResetForPaging);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzeDelegateAsync(c, pxContext), SymbolKind.Method);
		}

		private static async Task AnalyzeDelegateAsync(SymbolAnalysisContext syntaxContext, PXContext pxContext)
		{
			IMethodSymbol method = syntaxContext.Symbol as IMethodSymbol;

			if (!IsDiagnosticValid(method, syntaxContext, pxContext))
				return;

			var declaration = method.DeclaringSyntaxReferences[0];
			var methodDeclaration = await declaration.GetSyntaxAsync(syntaxContext.CancellationToken)
													 .ConfigureAwait(false) as MethodDeclarationSyntax;

			if (methodDeclaration?.Body == null || syntaxContext.CancellationToken.IsCancellationRequested)
				return;

			SemanticModel semanticModel = syntaxContext.Compilation.GetSemanticModel(declaration.SyntaxTree);
			ILocalSymbol refStartRow = await GetReferenceToStartRow(methodDeclaration, semanticModel, pxContext, syntaxContext.CancellationToken)
													.ConfigureAwait(false);

			if (refStartRow == null || syntaxContext.CancellationToken.IsCancellationRequested)
				return;

			var (selectSymbol, selectInvocation) = GetSelectSymbolAndInvocationNode(methodDeclaration, refStartRow, pxContext, semanticModel,
																					syntaxContext.CancellationToken);
			if (selectSymbol == null || selectInvocation == null || syntaxContext.CancellationToken.IsCancellationRequested)
				return;

			AssignmentExpressionSyntax lastAssigment = GetLastStartRowResetAssignment(methodDeclaration);

			if (lastAssigment != null && lastAssigment.SpanStart > selectInvocation.Span.End)
				return;

			bool registerCodeFix = RegisterCodeFix(methodDeclaration, selectInvocation);
			var diagnosticProperties = new Dictionary<string, string>
			{
				{ DiagnosticProperty.RegisterCodeFix, registerCodeFix.ToString() }
			}.ToImmutableDictionary();

			syntaxContext.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.PX1010_StartRowResetForPaging, selectInvocation.GetLocation(), diagnosticProperties));
		}

		private static bool IsDiagnosticValid(IMethodSymbol method, SymbolAnalysisContext syntaxContext, PXContext pxContext)
		{
			if (method == null || method.IsAbstract || method.ReturnType.SpecialType != SpecialType.System_Collections_IEnumerable ||
				method.DeclaringSyntaxReferences.Length != 1)
			{
				return false;
			}

			return method.IsDelegateForViewInPXGraph(pxContext);
		}

		private static async Task<ILocalSymbol> GetReferenceToStartRow(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel,
																	   PXContext pxContext, CancellationToken cancellationToken)
		{
			DataFlowAnalysis df = semanticModel.AnalyzeDataFlow(methodDeclaration.Body);

			if (df == null || !df.Succeeded || cancellationToken.IsCancellationRequested)
				return null;

			foreach (ILocalSymbol localSymbol in df.WrittenInside.OfType<ILocalSymbol>())
			{
				ImmutableArray<SyntaxReference> syntaxReferences = localSymbol.DeclaringSyntaxReferences;

				if (syntaxReferences.Length == 0)
					continue;

				SyntaxNode symbolSyntax = await syntaxReferences[0].GetSyntaxAsync(cancellationToken).ConfigureAwait(false);

				if (symbolSyntax == null || cancellationToken.IsCancellationRequested)
					return null;

				List<MemberAccessExpressionSyntax> memberAccesses = symbolSyntax.DescendantNodes()
																				.OfType<MemberAccessExpressionSyntax>()
																				.ToList();

				if (memberAccesses.Count != 1)
					continue;

				var symbol = semanticModel.GetSymbolInfo(memberAccesses[0]).Symbol;

				if (symbol != null && symbol.ContainingType.Equals(pxContext.PXViewType) && symbol.Name == nameof(PXView.StartRow))
				{
					return localSymbol;
				}
			}

			return null;
		}

		private static (IMethodSymbol SelectSymbol, InvocationExpressionSyntax SelectInvocation) GetSelectSymbolAndInvocationNode(
									MethodDeclarationSyntax methodDeclaration, ILocalSymbol refStartRow, PXContext pxContext,
									SemanticModel semanticModel, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
				return default;

			var invocationsWithStartRowPassedArg = from argument in methodDeclaration.DescendantNodes().OfType<ArgumentSyntax>()
												   where argument.Expression is IdentifierNameSyntax identifier &&
														 identifier.Identifier.ValueText == refStartRow.Name
												   select argument.Parent<InvocationExpressionSyntax>();

			foreach (InvocationExpressionSyntax invocation in invocationsWithStartRowPassedArg)
			{
				var symbol = (IMethodSymbol)semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol;

				if (cancellationToken.IsCancellationRequested)
					return default;

				if (symbol == null)
					continue;

				if (symbol.Name.StartsWith("Select") &&
				   (symbol.ContainingType.InheritsFromOrEquals(pxContext.PXViewType) ||
					symbol.ContainingType.InheritsFromOrEquals(pxContext.PXSelectBaseType)))
				{
					return (symbol, invocation);
				}
			}

			return default;
		}

		private static AssignmentExpressionSyntax GetLastStartRowResetAssignment(MethodDeclarationSyntax methodDeclaration)
		{
			AssignmentExpressionSyntax lastAssigment = null;
			var startRowAccesses = methodDeclaration.DescendantNodes()
													.OfType<MemberAccessExpressionSyntax>()
													.Where(m => m.Name is IdentifierNameSyntax i &&
																i.Identifier.ValueText == nameof(PXView.StartRow));

			foreach (var memberAccess in startRowAccesses)
			{
				if (memberAccess.Parent is AssignmentExpressionSyntax assigment &&
					assigment.Right is LiteralExpressionSyntax literalExpression &&
					literalExpression.Token.Value.ToString() == "0")
				{
					lastAssigment = assigment;
				}
			}

			return lastAssigment;
		}

		private static bool RegisterCodeFix(MethodDeclarationSyntax methodDeclaration, InvocationExpressionSyntax selectInvocation)
		{
			bool isInReturnStatement = selectInvocation.Parent<ReturnStatementSyntax>() != null;

			if (isInReturnStatement)
				return false;

			int selectInvocationEnd = selectInvocation.Span.End;
			var statements = methodDeclaration.Body.DescendantNodesAndSelf()
												   .OfType<StatementSyntax>();

			foreach (StatementSyntax statement in statements)
			{
				switch (statement)
				{
					case YieldStatementSyntax yieldStatement:
						return false;
					case GotoStatementSyntax gotoStatement when gotoStatement.SpanStart > selectInvocationEnd:
						if (gotoStatement.IsKind(SyntaxKind.GotoStatement))
							return false;

						SwitchSectionSyntax switchSectionWithGoTo = gotoStatement.Parent<SwitchSectionSyntax>();

						if (switchSectionWithGoTo == null || switchSectionWithGoTo.Span.OverlapsWith(selectInvocation.Span)) 
							return false;

						continue;
				}
			}

			return true;
		}
	}
}