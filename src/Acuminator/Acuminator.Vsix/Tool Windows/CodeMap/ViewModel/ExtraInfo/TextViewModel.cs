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

		public Color? DarkThemeForeground { get; }

		public Color? LightThemeForeground { get; }

		private string _tooltip;

		public string Tooltip
		{
			get => _tooltip;
			set
			{
				if (_tooltip != value)
				{
					_tooltip = value;
					NotifyPropertyChanged();
				}
			}
		}

		public TextViewModel(TreeNodeViewModel node, string text, Color? darkThemeForeground = null, Color? lightThemeForeground = null) :
						base(node)
		{
			Text = text.CheckIfNull(nameof(text));
			DarkThemeForeground = darkThemeForeground;
			LightThemeForeground = lightThemeForeground;
		}
	}
}
