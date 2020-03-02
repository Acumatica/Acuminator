using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class ExtraInfoViewModel : ViewModelBase
	{
		public TreeNodeViewModel Node { get; }

		protected ExtraInfoViewModel(TreeNodeViewModel node)
		{
			Node = node.CheckIfNull(nameof(node));
		}
	}
}
