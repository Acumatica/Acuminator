using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Analyzers.Utilities;
using System.Collections.Generic;
using PX.Data;

namespace Acuminator.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StartRowResetForPagingAnalyzer : PXDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1010_StartRowResetForPaging);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
            compilationStartContext.RegisterSymbolAction(c => AnalyzeSymbol(c, pxContext), SymbolKind.Method);
        }

		private static void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext)
		{
            var method = (IMethodSymbol)context.Symbol;
            if (method.ReturnType.SpecialType != SpecialType.System_Collections_IEnumerable)
                return;

            var parent = method.ContainingType;
            if (parent == null || !parent.InheritsFrom(pxContext.PXGraphType))
                return;

            var views = parent.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(f => f.Type.InheritsFrom(pxContext.PXSelectBaseType))
                .ToArray();
            if (!views.Any(f => String.Equals(f.Name, method.Name, StringComparison.OrdinalIgnoreCase)))
                return;

            var declaration = method.DeclaringSyntaxReferences[0];
            var methodDeclaration = declaration.GetSyntax() as MethodDeclarationSyntax;
            if (methodDeclaration == null)
                return;

            var semanticModel = context.Compilation.GetSemanticModel(declaration.SyntaxTree);
            DataFlowAnalysis df = semanticModel.AnalyzeDataFlow(
               methodDeclaration.Body);

            if (df == null || !df.Succeeded)
                return;

            ILocalSymbol refStartRow = null;
            foreach (var p in df.WrittenInside)
            {
                if (p is ILocalSymbol ls)
                {
                    List<MemberAccessExpressionSyntax> memberAccesses = p.DeclaringSyntaxReferences[0].GetSyntax().
                        DescendantNodes().OfType<MemberAccessExpressionSyntax>().ToList();
                    if (memberAccesses.Count != 1)
                        continue;
                    var symbol = semanticModel.GetSymbolInfo(memberAccesses[0]).Symbol;
                    if (symbol != null &&
                        symbol.ContainingType == pxContext.PXViewType &&
                        symbol.Name == nameof(PXView.StartRow))
                    {
                        refStartRow = ls;
                        break;
                    }
                }
            }

            if (refStartRow == null)
                return;

            IMethodSymbol selectSymbol = null;
            InvocationExpressionSyntax selectInvocation = null;
            foreach (var argumentSyntax in declaration.GetSyntax().DescendantNodes().OfType<ArgumentSyntax>())
            {
                var ins = argumentSyntax.Expression as IdentifierNameSyntax;
                if (ins != null && ins.Identifier.ValueText == refStartRow.Name)
                {
                    SyntaxNode ies = argumentSyntax;
                    do
                    {
                        ies = ies.Parent;
                    } while (!(ies is InvocationExpressionSyntax));
                    var symbol = (IMethodSymbol)semanticModel.GetSymbolInfo(ies).Symbol;
                    if(symbol.Name.StartsWith("Select") && 
                       (symbol.ContainingType.InheritsFromOrEquals(pxContext.PXViewType) ||
                        symbol.ContainingType.InheritsFromOrEquals(pxContext.PXSelectBaseType)))
                    {
                        //context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1010_StartRowResetForPaging, a.GetLocation()));
                        selectSymbol = symbol;
                        selectInvocation = (InvocationExpressionSyntax)ies;
                        break;
                    }
                }
            }

            if (selectSymbol == null)
                return;

            AssignmentExpressionSyntax lastAssigment = null;
            foreach (var memberAccess in declaration.GetSyntax().DescendantNodes().
                OfType<MemberAccessExpressionSyntax>().
                Where(m => m.Name is IdentifierNameSyntax i && i.Identifier.ValueText == nameof(PXView.StartRow)))
            {
                if (memberAccess.Parent is AssignmentExpressionSyntax assigment &&
                    assigment.Right is LiteralExpressionSyntax literalExpression &&
                    literalExpression.Token.Value.ToString() == "0")
                {
                    lastAssigment = assigment;
                }
            }

            if (lastAssigment != null &&
                lastAssigment.SpanStart > selectInvocation.Span.End)
                return;
                
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1010_StartRowResetForPaging, selectInvocation.GetLocation()));

        }
	}
}