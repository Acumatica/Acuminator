#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Vsix.Coloriser;

using Microsoft.VisualStudio.PlatformUI;

using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace Acuminator.Vsix.ToolWindows
{
	[SuppressMessage("Style",
		"VSTHRD010: Accessing \"Acuminator.Vsix.Coloriser.VSColors.IsDarkTheme\" should only be done on the main thread.Call Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread() first.",
		Justification = "Already called inside", Scope = "type")]
	public class ToolWindowViewModelBase : ViewModelBase, IDisposable
	{
		private bool _isDarkTheme;

		public bool IsDarkTheme
		{
			get => _isDarkTheme;
			private set
			{
				if (_isDarkTheme == value)
					return;

				_isDarkTheme = value;
				NotifyPropertyChanged();
			}
		}

		public ToolWindowViewModelBase()
		{
			_isDarkTheme = VSColors.IsDarkTheme();
			VSColorTheme.ThemeChanged += OnVsColorThemeChanged;
		}

		public virtual void FreeResources() { }

		public virtual void UnSubscribeVSEvents()
		{
			VSColorTheme.ThemeChanged -= OnVsColorThemeChanged;
		}

		public void Dispose()
		{
			UnSubscribeVSEvents();
			FreeResources();
		}

		protected virtual void OnVsColorThemeChanged(ThemeChangedEventArgs e)
		{
			IsDarkTheme = VSColors.IsDarkTheme();
		}
	}
}
