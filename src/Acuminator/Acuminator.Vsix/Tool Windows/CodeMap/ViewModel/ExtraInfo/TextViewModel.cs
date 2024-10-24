﻿#nullable enable

using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class TextViewModel : ExtraInfoViewModel
	{
		public string Text { get; }

		public Color? DarkThemeForeground { get; }

		public Color? LightThemeForeground { get; }

		private string? _tooltip;

		public string? Tooltip
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
			Text = text.CheckIfNull();
			DarkThemeForeground = darkThemeForeground;
			LightThemeForeground = lightThemeForeground;
		}
	}
}
