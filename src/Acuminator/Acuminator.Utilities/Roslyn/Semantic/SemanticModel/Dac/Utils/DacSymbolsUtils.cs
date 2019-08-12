using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
    public static class DacSymbolsUtils
    {
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
			typeSymbol.ThrowOnNull(nameof(typeSymbol));

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
			typeSymbol.CheckIfNull(nameof(typeSymbol))
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
		public static bool IsDacField(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.BaseValidation())
				return false;
			else if (typeSymbol.ImplementsInterface(TypeNames.IBqlField))       //Should work for named types and type parameters in most cases
				return true;
			else if (typeSymbol is ITypeParameterSymbol typeParameterSymbol)    //fallback for type parameters when Roslyn can't correctly determine interfaces (see ATR-376)
				return typeParameterSymbol.GetAllConstraintTypes()
										  .Any(constraint => constraint.ImplementsInterface(TypeNames.IBqlField));
			else
				return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDacField(this ITypeSymbol typeSymbol, PXContext pxContext)
		{
			typeSymbol.ThrowOnNull(nameof(typeSymbol));

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

		/// <summary>
		/// Get view's DAC for which the view was declared.
		/// </summary>
		/// <param name="pxView">The view to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns>
		/// The DAC from view.
		/// </returns>
		public static ITypeSymbol GetDacFromView(this ITypeSymbol pxView, PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (pxView?.InheritsFrom(pxContext.PXSelectBase.Type) != true)
				return null;

			INamedTypeSymbol baseViewType;

			if (pxView.IsFbqlView(pxContext))
			{

				baseViewType = pxView.BaseType.ContainingType.GetBaseTypesAndThis()
															 .OfType<INamedTypeSymbol>()
															 .FirstOrDefault(t => t.OriginalDefinition.Equals(pxContext.BQL.PXViewOf));
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
		public static ITypeSymbol GetDacFromAction(this INamedTypeSymbol pxAction)
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
		public static ITypeSymbol GetDacFromDacExtension(this ITypeSymbol dacExtension, PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

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
	}
}
