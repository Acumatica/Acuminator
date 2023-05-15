﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public static partial class DacPropertyAndFieldSymbolUtils
	{
		/// <summary>
		/// Get corresponding DAC BQL field for a DAC field property <paramref name="property"/>.
		/// </summary>
		/// <param name="property">The property to act on. Must be a dac property.</param>
		/// <param name="pxContext">Acumatica Context.</param>
		/// <param name="checkContainingTypeIsDac">True for extra safety check that containing type is DAC or DAC extension.</param>
		/// <returns>
		/// The corresponding BQL field or null.
		/// </returns>
		public static INamedTypeSymbol? GetCorrespondingBqlField(this IPropertySymbol property, PXContext pxContext, bool checkContainingTypeIsDac)
		{
			property.ThrowOnNull(nameof(property));
			pxContext.ThrowOnNull(nameof(pxContext));

			var containingDacOrDacExt = property.ContainingType;

			if (containingDacOrDacExt == null ||
				(checkContainingTypeIsDac && !containingDacOrDacExt.IsDacOrExtension(pxContext)))
			{
				return null;
			}

			var mappedBqlField = GetBqlFieldFromTypeByName(containingDacOrDacExt, property.Name);

			if (mappedBqlField != null)
				return mappedBqlField;

			var currentType = containingDacOrDacExt.BaseType;

			while (currentType != null && currentType.IsDAC(pxContext))
			{
				mappedBqlField = GetBqlFieldFromTypeByName(currentType, property.Name);

				if (mappedBqlField != null)
					return mappedBqlField;

				currentType = currentType.BaseType;
			}

			return null;
		}

		private static INamedTypeSymbol? GetBqlFieldFromTypeByName(INamedTypeSymbol type, string caseInsensitiveName)
		{
			var members = type.GetMembers();

			if (members.IsDefaultOrEmpty)
				return null;

			return members.OfType<INamedTypeSymbol>()
						  .FirstOrDefault(bqlField => caseInsensitiveName.Equals(bqlField.Name, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Get the DAC fields symbols and syntax nodes from the DAC.
		/// </summary>
		/// <param name="dac">The DAC to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="includeFromInheritanceChain">(Optional) True to include, false to exclude the DAC fields from the inheritance chain.</param>
		/// <param name="cancellation">(Optional) Cancellation token.</param>
		/// <returns>
		/// The DAC fields from DAC.
		/// </returns>
		public static OverridableItemsCollection<DacFieldInfo> GetDacFieldsFromDac(this ITypeSymbol dac, PXContext pxContext, 
																				   bool includeFromInheritanceChain = true,
																				   CancellationToken cancellation = default)
		{
			dac.ThrowOnNull(nameof(dac));
			pxContext.ThrowOnNull(nameof(pxContext));

			if (!dac.IsDAC(pxContext))
				return new OverridableItemsCollection<DacFieldInfo>();

			int estimatedCapacity = dac.GetTypeMembers().Length;
			var dacFieldsByName = new OverridableItemsCollection<DacFieldInfo>(estimatedCapacity);
			var dacFields = GetRawDacFieldsFromDacImpl(dac, pxContext, includeFromInheritanceChain, cancellation);

			dacFieldsByName.AddRangeWithDeclarationOrder(dacFields, startingOrder: 0, 
														 (dacField, order) => new DacFieldInfo(dacField.Node, dacField.Symbol, order));
			return dacFieldsByName;
		}

		/// <summary>
		/// Get the DAC field symbols and syntax nodes from the DAC extension.
		/// </summary>
		/// <param name="dacExtension">The DAC extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns>
		/// The DAC fields from DAC extension and base DAC.
		/// </returns>
		public static OverridableItemsCollection<DacFieldInfo> GetDacFieldsFromDacExtensionAndBaseDac(this ITypeSymbol dacExtension, PXContext pxContext,
																									  CancellationToken cancellation = default)
		{
			dacExtension.ThrowOnNull(nameof(dacExtension));
			pxContext.ThrowOnNull(nameof(pxContext));

			return GetPropertiesOrFieldsInfoFromDacExtension<DacFieldInfo>(dacExtension, pxContext, AddFieldsFromDac, AddFieldsFromDacExtension);


			int AddFieldsFromDac(OverridableItemsCollection<DacFieldInfo> fieldsCollection, ITypeSymbol dac, int startingOrder)
			{
				var rawDacFields = dac.GetRawDacFieldsFromDacImpl(pxContext, includeFromInheritanceChain: true, cancellation);
				return fieldsCollection.AddRangeWithDeclarationOrder(rawDacFields, startingOrder, 
																	 (dacField, order) => new DacFieldInfo(dacField.Node, dacField.Symbol, order));
			}

			int AddFieldsFromDacExtension(OverridableItemsCollection<DacFieldInfo> fieldsCollection, ITypeSymbol dacExt, int startingOrder)
			{
				var rawDacExtensionFields = dacExt.GetRawDacFieldsFromDacOrDacExtension(pxContext, cancellation);
				return fieldsCollection.AddRangeWithDeclarationOrder(rawDacExtensionFields, startingOrder,
																	 (dacField, order) => new DacFieldInfo(dacField.Node, dacField.Symbol, order));
			}
		}

		private static IEnumerable<(ClassDeclarationSyntax Node, INamedTypeSymbol Symbol)> GetRawDacFieldsFromDacImpl(this ITypeSymbol dac,
																								PXContext pxContext, bool includeFromInheritanceChain,
																								CancellationToken cancellation)
		{
			if (includeFromInheritanceChain)
			{
				return dac.GetDacWithBaseTypes()
						  .Reverse()
						  .SelectMany(baseGraph => GetRawDacFieldsFromDacOrDacExtension(baseGraph, pxContext, cancellation));
			}
			else
			{
				return GetRawDacFieldsFromDacOrDacExtension(dac, pxContext, cancellation);
			}
		}

		private static IEnumerable<(ClassDeclarationSyntax Node, INamedTypeSymbol Symbol)> GetRawDacFieldsFromDacOrDacExtension(
																								this ITypeSymbol dacOrDacExtension,
																								PXContext pxContext, CancellationToken cancellation)
		{
			IEnumerable<INamedTypeSymbol> dacFields = dacOrDacExtension.GetTypeMembers()
																	   .Where(type => type.IsDacField(pxContext));												 
			foreach (INamedTypeSymbol field in dacFields)
			{
				cancellation.ThrowIfCancellationRequested();

				SyntaxReference reference = field.DeclaringSyntaxReferences.FirstOrDefault();

				if (reference?.GetSyntax(cancellation) is ClassDeclarationSyntax declaration)
				{
					yield return (declaration, field);
				}
			}
		}
	}
}
