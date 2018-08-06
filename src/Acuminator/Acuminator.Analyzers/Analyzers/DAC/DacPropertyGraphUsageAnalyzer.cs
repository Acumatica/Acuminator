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
    public class DacPropertyGraphUsageAnalyzer : PXDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create
            (
                Descriptors.PX1029_PXGraphUsageInDacProperty
            );

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeGraphUsageInsideDacProperty(syntaxContext, pxContext), SyntaxKind.PropertyDeclaration);
        }

        private void AnalyzeGraphUsageInsideDacProperty(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {    
            PropertyDeclarationSyntax node = syntaxContext.Node as PropertyDeclarationSyntax;
            IPropertySymbol property = syntaxContext.SemanticModel.GetDeclaredSymbol(node, syntaxContext.CancellationToken);

            if (property != null && IsDacProperty(property, pxContext))
            {
                IEnumerable<MemberAccessExpressionSyntax> membersAccess = node.DescendantNodes().OfType<MemberAccessExpressionSyntax>();

                foreach (MemberAccessExpressionSyntax ma in membersAccess)
                {
                    AnalyzeMemberAccessName(ma.Name, syntaxContext, pxContext);
                    AnalyzeMemberAccessExpression(ma.Expression, syntaxContext, pxContext);
                }

                IEnumerable<LocalDeclarationStatementSyntax> localVariables = node.DescendantNodes().OfType<LocalDeclarationStatementSyntax>();

                foreach (LocalDeclarationStatementSyntax l in localVariables)
                {
                    AnalyzeLocalDeclaration(l, syntaxContext, pxContext);
                }
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
