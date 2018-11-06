using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.TypoInViewDelegateName
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
	        if (parent != null && parent.InheritsFrom(pxContext.PXGraph.Type))
	        {
		        var views = parent.GetMembers()
					.OfType<IFieldSymbol>()
					.Where(f => f.Type.InheritsFrom(pxContext.PXSelectBase.Type))
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
			    int distance = StringExtensions.LevenshteinDistance(methodName, view.Name.ToLowerInvariant());

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
