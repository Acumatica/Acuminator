using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
		
		private static readonly Dictionary<SyntaxKind, string> ModifierText = new()
		{
			{ SyntaxKind.PublicKeyword, "public" },
			{ SyntaxKind.ProtectedKeyword, "protected" },
			{ SyntaxKind.VirtualKeyword, "virtual" }
		};

		private static readonly Dictionary<Accessibility, SyntaxKind> AccessibilitySyntaxKindMap = new()
		{
			{ Accessibility.Public, SyntaxKind.PublicKeyword },
			{ Accessibility.Protected, SyntaxKind.ProtectedKeyword }
		};

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphOrGraphExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			
			var declaredEventHandlers = graphOrGraphExtension
				.GetAllEvents()
				.Where(graphEvent => graphEvent.Symbol.IsDeclaredInType(graphOrGraphExtension.Symbol));

			var allInterfaceMethods = graphOrGraphExtension.Symbol.AllInterfaces
																  .SelectMany(im => im.GetMethods())
																  .Where(im => im.GetEventHandlerType(pxContext) != EventType.None)
																  .ToList();
			foreach (var handler in declaredEventHandlers)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				var location = handler.Symbol.Locations.FirstOrDefault();

				if (location == null || handler.Symbol.IsOverride)
				{
					continue;
				}

				if (handler.Symbol.MethodKind == MethodKind.ExplicitInterfaceImplementation)
				{
					context.ReportDiagnosticWithSuppressionCheck(
								Diagnostic.Create(Descriptors.PX1077_EventHandlersShouldNotBeExplicitInterfaceImplementations, location),
								pxContext.CodeAnalysisSettings);
				}
				else if (!IsImplicitInterfaceImplementation(handler.Symbol, allInterfaceMethods))
				{
					var targetAccessibility = handler.Symbol.ContainingType.IsSealed ? Accessibility.Public : Accessibility.Protected;
					var addVirtualModifier = !handler.Symbol.ContainingType.IsSealed && !handler.Symbol.IsAbstract;

					var properties = new Dictionary<string, string?>
					{
						{ PX1077DiagnosticProperty.IsContainingTypeSealed, handler.Symbol.ContainingType.IsSealed.ToString() },
						{ PX1077DiagnosticProperty.AddVirtualModifier	 , addVirtualModifier.ToString() },
						{ DiagnosticProperty.RegisterCodeFix			 , bool.TrueString }
					}
					.ToImmutableDictionary();

					if ((!handler.Symbol.IsVirtual && addVirtualModifier) || handler.Symbol.DeclaredAccessibility != targetAccessibility)
					{
						if (handler.Symbol.DeclaredAccessibility == Accessibility.Private)
						{
							context.ReportDiagnosticWithSuppressionCheck(
								Diagnostic.Create(Descriptors.PX1077_EventHandlersShouldNotBePrivate, location, properties), 
								pxContext.CodeAnalysisSettings);
						}
						else
						{
							var modifierText = GetModifierFormatArg(AccessibilitySyntaxKindMap[targetAccessibility], addVirtualModifier);
							context.ReportDiagnosticWithSuppressionCheck(
								Diagnostic.Create(Descriptors.PX1077_EventHandlersShouldBeProtectedVirtual, location, properties, modifierText),
								pxContext.CodeAnalysisSettings);
						}
					}
				}
			}
		}

		internal static string GetModifierFormatArg(SyntaxKind accessibilityModifier, bool addVirtualModifier)
		{
			var modifierFormatArg = ModifierText[accessibilityModifier];

			if (addVirtualModifier)
			{
				modifierFormatArg += " " + ModifierText[SyntaxKind.VirtualKeyword];
			}

			return modifierFormatArg;
		}

		private static bool IsImplicitInterfaceImplementation(IMethodSymbol method, List<IMethodSymbol> allInterfaceMethods)
		{
			return allInterfaceMethods.Any(im =>
				method.ContainingType.FindImplementationForInterfaceMember(im)?.Equals(method, SymbolEqualityComparer.Default) ?? false);
		}
	}
}