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
    public class DacGraphUsageAnalyzer : PXDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create
            (
                Descriptors.PX1029_PXGraphUsageInDacProperty
            );

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeGraphUsageInDac(syntaxContext, pxContext), SyntaxKind.ClassDeclaration);
            //compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeGraphUsageInsideDacProperty(syntaxContext, pxContext), SyntaxKind.PropertyDeclaration);
        }

        private void AnalyzeGraphUsageInDac(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            if (!(syntaxContext.Node is ClassDeclarationSyntax classDeclaration) || syntaxContext.CancellationToken.IsCancellationRequested)
                return;

            IEnumerable<MemberDeclarationSyntax> memberDeclarations = classDeclaration.DescendantNodes().OfType<MemberDeclarationSyntax>();

            foreach (MemberDeclarationSyntax member in memberDeclarations)
            {

            }

            //IsGraph
        }

        private void AnalyzeGraphUsageInsideDacProperty(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {    
            PropertyDeclarationSyntax node = syntaxContext.Node as PropertyDeclarationSyntax;
            IPropertySymbol property = syntaxContext.SemanticModel.GetDeclaredSymbol(node, syntaxContext.CancellationToken);

            if (property != null && IsDacProperty(property, pxContext) && !syntaxContext.CancellationToken.IsCancellationRequested)
            {
                foreach (SyntaxNode descendant in node.DescendantNodes())
                {
                    if (syntaxContext.CancellationToken.IsCancellationRequested)
                        return;

                    if (descendant is MemberAccessExpressionSyntax ma)
                    {
                        AnalyzeMemberAccessName(ma.Name, syntaxContext, pxContext);
                        AnalyzeMemberAccessExpression(ma.Expression, syntaxContext, pxContext);
                    }
                    else if (descendant is MemberBindingExpressionSyntax mb)
                    {
                        AnalyzeMemberAccessName(mb.Name, syntaxContext, pxContext);

                        SymbolInfo si = syntaxContext.SemanticModel.GetSymbolInfo(mb.Name, syntaxContext.CancellationToken);
                        if (si.Symbol != null && IsGraphUsedAsMemberName(si.Symbol, pxContext))
                        {
                            syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1029_PXGraphUsageInDacProperty, mb.GetLocation()));
                        }
                    }
                    else if (descendant is LocalDeclarationStatementSyntax ld)
                    {
                        AnalyzeLocalDeclaration(ld, syntaxContext, pxContext);
                    }
                }

                //IEnumerable<MemberAccessExpressionSyntax> membersAccess = node.DescendantNodes().OfType<MemberAccessExpressionSyntax>();

                //foreach (MemberAccessExpressionSyntax ma in membersAccess)
                //{
                //    AnalyzeMemberAccessName(ma.Name, syntaxContext, pxContext);
                //    AnalyzeMemberAccessExpression(ma.Expression, syntaxContext, pxContext);
                //}

                //IEnumerable<LocalDeclarationStatementSyntax> localVariables = node.DescendantNodes().OfType<LocalDeclarationStatementSyntax>();

                //foreach (LocalDeclarationStatementSyntax l in localVariables)
                //{
                //    AnalyzeLocalDeclaration(l, syntaxContext, pxContext);
                //}
            }
        }

        private void AnalyzeLocalDeclaration(LocalDeclarationStatementSyntax local, SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            bool isGraphDeclarationType = syntaxContext.SemanticModel.GetSymbolInfo(local.Declaration.Type).Symbol is ITypeSymbol typeSymbol && typeSymbol.InheritsFromOrEquals(pxContext.PXGraphType);

            if (isGraphDeclarationType)
            {
                syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1029_PXGraphUsageInDacProperty, local.GetLocation()));
            }
        }

        private void AnalyzeMemberAccessExpression(ExpressionSyntax expression, SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            ISymbol expressionSymbol = syntaxContext.SemanticModel.GetSymbolInfo(expression, syntaxContext.CancellationToken).Symbol;

            if (expressionSymbol != null && IsGraphUsedAsMemberExpression(expressionSymbol, pxContext))
            {
                syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1029_PXGraphUsageInDacProperty, expression.GetLocation()));
            }
        }

        private void AnalyzeMemberAccessName(SimpleNameSyntax name, SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            ISymbol nameSymbol = syntaxContext.SemanticModel.GetSymbolInfo(name, syntaxContext.CancellationToken).Symbol;

            if (nameSymbol != null && IsGraphUsedAsMemberName(nameSymbol, pxContext))
            {
                syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1029_PXGraphUsageInDacProperty, name.GetLocation()));
            }
        }

        private bool IsGraphUsedAsMemberExpression(ISymbol expressionSymbol, PXContext pxContext)
        {
            bool isGraphUsed = false;

            switch (expressionSymbol)
            {
                case ILocalSymbol l:
                    if (l.Type.InheritsFromOrEquals(pxContext.PXGraphType))
                    {
                        isGraphUsed = true;
                    }
                    break;
                default:
                    isGraphUsed = IsGraphUsedAsMemberName(expressionSymbol, pxContext);
                    break;
            }

            return isGraphUsed;
        }

        private bool IsGraphUsedAsMemberName(ISymbol nameSymbol, PXContext pxContext)
        {
            bool isGraphUsed = false;

            switch (nameSymbol)
            {
                case IFieldSymbol f:
                    if (f.Type.InheritsFromOrEquals(pxContext.PXGraphType))
                    {
                        isGraphUsed = true;
                    }
                    break;
                case IPropertySymbol p:
                    if (p.Type.InheritsFromOrEquals(pxContext.PXGraphType))
                    {
                        isGraphUsed = true;
                    }
                    break;
                case IMethodSymbol m:
                    if (m.ReturnType.InheritsFromOrEquals(pxContext.PXGraphType))
                    {
                        isGraphUsed = true;
                    }
                    break;
            }

            return isGraphUsed;
        }

        private bool IsDacProperty(IPropertySymbol property, PXContext pxContext)
        {
            return property.ContainingType.ImplementsInterface(pxContext.IBqlTableType) ||
                   property.ContainingType.InheritsFrom(pxContext.PXCacheExtensionType);
        }
    }
}
