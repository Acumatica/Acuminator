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

		public Color? Foreground { get; }

		public TextViewModel(string text, Color foreground) : this(text)
		{
			Foreground = foreground;
		}

		public TextViewModel(string text) : base()
		{
			Text = text.CheckIfNull(nameof(text));
		}
	}
}
