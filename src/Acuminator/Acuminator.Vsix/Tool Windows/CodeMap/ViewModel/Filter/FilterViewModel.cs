#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;

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
				FilterChanged?.Invoke(this, new FilterEventArgs(_filterText, oldValue));
			}
		}
	}

	public bool HasFilterText => !_filterText.IsNullOrEmpty();

	public Command FilterCommand { get; }

	public FilterViewModel()
	{
		FilterCommand = new Command(p => FilterCodeMap());
	}

	public void ClearSearch()
	{
		FilterText = null;
	}

	private void FilterCodeMap()
	{

	}
}
