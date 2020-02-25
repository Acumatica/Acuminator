using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using Acuminator.Utilities.Common;



namespace Acuminator.Vsix.Utilities
{
	/// <summary>
	/// Modified <see cref="ObservableCollection{T}"/> which allows to add multiple items at once with only one notification.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ExtendedObservableCollection<T> : ObservableCollection<T>
	{
		public ExtendedObservableCollection() : base()
		{
		}

		public ExtendedObservableCollection(IEnumerable<T> collection) : base(collection)
		{
		}

		public ExtendedObservableCollection(List<T> list) : base(list)
		{
		}

		/// <summary>
		/// Special constructor for collections with only one element (most frequent case for code map) - used to avoid the allocation of array.
		/// </summary>
		/// <param name="item">The item.</param>
		public ExtendedObservableCollection(T item) : base()
		{
			item.ThrowOnNull(nameof(item));
			Items.Add(item);
		}

		/// <summary>
		/// Adds the range of items.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <exception cref="System.ArgumentNullException"></exception>
		public void AddRange(IEnumerable<T> range)
		{
			range.ThrowOnNull(nameof(range));
			CheckReentrancy();

			foreach (T item in range)
			{
				Items.Add(item);
			}

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
			OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
			OnCollectionChanged(
				new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Inserts the range.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="range">The range.</param>
		/// <exception cref="System.ArgumentNullException"></exception>
		/// <exception cref="System.ArgumentOutOfRangeException"></exception>
		public void InsertRange(int index, IEnumerable<T> range)
		{
			range.ThrowOnNull(nameof(range));

			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			CheckReentrancy();

			foreach (var item in range)
			{
				Items.Insert(index, item);
				index++;
			}

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
			OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
			OnCollectionChanged(
				new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Removes the range of items from the collection.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <exception cref="System.ArgumentNullException"></exception>
		public void RemoveRange(IEnumerable<T> range)
		{
			range.ThrowOnNull(nameof(range));
			CheckReentrancy();

			foreach (T item in range)
			{
				Items.Remove(item);
			}

			OnPropertyChanged(new PropertyChangedEventArgs("Count"));
			OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
			OnCollectionChanged(
				new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
		

		/// <summary>
		/// Clears collection and then adds specified range.
		/// </summary>
		/// <param name="range">The range.</param>
		public void Reset(IEnumerable<T> range)
		{
			Items.Clear();

			AddRange(range);
		}
	}
}
