#nullable enable
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	/// <summary>
	/// Generic class for overridable semantic infos from a graph or DAC with node and symbol.
	/// </summary>
	/// <typeparam name="TInfo">Type of the information symbol.</typeparam>
	/// <typeparam name="N">Type of the declaration node of the item.</typeparam>
	/// <typeparam name="S">Type of the declaration symbol of the item.</typeparam>
	public abstract class OverridableNodeSymbolItem<TInfo, N, S> : NodeSymbolItem<N, S>, IWriteableBaseItem<TInfo>
	where TInfo : OverridableNodeSymbolItem<TInfo, N, S>
	where N : SyntaxNode
	where S : ISymbol
	{
		protected TInfo? _baseInfo;

		/// <summary>
		/// The overriden bas info if any.
		/// </summary>
		public TInfo? Base => _baseInfo;

		/// <summary>
		/// The overriden bas info if any.
		/// </summary>
		TInfo? IWriteableBaseItem<TInfo>.Base
		{
			get => Base;
			set {
				_baseInfo = value;

				if (value != null)
					CombineWithBaseInfo(value);
			}
		}

		protected OverridableNodeSymbolItem(N? node, S symbol, int declarationOrder, TInfo baseInfo) : this(node, symbol, declarationOrder)
		{
			_baseInfo = baseInfo.CheckIfNull();
			CombineWithBaseInfo(_baseInfo);
		}

		protected OverridableNodeSymbolItem(N? node, S symbol, int declarationOrder) : base(node, symbol, declarationOrder)
		{
		}

		void IWriteableBaseItem<TInfo>.CombineWithBaseInfo(TInfo baseInfo) => 
			CombineWithBaseInfo(baseInfo);

		/// <summary>
		/// Combine this info with info from base types.
		/// </summary>
		protected virtual void CombineWithBaseInfo(TInfo baseInfo)
		{

		}
	}
}
