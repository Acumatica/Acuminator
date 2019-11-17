using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class TextViewModel : ExtraInfoViewModel
	{
		public string Text { get; }

		public Brush Foreground { get; }

		public TextViewModel(string text, Brush foreground = null) : 
						base()
		{
			Text = text.CheckIfNull(nameof(text));
			Foreground = foreground;
		}
	}
}
