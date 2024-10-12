using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ForbidPrivateEventHandlers
{
	public class ForbidPrivateEventHandlersAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
				Descriptors.PX1077_EventHandlersShouldNotBePrivate,
				Descriptors.PX1077_EventHandlersShouldBeProtectedVirtual,
				Descriptors.PX1077_EventHandlersShouldNotBeExplicitInterfaceImplementations);
		
		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphOrGraphExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			
			var declaredEventHandlers = graphOrGraphExtension.GetAllEvents()
															 .Where(graphEvent => graphEvent.Symbol.IsDeclaredInType(graphOrGraphExtension.Symbol));

			List<IMethodSymbol>? allInterfaceMethodsImplementations = GetAllInterfaceMethodsImplementations(graphOrGraphExtension.Symbol, pxContext);

			foreach (var handler in declaredEventHandlers)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				AnalyzeEventHandler(context, pxContext, handler, allInterfaceMethodsImplementations);
			}
		}

		private static List<IMethodSymbol>? GetAllInterfaceMethodsImplementations(INamedTypeSymbol graphOrGraphExtSymbol, PXContext pxContext)
		{
			var allInterfaces = graphOrGraphExtSymbol.AllInterfaces;

			if (allInterfaces.IsDefaultOrEmpty)
				return null;

			var allInterfaceMethods = allInterfaces.SelectMany(@interface => @interface.GetMethods())
												   .Where(iMethod => iMethod.GetEventHandlerType(pxContext) != EventType.None);

			var allInterfaceMethodsImplementations = allInterfaceMethods.Select(graphOrGraphExtSymbol.FindImplementationForInterfaceMember)
																		.OfType<IMethodSymbol>()    //removes null
																		.ToList();
			return allInterfaceMethodsImplementations;
		}

		private static void AnalyzeEventHandler(SymbolAnalysisContext context, PXContext pxContext, GraphEventInfoBase handler, 
												List<IMethodSymbol>? allInterfaceMethodsImplementations)
		{
			var location = handler.Symbol.Locations.FirstOrDefault();

			if (location == null || handler.Symbol.IsOverride)  // Do not report C# overrides of event handlers because their signature cannot be changed
				return;

			if (handler.Symbol.MethodKind == MethodKind.ExplicitInterfaceImplementation)
			{
				context.ReportDiagnosticWithSuppressionCheck(
							Diagnostic.Create(Descriptors.PX1077_EventHandlersShouldNotBeExplicitInterfaceImplementations, location),
							pxContext.CodeAnalysisSettings);
				return;
			}
			else if (IsImplicitInterfaceImplementation(handler.Symbol, allInterfaceMethodsImplementations))
				return;

			var targetAccessibility = handler.Symbol.ContainingType.IsSealed ? Accessibility.Public : Accessibility.Protected;
			var addVirtualModifier = !handler.Symbol.ContainingType.IsSealed && !handler.Symbol.IsAbstract;

			if ((handler.Symbol.IsVirtual || !addVirtualModifier) && handler.Symbol.DeclaredAccessibility == targetAccessibility)
				return;

			var properties = new Dictionary<string, string?>
			{
				{ PX1077DiagnosticProperty.IsContainingTypeSealed, handler.Symbol.ContainingType.IsSealed.ToString() },
				{ PX1077DiagnosticProperty.AddVirtualModifier    , addVirtualModifier.ToString() },
				{ DiagnosticProperty.RegisterCodeFix             , bool.TrueString }
			}
			.ToImmutableDictionary();

			if (handler.Symbol.DeclaredAccessibility == Accessibility.Private)
			{
				context.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1077_EventHandlersShouldNotBePrivate, location, properties),
					pxContext.CodeAnalysisSettings);
			}
			else
			{
				var modifiersText = GetModifiersText(isPublic: targetAccessibility == Accessibility.Public, addVirtualModifier);
				context.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1077_EventHandlersShouldBeProtectedVirtual, location, properties, modifiersText),
					pxContext.CodeAnalysisSettings);
			}
		}

		private static bool IsImplicitInterfaceImplementation(IMethodSymbol method, List<IMethodSymbol>? allInterfaceMethodsImplementations)
		{
			if (allInterfaceMethodsImplementations?.Count is null or 0)
				return false;

			return allInterfaceMethodsImplementations.Any(
						interfaceMethodImplementation => interfaceMethodImplementation.Equals(method, SymbolEqualityComparer.Default));
		}

		internal static string GetModifiersText(bool isPublic, bool addVirtualModifier)
		{
			const string publicModifierString = "public";
			const string protectedModifierString = "protected";
			const string protectedVirtualModifierString = "protected virtual";

			return isPublic
				? publicModifierString
				: addVirtualModifier
					? protectedVirtualModifierString
					: protectedModifierString;
		}
	}
}