#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
    public static class DacSymbolsUtils
    {
		/// <summary>
		/// Get the DAC type for a type symbol. Return <c>null</c> if <paramref name="type"/> is not a DAC and not a DAC extension.
		/// </summary>
		/// <param name="type">The type to act on.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <returns>
		/// The DAC type or <c>null</c> if <paramref name="type"/> is not a DAC and not a DAC extension.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DacType? GetDacType(this ITypeSymbol type, PXContext pxContext)
		{
			return type.CheckIfNull().IsDAC(pxContext)
				? DacType.Dac
				: type.IsDacExtension(pxContext)
					? DacType.DacExtension
					: null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDAC(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.BaseValidation())
				return false;

			if (typeSymbol.ImplementsInterface(TypeNames.IBqlTable))    //Should work for named types and type parameters in most cases
				return true;
			else if (typeSymbol is ITypeParameterSymbol typeParameterSymbol)    //fallback for type parameters when Roslyn can't correctly determine interfaces (see ATR-376)
			{
				return typeParameterSymbol.GetAllConstraintTypes()
										  .Any(constraint => constraint.ImplementsInterface(TypeNames.IBqlTable));
			}
			else
				return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDAC(this ITypeSymbol typeSymbol, PXContext pxContext)
		{
			typeSymbol.ThrowOnNull();

			if (typeSymbol.ImplementsInterface(pxContext.IBqlTableType))    //Should work for named types and type parameters in most cases
				return true;
			else if (typeSymbol is ITypeParameterSymbol typeParameterSymbol)    //fallback for type parameters when Roslyn can't correctly determine interfaces (see ATR-376)
			{
				return typeParameterSymbol.GetAllConstraintTypes()
										  .Any(constraint => constraint.ImplementsInterface(pxContext.IBqlTableType));
			}
			else
				return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDacOrExtension(this ITypeSymbol typeSymbol, PXContext pxContext) => 
			typeSymbol.IsDAC(pxContext) || typeSymbol.IsDacExtension(pxContext);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDacExtension(this ITypeSymbol typeSymbol, PXContext pxContext) => 
			typeSymbol.CheckIfNull()
					  .InheritsFrom(pxContext.PXCacheExtensionType);

		/// <summary>
		/// An extension method that query if <paramref name="typeSymbol"/> is DAC extension.
		/// </summary>
		/// <param name="typeSymbol">The type symbol to act on.</param>
		/// <returns>
		/// True if DAC extension, false if not.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDacExtension(this ITypeSymbol typeSymbol) =>
			typeSymbol.BaseValidation()
				? typeSymbol.InheritsOrImplementsOrEquals(TypeNames.PXCacheExtension, includeInterfaces: false)
				: false;
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDacBqlField(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.BaseValidation())
				return false;
			else if (typeSymbol.ImplementsInterface(TypeNames.BqlField.IBqlField))       //Should work for named types and type parameters in most cases
				return true;
			else if (typeSymbol is ITypeParameterSymbol typeParameterSymbol)    //fallback for type parameters when Roslyn can't correctly determine interfaces (see ATR-376)
				return typeParameterSymbol.GetAllConstraintTypes()
										  .Any(constraint => constraint.ImplementsInterface(TypeNames.BqlField.IBqlField));
			else
				return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDacBqlField(this ITypeSymbol typeSymbol, PXContext pxContext)
		{
			typeSymbol.ThrowOnNull();

			if (typeSymbol.ImplementsInterface(pxContext.IBqlFieldType))    
				return true;
			else if (typeSymbol is ITypeParameterSymbol typeParameterSymbol)    //fallback for type parameters when Roslyn can't correctly determine interfaces (see ATR-376)
			{
				return typeParameterSymbol.GetAllConstraintTypes()
										  .Any(constraint => constraint.ImplementsInterface(pxContext.IBqlFieldType));
			}
			else
				return false;
		}

		public static bool IsStronglyTypedBqlFieldOrBqlConstant(this INamedTypeSymbol bqlFieldOrBqlConstantType, PXContext pxContext)
		{
			pxContext.ThrowOnNull();

			var allInterfaces = bqlFieldOrBqlConstantType.CheckIfNull().AllInterfaces;

			if (allInterfaces.IsDefaultOrEmpty)
				return false;

			foreach (INamedTypeSymbol @interface in allInterfaces)
			{
				if (!@interface.IsGenericType || @interface.TypeArguments.IsDefaultOrEmpty)
					continue;

				if (!@interface.Equals(pxContext.IImplementType, SymbolEqualityComparer.Default) &&
					(@interface.OriginalDefinition == null || 
					 !@interface.OriginalDefinition.Equals(pxContext.IImplementType, SymbolEqualityComparer.Default)))
				{
					continue;
				}

				var firstTypeArgument = @interface.TypeArguments[0];

				if (firstTypeArgument.ImplementsInterface(pxContext.BqlTypes.IBqlDataType))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Get view's DAC for which the view was declared.
		/// </summary>
		/// <param name="pxView">The view to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns>
		/// The DAC from view.
		/// </returns>
		public static ITypeSymbol? GetDacFromView(this ITypeSymbol pxView, PXContext pxContext)
		{
			pxContext.ThrowOnNull();

			if (pxView?.InheritsFrom(pxContext.PXSelectBase.Type) != true)
				return null;

			INamedTypeSymbol baseViewType;

			if (pxView.IsFbqlView(pxContext))
			{

				baseViewType = pxView.BaseType!.ContainingType
											   .GetBaseTypesAndThis()
											   .OfType<INamedTypeSymbol>()
											   .FirstOrDefault(t => t.OriginalDefinition.Equals(pxContext.BQL.PXViewOf, SymbolEqualityComparer.Default));
			}
			else
			{
				baseViewType = pxView.GetBaseTypesAndThis()
									 .OfType<INamedTypeSymbol>()
									 .FirstOrDefault(type => !type.IsCustomBqlCommand(pxContext));

				if (baseViewType?.IsBqlCommand() != true)
					return null;
			}

			if (baseViewType == null || baseViewType.TypeArguments.Length == 0)
			{
				return null;
			}

			return baseViewType.TypeArguments[0];
		}

		/// <summary>
		/// Get action's DAC for which the action was declared.
		/// </summary>
		/// <param name="pxAction">The action to act on.</param>
		/// <returns>
		/// The DAC from action.
		/// </returns>
		public static ITypeSymbol? GetDacFromAction(this INamedTypeSymbol pxAction)
		{
			if (pxAction?.IsPXAction() != true)
				return null;

			ImmutableArray<ITypeSymbol> actionTypeArgs = pxAction.TypeArguments;

			if (actionTypeArgs.Length == 0)
				return null;

			ITypeSymbol pxActionDacType = actionTypeArgs[0];
			return pxActionDacType.IsDAC()
				? pxActionDacType
				: null;
		}

		/// <summary>
		/// Gets the DAC type from DAC extension type.
		/// </summary>
		/// <param name="dacExtension">The DAC extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns>
		/// The DAC from DAC extension.
		/// </returns>
		public static ITypeSymbol? GetDacFromDacExtension(this ITypeSymbol dacExtension, PXContext pxContext)
		{
			pxContext.ThrowOnNull();

			if (dacExtension == null || !dacExtension.IsDacExtension(pxContext))
				return null;

			var baseDacExtensionType = dacExtension.GetBaseTypesAndThis()
												   .OfType<INamedTypeSymbol>()
												   .FirstOrDefault(type => type.IsDacExtensionBaseType());
			if (baseDacExtensionType == null)
				return null;

			var dacExtTypeArgs = baseDacExtensionType.TypeArguments;

			if (dacExtTypeArgs.Length == 0)
				return null;

			ITypeSymbol dacType = dacExtTypeArgs.Last();
			return dacType.IsDAC(pxContext)
				? dacType
				: null;
		}

		/// <summary>
		/// A base validation for <paramref name="typeSymbol"/> to see if it can be a DAC/DAC extension/DAC field.
		/// </summary>
		/// <param name="typeSymbol">The typeSymbol to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool BaseValidation(this ITypeSymbol typeSymbol)
		{
			if (typeSymbol == null)
				return false;

			return typeSymbol.TypeKind == TypeKind.Class ||
				   typeSymbol.TypeKind == TypeKind.TypeParameter ||
				   typeSymbol.TypeKind == TypeKind.Interface ||
				   typeSymbol.TypeKind == TypeKind.Unknown;
		}


		/// <summary>
		/// Check if <paramref name="dac"/> is projection DAC.
		/// </summary>
		/// <param name="dac">The DAC type to act on.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <param name="checkTypeIsDac">True to check that type is DAC.</param>
		/// <returns>
		/// True if <paramref name="dac"/> is a projection dac, false if not.
		/// </returns>
		public static bool IsProjectionDac(this ITypeSymbol dac, PXContext pxContext, bool checkTypeIsDac)
		{
			var projectionAttributeApplication = GetProjectionAttributeApplication(dac, pxContext, checkTypeIsDac);
			return projectionAttributeApplication != null;
		}


		/// <summary>
		/// Get the projection attribute application from <paramref name="dac"/>.
		/// </summary>
		/// <param name="dac">The DAC type to act on.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <param name="checkTypeIsDac">True to check that type is DAC.</param>
		/// <returns>
		/// The application of a projection attribute.
		/// </returns>
		public static AttributeData? GetProjectionAttributeApplication(this ITypeSymbol dac, PXContext pxContext, bool checkTypeIsDac)
		{
			pxContext.ThrowOnNull();
			dac.ThrowOnNull();

			if (checkTypeIsDac && !dac.IsDAC(pxContext))
				return null;

			var projectionAttribute = pxContext.AttributeTypes.PXProjectionAttribute;

			if (projectionAttribute == null)
				return null;

			var attributes = dac.GetAllAttributesApplicationsDefinedOnThisAndBaseTypes();
			var projectionAttributeApplication = attributes.FirstOrDefault(a => a.AttributeClass != null && a.AttributeClass.InheritsFromOrEquals(projectionAttribute));
			return projectionAttributeApplication;
		}
	}
}
