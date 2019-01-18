using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.PlatformUI;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Coloriser;



namespace Acuminator.Vsix.ToolWindows
{
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
			VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
		}

		public virtual void FreeResources() { }

		public virtual void UnSubscribeVSEvents()
		{
			VSColorTheme.ThemeChanged -= VSColorTheme_ThemeChanged;
		}

		public void Dispose()
		{
			UnSubscribeVSEvents();
			FreeResources();
		}

		private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
		{
			IsDarkTheme = VSColors.IsDarkTheme();
		}
	}
}
