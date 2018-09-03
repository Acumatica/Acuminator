using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

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
                IEnumerable<INamedTypeSymbol> whiteList = GetWhitelist(pxContext);

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
						AnalyzeMethodInvocationInDacProperty(property, whiteList, syntaxContext, pxContext);
					}
				}
			}
		}

        private IEnumerable<INamedTypeSymbol> GetWhitelist(PXContext pxContext)
        {
            return new[]
            {
                pxContext.SystemTypes.Bool,
                pxContext.SystemTypes.Byte,
                pxContext.SystemTypes.Int16,
                pxContext.SystemTypes.Int32,
                pxContext.SystemTypes.Int64,
                pxContext.SystemTypes.Decimal,
                pxContext.SystemTypes.Float,
                pxContext.SystemTypes.Double,
                pxContext.SystemTypes.String,
                pxContext.SystemTypes.Guid,
                pxContext.SystemTypes.DateTime,
                pxContext.SystemTypes.TimeSpan,
                pxContext.SystemTypes.Enum,
                pxContext.SystemTypes.Nullable
            };
        }

		private void AnalyzeMethodInvocationInDacProperty(PropertyDeclarationSyntax property, IEnumerable<INamedTypeSymbol> whiteList,
            SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			foreach (SyntaxNode node in property.DescendantNodes())
			{
				syntaxContext.CancellationToken.ThrowIfCancellationRequested();

				if (node is InvocationExpressionSyntax invocation)
				{
					ISymbol symbol = syntaxContext.SemanticModel.GetSymbolInfo(invocation, syntaxContext.CancellationToken).Symbol;

                    if (symbol == null || !(symbol is IMethodSymbol method) || method.IsStatic)
                        continue;

                    bool inWhitelist = whiteList.Any(t => method.ContainingType.Equals(t) ||
                                                          method.ContainingType.ConstructedFrom.Equals(t));
                    if (inWhitelist)
                        continue;

                    syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1032_DacPropertyCannotContainMethodInvocations,
                        invocation.GetLocation()));
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