using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class IconViewModel : ExtraInfoViewModel
	{
		public Icon IconType { get; }

		public IconViewModel(Icon icon) : base()
		{
			IconType = icon;
		}
	}
}
