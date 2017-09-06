using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PX.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class InvalidPXActionSignatureAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1000_InvalidPXActionHandlerSignature);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzeMethod(c, pxContext), SymbolKind.Method);
		}

		private static void AnalyzeMethod(SymbolAnalysisContext context, PXContext pxContext)
		{
			var method = (IMethodSymbol) context.Symbol;
			var parent = method.ContainingType;
			if (parent != null && parent.InheritsFrom(pxContext.PXGraphType))
			{
				var field = parent.GetMembers()
					.OfType<IFieldSymbol>()
					.FirstOrDefault(f => f.Type.InheritsFrom(pxContext.PXActionType)
								&& String.Equals(f.Name, method.Name, StringComparison.OrdinalIgnoreCase));

				if (field != null)
				{
					if (method.ReturnType.SpecialType == SpecialType.System_Collections_IEnumerable
						&& (method.Parameters.Length != 1 || !method.Parameters[0].Type.Equals(pxContext.PXAdapterType))
						|| method.ReturnsVoid && method.Parameters.Length > 0)
					{
						context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1000_InvalidPXActionHandlerSignature, method.Locations.First()));
					}
				}
			}
		}
	}
}