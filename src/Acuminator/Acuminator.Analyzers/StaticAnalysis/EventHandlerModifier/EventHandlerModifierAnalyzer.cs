using System.Collections.Immutable;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PrivateEventHandlers
{
	public class EventHandlerModifierAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
				Descriptors.PX1077_EventHandlersShouldNotBePrivate,
				Descriptors.PX1078_EventHandlersShouldBeProtectedVirtual,
				Descriptors.PX1078_EventHandlersShouldNotBeSealed);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			foreach (var handler in graphExtension.GetAllEvents().Where(graphEvent => graphEvent.Symbol.IsDeclaredInType(graphExtension.Symbol)))
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (handler.Symbol.DeclaredAccessibility == Accessibility.Private)
				{
					// For private modifiers, there is no need to check if the method is overriden. It's a %100 violation.
					context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1077_EventHandlersShouldNotBePrivate, handler.Symbol.Locations.First()));
				}

				if (handler.Symbol.IsOverride)
				{
					// If the method is overriden, the only action can be to remove the sealed keyword.
					// In general, the OOP related issues will be handled anyway by the compiler. We don't need to get involved.

					if (handler.Symbol.IsSealed)
					{
						context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1078_EventHandlersShouldNotBeSealed, handler.Symbol.Locations.First()));
					}
				}
				else
				{
					// The method should
					// - be protected
					// - be virtual (override is not possible at this point, the method is not an override)
					// - not be sealed
					if (!handler.Symbol.IsVirtual || handler.Symbol.IsSealed || handler.Symbol.DeclaredAccessibility != Accessibility.Protected)
					{
						context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1078_EventHandlersShouldBeProtectedVirtual, handler.Symbol.Locations.First()));
					}
				}
			}
		}
	}
}