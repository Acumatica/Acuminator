using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;


namespace Acuminator.Analyzers
{
    //[DiagnosticAnalyzer(LanguageNames.CSharp)]
    //public class BqlParameterMismatchAnalyzer : PXDiagnosticAnalyzer
    //{
    //    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    //        ImmutableArray.Create(Descriptors.PX1015_PXBqlParametersMismatch);

    //    internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
    //    {
    //        compilationStartContext.RegisterSyntaxNodeAction(c => AnalyzeNode(c, pxContext), SyntaxKind.GenericName);
    //    }

    //    private static void AnalyzeNode(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
    //    {
    //        GenericNameSyntax genericNode = syntaxContext.Node as GenericNameSyntax;

    //        if (!genericNode.CheckGenericNodeParentKind())
    //            return;

    //        ITypeSymbol typeSymbol = syntaxContext.SemanticModel.GetSymbolInfo(genericNode).Symbol as ITypeSymbol;

    //        if (typeSymbol == null)
    //            return;

    //        if (!typeSymbol.InheritsFrom(pxContext.BQL.PXSelectBase))
    //            return;

    //        if (!CheckBQLStatement(genericNode, syntaxContext, pxContext))
    //        {
    //            DiagnosticDescriptor descriptor = Descriptors.PXF1001_PXBadBqlFormat;
    //            syntaxContext.ReportDiagnostic(Diagnostic.Create(descriptor, genericNode.GetLocation()));
    //        }
    //    }

    //    private static bool CheckBQLStatement(GenericNameSyntax genericNode, SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
    //    {
    //        if (genericNode.TypeArgumentList.Arguments.Count <= 1)
    //            return true;

    //        var typeArgsList = genericNode.TypeArgumentList.Arguments;

    //        if (!typeArgsList[0].HasExactlyOneEOL())
    //            return false;

    //        for (int i = 1; i < typeArgsList.Count; i++)
    //        {
    //            if (typeArgsList[i].Kind() == SyntaxKind.GenericName)
    //            {
    //                GenericNameSyntax typeSyntNode = typeArgsList[i] as GenericNameSyntax;

    //                if (typeSyntNode != null && !AnalyzeGenericSubNode(typeSyntNode, syntaxContext, pxContext))
    //                    return false;


    //            }

    //        }

    //        return true;
    //    }

    //    private static bool AnalyzeGenericSubNode(GenericNameSyntax typeSyntNode, SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
    //    {
    //        ITypeSymbol typeSymbol = syntaxContext.SemanticModel.GetSymbolInfo(typeSyntNode).Symbol as ITypeSymbol;

    //        if (typeSymbol == null)
    //            return true;

    //        if (typeSymbol.InheritsFrom(pxContext.BQL.OrderBy))
    //        {
    //            if (!typeSyntNode.HasExactlyOneEOL())
    //                return false;
    //        }

    //        return true;
    //    }
    //}
}
