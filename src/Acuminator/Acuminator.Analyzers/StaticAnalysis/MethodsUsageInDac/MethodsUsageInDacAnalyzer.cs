using System.Collections.Immutable;
using System.Threading;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.MethodsUsageInDac
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class MethodsUsageInDacAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1031_DacCannotContainInstanceMethods,
				Descriptors.PX1032_DacPropertyCannotContainMethodInvocations
			);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext,
			PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeDacDeclaration(syntaxContext, pxContext),
				SyntaxKind.ClassDeclaration);
		}

		private void AnalyzeDacDeclaration(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax) syntaxContext.Node;
			INamedTypeSymbol typeSymbol =
				syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclaration, syntaxContext.CancellationToken);

			if (typeSymbol != null && typeSymbol.IsDacOrExtension(pxContext))
			{
				foreach (SyntaxNode node in classDeclaration.DescendantNodes(
					n => !(n is ClassDeclarationSyntax) || IsDacOrExtension(n, pxContext, syntaxContext.SemanticModel, syntaxContext.CancellationToken)))
				{
					syntaxContext.CancellationToken.ThrowIfCancellationRequested();

					if (node is MethodDeclarationSyntax method)
					{
						AnalyzeMethodDeclarationInDac(method, syntaxContext, pxContext);
					}
					else if (node is PropertyDeclarationSyntax property)
					{
						AnalyzeMethodInvocationInDacProperty(property, syntaxContext, pxContext);
					}
				}
			}
		}

		private void AnalyzeMethodInvocationInDacProperty(PropertyDeclarationSyntax property,
			SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			foreach (SyntaxNode node in property.DescendantNodes())
			{
				syntaxContext.CancellationToken.ThrowIfCancellationRequested();

				if (node is InvocationExpressionSyntax invocation)
				{
					SymbolInfo symbol = syntaxContext.SemanticModel.GetSymbolInfo(invocation, syntaxContext.CancellationToken);

					if (symbol.Symbol != null && symbol.Symbol.Kind == SymbolKind.Method && !symbol.Symbol.IsStatic)
					{
						syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1032_DacPropertyCannotContainMethodInvocations,
							invocation.GetLocation()));
					}
				}
				else if (node is ObjectCreationExpressionSyntax)
				{
					syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1032_DacPropertyCannotContainMethodInvocations,
						node.GetLocation()));
				}
			}
		}

		private void AnalyzeMethodDeclarationInDac(MethodDeclarationSyntax method, SyntaxNodeAnalysisContext syntaxContext,
			PXContext pxContext)
		{
			if (method != null && !method.IsStatic())
			{
				syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1031_DacCannotContainInstanceMethods,
					method.Identifier.GetLocation()));
			}
		}

		private bool IsDacOrExtension(SyntaxNode node, PXContext pxContext, SemanticModel semanticModel, CancellationToken cancellationToken)
		{
			if (node is ClassDeclarationSyntax classDeclaration)
			{
				INamedTypeSymbol symbol = semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken);
				if (symbol != null)
					return symbol.IsDacOrExtension(pxContext);
			}

			return false;
		}
	}
}