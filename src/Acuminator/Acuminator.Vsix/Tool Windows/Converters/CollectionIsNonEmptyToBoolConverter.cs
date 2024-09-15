#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

using Acuminator.Vsix.ToolWindows.CodeMap;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.Converters;

/// <summary>
/// A one way converter which converts collections to <see cref="bool"/> value.
/// Returns <see langword="true"/> if the collection is not null or empty. Otherwise, returns <see langword="false"/>.
/// </summary>
[ValueConversion(typeof(IEnumerable), typeof(bool))]
public class CollectionIsNonEmptyToBoolConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
		value switch
		{
			ExtendedObservableCollection<TreeNodeViewModel> nodesCollection => nodesCollection.Count > 0,
			ReadOnlyObservableCollection<TreeNodeViewModel> nodesCollection => nodesCollection.Count > 0,
			IReadOnlyCollection<object> genCollection 						=> genCollection.Count > 0,  
			ICollection nonGenCollection 			  						=> nonGenCollection.Count > 0,
			IEnumerable enumerable 					  						=> Any(enumerable),
			null 									  						=> false,
			_ 										  						=> false
		};

	private bool Any(IEnumerable enumerable)
	{
		IEnumerator? enumerator = null;

		try
		{
			enumerator = enumerable.GetEnumerator();
			return enumerator?.MoveNext() ?? false;
		}
		finally
		{
			if (enumerator is IDisposable disposable)
				disposable.Dispose();
		}
	}

	public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
