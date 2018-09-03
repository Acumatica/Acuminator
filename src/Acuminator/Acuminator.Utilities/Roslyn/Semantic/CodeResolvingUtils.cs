using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	public static class CodeResolvingUtils
	{
		/// <summary>
		/// An ITypeSymbol extension method that gets <see cref="PXCodeType"/> from identifier type symbol.
		/// </summary>
		/// <param name="identifierType">The identifierType to act on.</param>
		/// <param name="skipValidation">(Optional) True to skip validation.</param>
		/// <param name="checkItself">(Optional) True to check the type itself.</param>
		/// <returns>
		/// The coloring type from identifier.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PXCodeType? GetColoringTypeFromIdentifier(this ITypeSymbol identifierType, bool skipValidation = false,
																	 bool checkItself = false)
		{
			if (!skipValidation && !identifierType.IsValidForColoring())
				return null;

			IEnumerable<ITypeSymbol> typeHierarchy = checkItself
				? identifierType.GetBaseTypesAndThis()
				: identifierType.GetBaseTypes();

			typeHierarchy = typeHierarchy.ConcatStructList(identifierType.AllInterfaces);
			PXCodeType? resolvedColoredCodeType = null;

			foreach (ITypeSymbol typeOrInterface in typeHierarchy)
			{
				if (TypeNames.TypeNamesToCodeTypesForIdentifier.TryGetValue(typeOrInterface.Name, out PXCodeType coloredCodeType))
				{
					resolvedColoredCodeType = coloredCodeType;
					break;
				}
			}

			return resolvedColoredCodeType;
		}

		/// <summary>
		/// An ITypeSymbol extension method that gets <see cref="PXCodeType"/> from generic Name type symbol.
		/// </summary>
		/// <param name="genericName">The generic Name type symbol to act on.</param>
		/// <param name="genericNode">The generic node.</param>
		/// <returns>
		/// The code type from generic name.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PXCodeType? GetCodeTypeFromGenericName(this ITypeSymbol genericName)
		{
			if (!genericName.IsValidForColoring())
				return null;

			if (genericName.IsStatic && TypeNames.PXUpdateBqlTypes.Contains(genericName.Name))
				return PXCodeType.BqlCommand;

			IEnumerable<ITypeSymbol> typeHierarchy = genericName.GetBaseTypes()
																.ConcatStructList(genericName.AllInterfaces);
			PXCodeType? resolvedColoredCodeType = null;

			foreach (ITypeSymbol typeOrInterface in typeHierarchy)
			{
				if (TypeNames.TypeNamesToCodeTypesForGenericName.TryGetValue(typeOrInterface.Name, out PXCodeType coloredCodeType))
				{
					resolvedColoredCodeType = coloredCodeType;
					break;
				}
			}

			return resolvedColoredCodeType;
		}

		/// <summary>
		/// An <see cref="ITypeSymbol"/> extension method that query if <paramref name="typeSymbol"/> is valid for coloring.
		/// </summary>
		/// <param name="typeSymbol">The typeSymbol to act on.</param>
		/// <param name="checkForNotColoredTypes">(Optional) True to check <paramref name="typeSymbol"/> in a list of not colored types.
		/// By default is true to prevent coloring of base types and interfaces.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidForColoring(this ITypeSymbol typeSymbol, bool checkForNotColoredTypes = true)
		{
			if (typeSymbol == null)
				return false;

			if (checkForNotColoredTypes && typeSymbol.IsAbstract && TypeNames.NotColoredTypes.Contains(typeSymbol.Name))
				return false;

			switch (typeSymbol.TypeKind)
			{
				case TypeKind.Unknown:
				case TypeKind.Delegate:
				case TypeKind.Dynamic:
				case TypeKind.Enum:
				case TypeKind.Error:
				case TypeKind.Interface:
				case TypeKind.Module:
				case TypeKind.Pointer:
				case TypeKind.Submission:
					return false;
				default:
					return true;
			}
		}

		/// <summary>
		/// An ITypeSymbol extension method that query if 'typeSymbol' is bql command.
		/// </summary>
		/// <param name="typeSymbol">The typeSymbol to act on.</param>
		/// <param name="context">The context.</param>
		/// <returns>
		/// True if bql command, false if not.
		/// </returns>
		public static bool IsBqlCommand(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.IsValidForColoring(checkForNotColoredTypes: false))
				return false;

			if (typeSymbol.IsStatic && TypeNames.PXUpdateBqlTypes.Contains(typeSymbol.Name))
				return true;

			List<string> typeHierarchyNames = typeSymbol.GetBaseTypesAndThis()
														.Select(type => type.Name)
														.ToList();

			return typeHierarchyNames.Contains(TypeNames.PXSelectBaseType) ||
				   typeHierarchyNames.Contains(TypeNames.BqlCommand);
		}

		public static bool IsBqlCommand(this ITypeSymbol typeSymbol, PXContext pxContext)
		{
			if (!typeSymbol.IsValidForColoring(checkForNotColoredTypes: false))
				return false;

			if (pxContext == null)
				return typeSymbol.IsBqlCommand();
			else if (typeSymbol.IsStatic && TypeNames.PXUpdateBqlTypes.Contains(typeSymbol.Name))
				return true;

			List<ITypeSymbol> typeHierarchy = typeSymbol.GetBaseTypesAndThis().ToList();
			return typeHierarchy.Contains(pxContext.PXSelectBaseType) || typeHierarchy.Contains(pxContext.BQL.BqlCommand);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBqlParameter(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.IsValidForColoring(checkForNotColoredTypes: false))
				return false;

			return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.IBqlParameter);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBqlParameter(this ITypeSymbol typeSymbol, PXContext pxContext)
		{
			if (!typeSymbol.IsValidForColoring(checkForNotColoredTypes: false))
				return false;

			if (pxContext == null)
				return typeSymbol.IsBqlOperator();

			return typeSymbol.ImplementsInterface(pxContext.BQL.IBqlParameter);
		}

		public static bool IsBqlOperator(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.IsValidForColoring(checkForNotColoredTypes: false))
				return false;

			foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
			{
				if (interfaceSymbol.Name == TypeNames.IBqlCreator || interfaceSymbol.Name == TypeNames.IBqlJoin)
					return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDAC(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.IsValidForColoring())
				return false;

			return typeSymbol.ImplementsInterface(TypeNames.IBqlTable);
		}

        public static bool IsDacOrExtension(this ITypeSymbol typeSymbol, PXContext pxContext)
        {
            typeSymbol.ThrowOnNull(nameof(typeSymbol));
            pxContext.ThrowOnNull(nameof(pxContext));

            return typeSymbol.ImplementsInterface(pxContext.IBqlTableType) ||
                   typeSymbol.InheritsFrom(pxContext.PXCacheExtensionType);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDacExtension(this ITypeSymbol typeSymbol, bool ruleOutBaseTypes = false)
		{
			if (!typeSymbol.IsValidForColoring() || (ruleOutBaseTypes && string.Equals(typeSymbol.Name, TypeNames.PXCacheExtension)))
				return false;

			return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.PXCacheExtension, includeInterfaces: false);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDacField(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.IsValidForColoring())
				return false;

			return typeSymbol.ImplementsInterface(TypeNames.IBqlField);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBqlConstant(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.IsValidForColoring() || typeSymbol.Name.StartsWith(TypeNames.Constant))
				return false;

			return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.Constant, includeInterfaces: false);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPXGraph(this ITypeSymbol typeSymbol, bool ruleOutBaseTypes = false)
		{
			if (!typeSymbol.IsValidForColoring() || (ruleOutBaseTypes && string.Equals(typeSymbol.Name, TypeNames.PXGraph)))
				return false;

			return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.PXGraph, includeInterfaces: false);
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPXGraphOrExtension(this ITypeSymbol typeSymbol, PXContext pxContext)
        {
            typeSymbol.ThrowOnNull(nameof(typeSymbol));
            pxContext.ThrowOnNull(nameof(pxContext));

            return typeSymbol.InheritsFromOrEquals(pxContext.PXGraphType) ||
                   typeSymbol.InheritsFromOrEquals(pxContext.PXGraphExtensionType);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPXAction(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.IsValidForColoring())
				return false;

			return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.PXAction, includeInterfaces: false);
		}

		public static bool IsCustomBqlCommand(this ITypeSymbol bqlTypeSymbol, PXContext context)
		{
			bqlTypeSymbol.ThrowOnNull(nameof(bqlTypeSymbol));
			context.ThrowOnNull(nameof(context));

			int pxSelectBaseStandartDepth = context.IsAcumatica2018R2 ? 3 : 2;
			int? pxSelectBaseDepth = bqlTypeSymbol.GetInheritanceDepth(context.PXSelectBaseType);

			if (pxSelectBaseDepth > pxSelectBaseStandartDepth)
				return true;

			const int bqlCommandBaseStandartDepth = 2;
			int? bqlCommandDepth = bqlTypeSymbol.GetInheritanceDepth(context.BQL.BqlCommand);
			return bqlCommandDepth > bqlCommandBaseStandartDepth;
		}

		public static bool IsPXSelectReadOnlyCommand(this ITypeSymbol bqlTypeSymbol)
		{
			bqlTypeSymbol.ThrowOnNull(nameof(bqlTypeSymbol));

			if (!bqlTypeSymbol.IsBqlCommand())
				return false;

			return bqlTypeSymbol.GetBaseTypesAndThis()
								.Select(type => type.Name.Split('`').FirstOrDefault())
								.Any(typeName => TypeNames.ReadOnlySelects.Contains(typeName));
		}

		public static bool IsReadOnlyBqlCommand(this ITypeSymbol bqlTypeSymbol, PXContext context)
		{
			bqlTypeSymbol.ThrowOnNull(nameof(bqlTypeSymbol));
			context.ThrowOnNull(nameof(context));

			if (!bqlTypeSymbol.IsBqlCommand(context))
				return false;

			return bqlTypeSymbol.ImplementsInterface(context.BQL.IPXNonUpdateable);
		}

		public static bool IsPXSetupBqlCommand(this ITypeSymbol bqlTypeSymbol, PXContext context)
		{
			bqlTypeSymbol.ThrowOnNull(nameof(bqlTypeSymbol));
			context.ThrowOnNull(nameof(context));

			ImmutableArray<INamedTypeSymbol> setupTypes = context.BQL.GetPXSetupTypes();
			return bqlTypeSymbol.GetBaseTypesAndThis()
								.Any(type => setupTypes.Contains(type));
		}

		public static TextSpan? GetBqlOperatorOutliningTextSpan(this ITypeSymbol typeSymbol, GenericNameSyntax bqlOperatorNode)
		{
			List<string> operatorInterfaces = typeSymbol.AllInterfaces
														.Select(type => type.Name)
														.ToList();

			if (operatorInterfaces.Contains(TypeNames.IBqlComparison) ||
				operatorInterfaces.Contains(TypeNames.IBqlFunction) ||
				operatorInterfaces.Contains(TypeNames.IBqlSortColumn))
			{
				return null;
			}
			else if (operatorInterfaces.Contains(TypeNames.IBqlAggregate) ||
					 operatorInterfaces.Contains(TypeNames.IBqlOrderBy))
			{
				return bqlOperatorNode.TypeArgumentList.Span;
			}
			else if (operatorInterfaces.Contains(TypeNames.IBqlJoin))
			{
				return GetOutliningSpanForBqlJoin();
			}
			else if (!operatorInterfaces.Contains(TypeNames.IBqlPredicateChain) &&
					 !operatorInterfaces.Contains(TypeNames.IBqlOn))
			{
				return null;
			}

			TypeArgumentListSyntax typeParamsListSyntax = bqlOperatorNode.TypeArgumentList;
			var typeArgumentsList = typeParamsListSyntax.Arguments;
			
			switch (typeArgumentsList.Count)
			{
				case 0:
				case 1:	
					return typeParamsListSyntax.Span;

				case 2:			
					if (!(typeSymbol is INamedTypeSymbol namedTypeSymbol) || 
						!namedTypeSymbol.TypeArguments[0].ImplementsInterface(TypeNames.IBqlCreator))
					{
						return typeParamsListSyntax.Span;
					}

					int twoArgsLength = typeArgumentsList.GetSeparator(0).SpanStart - typeParamsListSyntax.SpanStart;
					return new TextSpan(typeParamsListSyntax.SpanStart, twoArgsLength);

				default:				
					int threeArgsLength = typeArgumentsList.GetSeparator(1).SpanStart - typeParamsListSyntax.SpanStart;
					return new TextSpan(typeParamsListSyntax.SpanStart, threeArgsLength);
			}

			//************************************Local Functions*************************************************
			TextSpan? GetOutliningSpanForBqlJoin()
			{
				TypeArgumentListSyntax bqlJoinTypeParamsListSyntax = bqlOperatorNode.TypeArgumentList;
				var bqlJoinTypeArgumentsList = bqlJoinTypeParamsListSyntax.Arguments;

				switch (bqlJoinTypeArgumentsList.Count)
				{
					case 0:
					case 1:
						return null;
					case 2:
						return bqlJoinTypeParamsListSyntax.Span;
					default:                                                                                                            //Has next Join => we emit extra outlining tag
						int length = bqlJoinTypeArgumentsList.GetSeparator(1).SpanStart - bqlJoinTypeParamsListSyntax.SpanStart;
						return new TextSpan(bqlJoinTypeParamsListSyntax.SpanStart, length);
				}
			}
		}

		/// <summary>
		/// Returns event handler type for the provided method symbol.
		/// </summary>
		/// <param name="symbol">Method symbol for the event handler</param>
		/// <param name="pxContext">PXContext instance</param>
		/// <returns>Event Type (e.g. RowSelecting). If method is not an event handler, returns <code>EventType.None</code>.</returns>
		public static EventType GetEventHandlerType(this IMethodSymbol symbol, PXContext pxContext)
		{
			if (symbol.ReturnsVoid && symbol.TypeParameters.IsEmpty && !symbol.Parameters.IsEmpty)
			{
				// Loosely check method signature because sometimes business logic from event handler calls is extracted to a separate method

				// Old syntax
				if (symbol.Parameters[0].Type.OriginalDefinition.InheritsFromOrEquals(pxContext.PXCacheType))
				{
					if (symbol.Name.EndsWith("CacheAttached", StringComparison.Ordinal))
						return EventType.CacheAttached;

					if (symbol.Parameters.Length >= 2)
					{
						switch (symbol.Parameters[1].Type.OriginalDefinition)
						{
							case var t when t.Equals(pxContext.Events.PXRowSelectingEventArgs): return EventType.RowSelecting;
							case var t when t.Equals(pxContext.Events.PXRowSelectedEventArgs): return EventType.RowSelected;
							case var t when t.Equals(pxContext.Events.PXRowInsertingEventArgs): return EventType.RowInserting;
							case var t when t.Equals(pxContext.Events.PXRowInsertedEventArgs): return EventType.RowInserted;
							case var t when t.Equals(pxContext.Events.PXRowUpdatingEventArgs): return EventType.RowUpdating;
							case var t when t.Equals(pxContext.Events.PXRowUpdatedEventArgs): return EventType.RowUpdated;
							case var t when t.Equals(pxContext.Events.PXRowDeletingEventArgs): return EventType.RowDeleting;
							case var t when t.Equals(pxContext.Events.PXRowDeletedEventArgs): return EventType.RowDeleted;
							case var t when t.Equals(pxContext.Events.PXRowPersistingEventArgs): return EventType.RowPersisting;
							case var t when t.Equals(pxContext.Events.PXRowPersistedEventArgs): return EventType.RowPersisted;
							case var t when t.Equals(pxContext.Events.PXFieldSelectingEventArgs): return EventType.FieldSelecting;
							case var t when t.Equals(pxContext.Events.PXFieldDefaultingEventArgs): return EventType.FieldDefaulting;
							case var t when t.Equals(pxContext.Events.PXFieldVerifyingEventArgs): return EventType.FieldVerifying;
							case var t when t.Equals(pxContext.Events.PXFieldUpdatingEventArgs): return EventType.FieldUpdating;
							case var t when t.Equals(pxContext.Events.PXFieldUpdatedEventArgs): return EventType.FieldUpdated;
							case var t when t.Equals(pxContext.Events.PXCommandPreparingEventArgs): return EventType.CommandPreparing;
							case var t when t.Equals(pxContext.Events.PXExceptionHandlingEventArgs): return EventType.ExceptionHandling;
						}
					}
				}
				else
				{
					// New generic event syntax
					switch (symbol.Parameters[0].Type.OriginalDefinition)
					{
						case var t when t.Equals(pxContext.Events.CacheAttached): return EventType.CacheAttached;
						case var t when t.Equals(pxContext.Events.RowSelecting): return EventType.RowSelecting;
						case var t when t.Equals(pxContext.Events.RowSelected): return EventType.RowSelected;
						case var t when t.Equals(pxContext.Events.RowInserting): return EventType.RowInserting;
						case var t when t.Equals(pxContext.Events.RowInserted): return EventType.RowInserted;
						case var t when t.Equals(pxContext.Events.RowUpdating): return EventType.RowUpdating;
						case var t when t.Equals(pxContext.Events.RowUpdated): return EventType.RowUpdated;
						case var t when t.Equals(pxContext.Events.RowDeleting): return EventType.RowDeleting;
						case var t when t.Equals(pxContext.Events.RowDeleted): return EventType.RowDeleted;
						case var t when t.Equals(pxContext.Events.RowPersisting): return EventType.RowPersisting;
						case var t when t.Equals(pxContext.Events.RowPersisted): return EventType.RowPersisted;
						case var t when t.Equals(pxContext.Events.FieldSelecting): return EventType.FieldSelecting;
						case var t when t.Equals(pxContext.Events.FieldDefaulting): return EventType.FieldDefaulting;
						case var t when t.Equals(pxContext.Events.FieldVerifying): return EventType.FieldVerifying;
						case var t when t.Equals(pxContext.Events.FieldUpdating): return EventType.FieldUpdating;
						case var t when t.Equals(pxContext.Events.FieldUpdated): return EventType.FieldUpdated;
						case var t when t.Equals(pxContext.Events.CommandPreparing): return EventType.CommandPreparing;
						case var t when t.Equals(pxContext.Events.ExceptionHandling): return EventType.ExceptionHandling;
					}
				}
			}

			return EventType.None;
		}
	}
}
