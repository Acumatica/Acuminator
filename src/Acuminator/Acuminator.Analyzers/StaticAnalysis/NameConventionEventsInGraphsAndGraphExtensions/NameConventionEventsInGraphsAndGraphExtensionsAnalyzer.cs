using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.Symbols;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.NameConventionEventsInGraphsAndGraphExtensions
{
	public class NameConventionEventsInGraphsAndGraphExtensionsAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1041_NameConventionEventsInGraphsAndGraphExtensions);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) => 
			base.ShouldAnalyze(pxContext, graph) && graph.Type != GraphType.None;

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, PXGraphSemanticModel pxGraphOrExtension)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var graphOrExtensionWithEvents = PXGraphEventSemanticModel.EnrichGraphModelWithEvents(pxGraphOrExtension, symbolContext.CancellationToken);
			var allDeclaredNamingConventionEvents = from @event in graphOrExtensionWithEvents.GetAllEvents()
													where @event.SignatureType == EventHandlerSignatureType.Default &&
														  @event.Symbol.IsDeclaredInType(pxGraphOrExtension.Symbol)
													select @event;

			INamedTypeSymbol pxOverrideAttribute = pxContext.AttributeTypes.PXOverrideAttribute;

			foreach (GraphEventInfoBase eventInfo in allDeclaredNamingConventionEvents)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				if (IsSuitableForConversionToGenericSignature(eventInfo, pxContext, pxOverrideAttribute))
				{
					var graphEventLocation = eventInfo.Node.Identifier.GetLocation();

					symbolContext.ReportDiagnosticWithSuppressionCheck(
							Diagnostic.Create(Descriptors.PX1041_NameConventionEventsInGraphsAndGraphExtensions, graphEventLocation),
							pxContext.CodeAnalysisSettings);
				}
			}
		}

		private bool IsSuitableForConversionToGenericSignature(GraphEventInfoBase eventInfo, PXContext pxContext, INamedTypeSymbol pxOverrideAttribute)
		{
			// event handlers with more than 2 parameters should be overrides which shouldn't be converted to generic events
			// as well as C# overrides of base events
			if (eventInfo.Symbol.Parameters.Length > 2 || eventInfo.Symbol.IsOverride)
				return false;

			var eventAttributes	= eventInfo.Symbol.GetAttributes();

			// PXOverridden events can't be converted either
			if (!eventAttributes.IsDefaultOrEmpty && eventAttributes.Any(a => pxOverrideAttribute.Equals(a.AttributeClass)))
				return false;

			// check that there is a corresponding generic event args symbol
			var eventTypeInfoForGenericSignature = new EventInfo(eventInfo.EventType, EventHandlerSignatureType.Generic);
			return pxContext.Events.EventHandlerSignatureTypeMap.ContainsKey(eventTypeInfoForGenericSignature);
		}
	}
}
