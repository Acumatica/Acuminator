using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap.TreeListViewControl
{
	public sealed partial class TreeNode : INotifyPropertyChanged
	{
		private class NodeCollection : Collection<TreeNode>
		{
			private TreeNode _owner;

			public NodeCollection(TreeNode owner)
			{
				_owner = owner;
			}

			protected override void ClearItems()
			{
				while (Count != 0)
				{
					RemoveAt(Count - 1);
				}
			}

			protected override void InsertItem(int index, TreeNode item)
			{
				item.ThrowOnNull(nameof(item));

				if (item.Parent != _owner)
				{
					if (item.Parent != null)
					{
						item.Parent.Children.Remove(item);
					}

					item.Parent = _owner;
					item.Index = index;

					for (int i = index; i < Count; i++)
					{
						this[i].Index++;
					}

					base.InsertItem(index, item);
				}
			}

			protected override void RemoveItem(int index)
			{
				TreeNode item = this[index];
				item.Parent = null;
				item.Index = -1;

				for (int i = index + 1; i < Count; i++)
				{
					this[i].Index--;
				}

				base.RemoveItem(index);
			}

			protected override void SetItem(int index, TreeNode item)
			{
				item.ThrowOnNull(nameof(item));
				RemoveAt(index);
				InsertItem(index, item);
			}
		}
	}
}
