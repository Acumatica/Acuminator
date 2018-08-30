using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using PX.Data;
using Acuminator.Analyzers;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities
{
	/// <summary>
	/// Information about the Acumatica system actions. The list of system actions is taken from the <see cref="PXGraph{TGraph, TPrimary}"/>.
	/// </summary>
	public class PXSystemActionsRegister
	{
		private readonly PXContext context;

		public ImmutableHashSet<ITypeSymbol> SystemActions { get; }
		
		public PXSystemActionsRegister(PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			context = pxContext;
			SystemActions = GetSystemActions(context).ToImmutableHashSet();
		}

		/// <summary>
		/// Query if <paramref name="pxAction"/> is system action. 
		/// The action is considered system if it is one of the actions in <see cref="SystemActions"/> set or is derived from one of them.
		/// </summary>
		/// <param name="pxAction">The action to check.</param>
		/// <returns/>
		public bool IsSystemAction(ITypeSymbol pxAction)
		{
			pxAction.ThrowOnNull(nameof(pxAction));

			return pxAction.GetBaseTypesAndThis()
						   .Any(action => SystemActions.Contains(action) || SystemActions.Contains(action.OriginalDefinition));
		}

		private static HashSet<ITypeSymbol> GetSystemActions(PXContext pxContext) =>
			new HashSet<ITypeSymbol>
			{
				pxContext.PXSystemActions.PXSave,
				pxContext.PXSystemActions.PXCancel,
				pxContext.PXSystemActions.PXInsert,
				pxContext.PXSystemActions.PXDelete,
				pxContext.PXSystemActions.PXCopyPasteAction,
				pxContext.PXSystemActions.PXFirst,
				pxContext.PXSystemActions.PXPrevious,
				pxContext.PXSystemActions.PXNext,
				pxContext.PXSystemActions.PXLast
			};	
	}
}
