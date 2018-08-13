using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DacMethodsUsageAnalyzer : PXDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create
            (
                Descriptors.PX1031_DacCannotContainInstanceMethods,
                Descriptors.PX1032_DacPropertyCannotContainMethodInvocations
            );

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeDacDeclaration(syntaxContext, pxContext), SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeDacDeclaration(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            ClassDeclarationSyntax classDeclaration = syntaxContext.Node as ClassDeclarationSyntax;
            INamedTypeSymbol typeSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclaration, syntaxContext.CancellationToken);

            if (typeSymbol != null && typeSymbol.IsDacOrExtension(pxContext) && !syntaxContext.CancellationToken.IsCancellationRequested)
            {
                foreach (SyntaxNode node in classDeclaration.DescendantNodes())
                {
                    if (syntaxContext.CancellationToken.IsCancellationRequested)
                        return;

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

        private void AnalyzeMethodInvocationInDacProperty(PropertyDeclarationSyntax property, SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            foreach (SyntaxNode node in property.DescendantNodes())
            {
                if (syntaxContext.CancellationToken.IsCancellationRequested)
                    return;

                if (node is InvocationExpressionSyntax invocation)
                {
                    SymbolInfo symbol = syntaxContext.SemanticModel.GetSymbolInfo(invocation, syntaxContext.CancellationToken);

                    if (symbol.Symbol != null && symbol.Symbol.Kind == SymbolKind.Method && !symbol.Symbol.IsStatic)
                    {
                        syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1032_DacPropertyCannotContainMethodInvocations, invocation.GetLocation()));
                    }
                }
                else if (node is ObjectCreationExpressionSyntax creation)
                {
                    syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1032_DacPropertyCannotContainMethodInvocations, node.GetLocation()));
                }
            }
        }

        private void AnalyzeMethodDeclarationInDac(MethodDeclarationSyntax method, SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            if (!method.IsStatic())
            {
                Location location = method?.Identifier.GetLocation() ?? method.GetLocation();
                syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1031_DacCannotContainInstanceMethods, location));
            }
        }
    }
}
