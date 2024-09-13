#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap.Filter;

public class FilterViewModel : ViewModelBase
{
	public event EventHandler<FilterEventArgs>? FilterChanged;

	private string? _filterText;

	public string? FilterText
	{
		get => _filterText;
		set
		{
			string? newValue = value.IsNullOrEmpty() 
				? null 
				: value;

			if (_filterText != newValue)
			{
				string? oldValue = _filterText;
				_filterText		 = newValue;

				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(HasFilterText));

				RaiseFilterChanged(new FilterEventArgs(_filterText, oldValue));
			}
		}
	}

	private bool _isFiltering;

	public bool IsFiltering
	{
		get => _isFiltering;
		set 
		{
			if (_isFiltering != value)
			{
				_isFiltering = value;
				NotifyPropertyChanged();
			}
		}
	}

	public bool HasFilterText => !_filterText.IsNullOrEmpty();

	public Command ClearCommand { get; }

	public FilterViewModel()
	{
		ClearCommand  = new Command(p => ClearSearch());
	}

	public void ClearSearch()
	{
		FilterText = null;
	}

	private void RaiseFilterChanged(FilterEventArgs filterEventArgs)
	{
		bool oldIsFiltering = IsFiltering;

		try
		{
			IsFiltering = true;
			FilterChanged?.Invoke(this, filterEventArgs);
		}
		finally
		{
			IsFiltering = oldIsFiltering;
		}
	}
}
