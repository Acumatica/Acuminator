﻿#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXSystemActions
{
	/// <summary>
	/// Information about the Acumatica system actions. The list of system actions is taken from the <see cref="PXGraph{TGraph, TPrimary}"/>.
	/// </summary>
	public class PXSystemActionsRegister
	{
		private readonly PXContext _context;

		public ImmutableHashSet<ITypeSymbol> SystemActions { get; }
		
		public PXSystemActionsRegister(PXContext pxContext)
		{
			_context	  = pxContext.CheckIfNull();
			SystemActions = GetSystemActions(_context).ToImmutableHashSet();
		}

		/// <summary>
		/// Query if <paramref name="pxAction"/> is system action. 
		/// The action is considered system if it is one of the actions in <see cref="SystemActions"/> set or is derived from one of them.
		/// </summary>
		/// <param name="pxAction">The action to check.</param>
		/// <returns/>
		public bool IsSystemAction(ITypeSymbol pxAction) =>
			pxAction.CheckIfNull()
					.GetBaseTypesAndThis()
					.Any(action => SystemActions.Contains(action) || SystemActions.Contains(action.OriginalDefinition));

		private static HashSet<ITypeSymbol> GetSystemActions(PXContext pxContext) =>
			[
				pxContext.PXSystemActions.PXSave,
				pxContext.PXSystemActions.PXCancel,
				pxContext.PXSystemActions.PXInsert,
				pxContext.PXSystemActions.PXDelete,
				pxContext.PXSystemActions.PXCopyPasteAction,
				pxContext.PXSystemActions.PXFirst,
				pxContext.PXSystemActions.PXPrevious,
				pxContext.PXSystemActions.PXNext,
				pxContext.PXSystemActions.PXLast,
				pxContext.PXSystemActions.PXChangeID
			];
	}
}
