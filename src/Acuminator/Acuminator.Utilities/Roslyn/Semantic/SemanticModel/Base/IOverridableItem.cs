#nullable enable

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	/// <summary>
	/// An interface for a DTO which stores info about some item. The item which can be overridable, and the info about base item is also stored.
	/// </summary>
	/// <typeparam name="T">Generic type parameter.</typeparam>
	public interface IOverridableItem<out T>
	where T : IOverridableItem<T>
	{
		string Name { get; }

		/// <summary>
		/// The overriden base info if any.
		/// </summary>
		T? Base { get; }

		int DeclarationOrder { get; }
	}

	internal interface IWriteableBaseItem<T> : IOverridableItem<T>
	where T : IOverridableItem<T>
	{
		/// <inheritdoc cref="IOverridableItem{T}.Base"/>
		new T? Base
		{
			get;
			set;
		}

		/// <summary>
		/// Combine this info with info from base types.
		/// </summary>
		void CombineWithBaseInfo(T baseInfo);
	}
}
