using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Values that represent icons for tree nodes in code map tree.
	/// </summary>
	public enum Icons
	{
		GraphIcon,
		EventIcon,
		DacIcon,
		DacFieldIcon,

		GroupNodeIcon,
		GroupingDacIcon,
		GroupingDacFieldIcon,

		ViewIcon,
		ViewDelegateIcon,

		ActionIcon,
		ActionHandlerIcon,

		PXOverrideIcon,

		RowEventIcon,
		FieldEventIcon,
		CacheAttachedIcon	
	}
}
