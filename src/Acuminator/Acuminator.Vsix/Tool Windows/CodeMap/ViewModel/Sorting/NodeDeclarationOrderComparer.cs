#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A code map tree node declaration order comparer.
	/// </summary>
	internal class NodeDeclarationOrderComparer : IComparer<TreeNodeViewModel>
	{
		public static readonly NodeDeclarationOrderComparer Instance = new();

		private  NodeDeclarationOrderComparer() { }

		public int Compare(TreeNodeViewModel x, TreeNodeViewModel y)
		{
			SymbolItem? symbolX = (x as INodeWithSymbolItem)?.Symbol;
			SymbolItem? symbolY = (y as INodeWithSymbolItem)?.Symbol;

			if (symbolX == null && symbolY == null)		//Always place nodes without symbol first
				return 0;
			else if (symbolX == null)
				return 1;
			else if (symbolY == null)
				return -1;

			return symbolX.DeclarationOrder - symbolY.DeclarationOrder;
		}
	}
}