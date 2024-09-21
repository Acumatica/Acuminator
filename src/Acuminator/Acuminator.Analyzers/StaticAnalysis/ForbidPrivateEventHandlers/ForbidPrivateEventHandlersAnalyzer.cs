using System.Collections.Immutable;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

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

			var allInterfaceMethods = graphOrGraphExtension
				.Symbol
				.AllInterfaces
				.SelectMany(im => im.GetMethods())
				.Where(im => im.GetEventHandlerType(pxContext) != EventType.None)
				.ToList();

			foreach (var handler in declaredEventHandlers)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				var location = handler.Symbol.Locations.FirstOrDefault();

				if (location == null)
				{
					continue;
				}

				if (handler.Symbol.IsOverride)
				{
					continue;
				}

				var properties = new Dictionary<string, string?>
				{
					{ DiagnosticProperty.IsContainingTypeSealed, handler.Symbol.ContainingType.IsSealed.ToString() }
				};

				if (handler.Symbol.MethodKind == MethodKind.ExplicitInterfaceImplementation)
				{
					context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1077_EventHandlersShouldNotBeExplicitInterfaceImplementations, location, properties));
				}
				else if (!IsImplicitInterfaceImplementation(handler.Symbol, allInterfaceMethods))
				{
					var targetAccessibility = handler.Symbol.ContainingType.IsSealed ? Accessibility.Public : Accessibility.Protected;
					var addVirtualModifier = !handler.Symbol.ContainingType.IsSealed && !handler.Symbol.IsAbstract;

					properties.Add(DiagnosticProperty.AddVirtualModifier, addVirtualModifier.ToString());
					properties.Add(StaticAnalysis.DiagnosticProperty.RegisterCodeFix, true.ToString());

					if (!(handler.Symbol.IsVirtual || handler.Symbol.ContainingType.IsSealed || handler.Symbol.IsAbstract) || handler.Symbol.DeclaredAccessibility != targetAccessibility)
					{
						if (handler.Symbol.DeclaredAccessibility == Accessibility.Private)
						{
							context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1077_EventHandlersShouldNotBePrivate, location, properties.ToImmutableDictionary()));
						}
						else
						{
							var modifierText = GetModifierFormatArg(AccessibilitySyntaxKindMap[targetAccessibility], addVirtualModifier);
							context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1077_EventHandlersShouldBeProtectedVirtual, location, properties.ToImmutableDictionary(), modifierText));
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

	internal static class DiagnosticProperty
	{
		/// <summary>
		/// The property used to pass the information whether the containing type is sealed or not. 
		/// </summary>
		public const string IsContainingTypeSealed = nameof(IsContainingTypeSealed);

		/// <summary>
		/// The property used to pass the information whether to add virtual modifier or not. 
		/// </summary>
		public const string AddVirtualModifier = nameof(AddVirtualModifier);
	}
}