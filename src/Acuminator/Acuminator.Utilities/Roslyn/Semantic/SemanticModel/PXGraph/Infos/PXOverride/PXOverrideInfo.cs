using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Semantic information about the PXOverride method. 
	/// </summary>
	public class PXOverrideInfo : SymbolItem<IMethodSymbol>
	{
		public PXOverrideType OverrideType { get; }

		/// <summary>
		/// The base method overridden by the PXOverride method.
		/// </summary>
		public IMethodSymbol? BaseMethod { get; }

		public PXOverrideInfo(IMethodSymbol symbol, PXOverrideType pxOverrideType, IMethodSymbol? baseMethod, int declarationOrder) : 
						base(symbol, declarationOrder)
		{
			if (pxOverrideType == PXOverrideType.None)
				throw new ArgumentOutOfRangeException(nameof(pxOverrideType), pxOverrideType, $"PXOverride type must not be {PXOverrideType.None}");

			OverrideType = pxOverrideType;
			BaseMethod = baseMethod;
		}

		internal static IEnumerable<PXOverrideInfo> GetDeclaredPXOverrides(GraphExtensionInfo graphExtensionInfo, PXContext context, 
																		   CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();
			var pxOverrideAttribute = context.AttributeTypes.PXOverrideAttribute;

			if (pxOverrideAttribute == null)
				yield break;

			var directBaseTypesAndThis = graphExtensionInfo.Symbol.GetBaseTypesAndThis()
																  .ToList(capacity: 4);
			var graphAndGraphExtensionBaseTypes = graphExtensionInfo.GetInfosFromDerivedExtensionToBaseGraph(includeSelf: false)
																	.Select(info => info.Symbol)
																	.Distinct<ITypeSymbol>(SymbolEqualityComparer.Default)
																	.Where(baseType => !directBaseTypesAndThis.Contains(baseType, SymbolEqualityComparer.Default))
																	.ToList(capacity: 4);

			var declaredMethods = graphExtensionInfo.Symbol.GetMethods();
			int declarationOrder = 0;

			foreach (IMethodSymbol method in declaredMethods)
			{
				cancellation.ThrowIfCancellationRequested();

				if (!method.HasPXOverrideAttribute(pxOverrideAttribute))
					continue;

				var baseMethod = GetBaseMethod(method, graphAndGraphExtensionBaseTypes, pxOverrideAttribute);
				var pxOverrideType = GetPXOverrideType(method, baseMethod);

				if (pxOverrideType != PXOverrideType.None)
				{
					yield return new PXOverrideInfo(method, pxOverrideType, baseMethod, declarationOrder);
					declarationOrder++;
				}
			}
		}

		private static IMethodSymbol? GetBaseMethod(IMethodSymbol patchMethodWithPXOverride, List<ITypeSymbol> graphAndGraphExtensionBaseTypes,
													INamedTypeSymbol pxOverrideAttribute)
		{
			var suitableBaseMethods = new List<IMethodSymbol>(capacity: 4);

			foreach (var baseType in graphAndGraphExtensionBaseTypes)
			{
				// Base method search should look for suitable methods in the base type hierarchy from the most derived type to the least derived type.
				// The base method is the one with the suitable signature that is not marked with PXOverrideAttribute.
				// The additional condition is required to filter out overrides of the base method in base extensions.
				var baseMethod = baseType.GetMethods(patchMethodWithPXOverride.Name)
										 .FirstOrDefault(baseMethod => patchMethodWithPXOverride.IsPXOverrideOf(baseMethod) && 
																	  !baseMethod.HasPXOverrideAttribute(pxOverrideAttribute));
				if (baseMethod != null)
				{
					bool alreadyFoundCSharpOverride = 
						suitableBaseMethods.Count > 0 && 
						suitableBaseMethods.Any(foundMethod => foundMethod.IsOverride && 
															   foundMethod.GetOverridden().Contains(baseMethod, SymbolEqualityComparer.Default));

					// Do not add the base method if a more derived C# override is already found
					if (!alreadyFoundCSharpOverride)
						suitableBaseMethods.Add(baseMethod);
				}
			}

			// If there are multiple suitable base methods found, we cannot determine which one is the correct base method for PXOverride
			// Thus, we return null in this case
			return suitableBaseMethods.Count == 1
				? suitableBaseMethods[0]
				: null;
		}

		private static PXOverrideType GetPXOverrideType(IMethodSymbol patchMethodWithPXOverride, IMethodSymbol? baseMethod)
		{
			var methodParameters = patchMethodWithPXOverride.Parameters;

			if (methodParameters.IsDefaultOrEmpty)
				return PXOverrideType.WithoutBaseDelegate;

			var lastParameter = methodParameters[^1];

			if (lastParameter.Type.TypeKind != TypeKind.Delegate)
				return PXOverrideType.WithoutBaseDelegate;

			if (baseMethod != null)
			{
				int baseMethodParametersCount = baseMethod.Parameters.Length;

				if (baseMethodParametersCount == methodParameters.Length)
					return PXOverrideType.WithoutBaseDelegate;
				else if (baseMethodParametersCount == (methodParameters.Length - 1))
					return PXOverrideType.WithValidBaseDelegate;
			}

			return patchMethodWithPXOverride.HasValidBaseDelegateParameter()
				? PXOverrideType.WithValidBaseDelegate
				: PXOverrideType.WithInvalidBaseDelegate;
		}
	}
}
