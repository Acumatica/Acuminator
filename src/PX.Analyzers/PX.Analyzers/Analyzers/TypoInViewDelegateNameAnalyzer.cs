using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PX.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TypoInViewDelegateNameAnalyzer : PXDiagnosticAnalyzer
    {
	    public const string ViewFieldNameProperty = "field";
		private const int MaximumDistance = 2;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
				Descriptors.PX1005_TypoInViewDelegateName);
        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSymbolAction(c => Analyze(c, pxContext), 
                SymbolKind.Method);
        }

        private void Analyze(SymbolAnalysisContext context, PXContext pxContext)
        {
			var method = (IMethodSymbol) context.Symbol;
	        if (method.ReturnType.SpecialType != SpecialType.System_Collections_IEnumerable
	            || method.IsOverride
				|| method.Parameters.Length > 0)
	        {
		        return;
	        }

	        var parent = method.ContainingType;
	        if (parent != null && parent.InheritsFrom(pxContext.PXGraphType))
	        {
		        var views = parent.GetMembers()
					.OfType<IFieldSymbol>()
					.Where(f => f.Type.InheritsFrom(pxContext.PXSelectBaseType))
					.ToArray();
		        if (views.Any(f => String.Equals(f.Name, method.Name, StringComparison.OrdinalIgnoreCase)))
			        return;

		        var nearest = FindNearestView(views, method);
				if (nearest != null)
				{
					var properties = ImmutableDictionary.CreateBuilder<string, string>();
					properties.Add(ViewFieldNameProperty, nearest.Name);
					context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1005_TypoInViewDelegateName, 
						method.Locations.First(), properties.ToImmutable(), nearest.Name));
		        }
	        }
		}

	    private IFieldSymbol FindNearestView(IEnumerable<IFieldSymbol> views, IMethodSymbol method)
	    {
			string methodName = method.Name.ToLowerInvariant();
		    int minDistance = int.MaxValue;
		    IFieldSymbol nearest = null;
		    foreach (var view in views)
		    {
			    int distance = StringHelpers.LevenshteinDistance(methodName, view.Name.ToLowerInvariant());
			    if (distance <= MaximumDistance && distance < minDistance)
			    {
				    minDistance = distance;
				    nearest = view;
			    }
		    }

		    return nearest;
	    }
	}
}
