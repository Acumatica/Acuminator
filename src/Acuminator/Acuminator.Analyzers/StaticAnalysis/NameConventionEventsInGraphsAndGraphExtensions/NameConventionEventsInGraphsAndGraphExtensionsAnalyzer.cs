#nullable enable

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

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, PXGraphEventSemanticModel graphOrExtensionWithEvents)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var allDeclaredNamingConventionEvents = from @event in graphOrExtensionWithEvents.GetAllEvents()
													where @event.SignatureType == EventHandlerSignatureType.Default &&
														  @event.Symbol.IsDeclaredInType(graphOrExtensionWithEvents.Symbol)
													select @event;

			INamedTypeSymbol pxOverrideAttribute = pxContext.AttributeTypes.PXOverrideAttribute;

			foreach (GraphEventInfoBase eventInfo in allDeclaredNamingConventionEvents)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				if (IsSuitableForConversionToGenericSignature(eventInfo, pxContext, pxOverrideAttribute))
				{
					ReportDiagnosticForEvent(symbolContext, pxContext, eventInfo);
				}
			}
		}

		private static void ReportDiagnosticForEvent(SymbolAnalysisContext symbolContext, PXContext pxContext, GraphEventInfoBase eventInfo)
		{
			// Node is not null here because aggregated graph analyzers work only on graphs and graph extensions declared in the source code,
			// and only events declared in the graph or graph extension are analyzed
			var graphEventLocation = eventInfo.Node!.Identifier.GetLocation();
			var properties = new Dictionary<string, string>
			{
				{ NameConventionEventsInGraphsAndGraphExtensionsDiagnosticProperties.EventType, eventInfo.EventType.ToString() },
				{ DiagnosticProperty.DacName, eventInfo.DacName }
			};

			if (eventInfo is GraphFieldEventInfo graphFieldEvent)
				properties.Add(DiagnosticProperty.DacFieldName, graphFieldEvent.DacFieldName);

			symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1041_NameConventionEventsInGraphsAndGraphExtensions, graphEventLocation,
									  properties: properties.ToImmutableDictionary()),
					pxContext.CodeAnalysisSettings);
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
