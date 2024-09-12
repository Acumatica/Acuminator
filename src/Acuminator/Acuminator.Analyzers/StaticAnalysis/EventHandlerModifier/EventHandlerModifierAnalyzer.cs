using System.Collections.Immutable;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;

namespace Acuminator.Analyzers.StaticAnalysis.PrivateEventHandlers
{
	public class EventHandlerModifierAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
				Descriptors.PX1077_EventHandlersShouldBeProtectedVirtual,
				Descriptors.PX1077_EventHandlersShouldNotBeExplicitInterfaceImplementations);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			foreach (var handler in graphExtension.GetAllEvents().Where(graphEvent => graphEvent.Symbol.IsDeclaredInType(graphExtension.Symbol)))
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
					{ "IsOverride",							handler.Symbol.IsOverride.ToString()},
					{ "IsExplicitInterfaceImplementation", (handler.Symbol.MethodKind == MethodKind.ExplicitInterfaceImplementation).ToString()},
					{ "ImplementsInterface",				HelperMethods.ImplementsInterface(handler.Symbol, pxContext).ToString()},
					{ "IsContainingTypeSealed",				handler.Symbol.ContainingType.IsSealed.ToString()}
				}
				.ToImmutableDictionary();

				if (handler.Symbol.MethodKind == MethodKind.ExplicitInterfaceImplementation)
				{
					context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1077_EventHandlersShouldNotBeExplicitInterfaceImplementations, location, properties));
				}
				else if (!HelperMethods.ImplementsInterface(handler.Symbol, pxContext))
				{
					var targetAccessibility = handler.Symbol.ContainingType.IsSealed ? Accessibility.Public : Accessibility.Protected;

					if (!(handler.Symbol.IsVirtual || handler.Symbol.ContainingType.IsSealed) || handler.Symbol.DeclaredAccessibility != targetAccessibility)
					{
						context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1077_EventHandlersShouldBeProtectedVirtual, location, properties));
					}
				}
			}
		}

		private static class HelperMethods
		{
			internal static bool ImplementsInterface(IMethodSymbol method, PXContext pxContext)
			{
				return method
					.ContainingType
					.AllInterfaces
					.SelectMany(i => i.GetMethods(method.Name))
					.Any(m =>
						IsEventHandlerInfosMatch(method, m, pxContext) &&
						IsImplicitInterfaceImplementation(method, m)
					);
			}

			private static bool IsEventHandlerInfosMatch(IMethodSymbol method, IMethodSymbol other, PXContext pxContext)
			{
				return method.GetEventHandlerInfo(pxContext).Equals(other.GetEventHandlerInfo(pxContext));
			}

			private static bool IsImplicitInterfaceImplementation(IMethodSymbol method, IMethodSymbol interfaceMethod)
			{
				if (method.Name != interfaceMethod.Name)
				{
					return false;
				}

				if (method.Parameters.Length != interfaceMethod.Parameters.Length)
				{
					return false;
				}

				if (!method.ReturnType.Equals(interfaceMethod.ReturnType, SymbolEqualityComparer.Default))
				{
					return false;
				}

				if (method.Arity != interfaceMethod.Arity)
				{
					return false;
				}

				if (method.IsGenericMethod != interfaceMethod.IsGenericMethod)
				{
					return false;
				}

				if (method.IsGenericMethod)
				{
					if (method.TypeArguments.Length != interfaceMethod.TypeArguments.Length)
					{
						return false;
					}

					for (int i = 0; i < method.TypeArguments.Length; i++)
					{
						if (!method.TypeArguments[i].Equals(interfaceMethod.TypeArguments[i], SymbolEqualityComparer.Default))
						{
							return false;
						}
					}
				}

				for (int i = 0; i < method.Parameters.Length; i++)
				{
					if (!method.Parameters[i].Type.Equals(interfaceMethod.Parameters[i].Type, SymbolEqualityComparer.Default))
					{
						return false;
					}
				}

				var implicitImplementation = method.ContainingType.FindImplementationForInterfaceMember(interfaceMethod);

				return implicitImplementation?.Equals(method, SymbolEqualityComparer.Default) ?? false;
			}
		}
	}
}