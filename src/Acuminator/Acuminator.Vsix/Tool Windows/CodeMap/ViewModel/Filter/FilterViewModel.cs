#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

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

				RaiseFilterChanged(_filterText, oldValue);
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

	[MemberNotNullWhen(returnValue: true, nameof(FilterText))]
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

	public FilterOptions CreateFilterOptionsFromCurrentFilter() =>
		HasFilterText
			? new FilterOptions(FilterText)
			: FilterOptions.NoFilter;

	private void RaiseFilterChanged(string? newFilterText, string? oldFilterText)
	{
		bool oldIsFiltering = IsFiltering;

		try
		{
			IsFiltering = true;

			var filterOptions = newFilterText.IsNullOrEmpty()
				? FilterOptions.NoFilter
				: new FilterOptions(newFilterText);
			var filterEventArgs = new FilterEventArgs(filterOptions, oldFilterText);

			FilterChanged?.Invoke(this, filterEventArgs);
		}
		finally
		{
			IsFiltering = oldIsFiltering;
		}
	}
}
