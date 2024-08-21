#nullable enable

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// A common generic graph event info DTO base class.
	/// </summary>
	public abstract class GraphEventInfoBase<TEventInfoType> : GraphEventInfoBase, IWriteableBaseItem<TEventInfoType>
	where TEventInfoType : GraphEventInfoBase<TEventInfoType>
	{
		protected TEventInfoType? _baseInfo;

		/// <summary>
		/// The base event. 
		/// </summary>
		/// <remarks>
		/// Internal setter is used for two reasons:
		/// 1) Perfomance - to avoid allocation of objects during retrieval of overrides hierarchy.  
		/// 2) Overcomplicated architecture - the use of completely readonly objects will require a more complex <see cref="GraphEventsCollection{TEventInfoType}"/> class
		/// which will know how to create a new <typeparamref name="TEventInfoType"/> event info. 
		/// This will lead to a two concrete implementations of collection for <see cref="GraphRowEventInfo"/> and <see cref="GraphFieldEventInfo"/> 
		/// or to a hard to read code in the <see cref="PXGraphEventSemanticModel.EventsCollector"/> if we choose to pass the delegates to the generic collection class. 
		/// </remarks>
		public TEventInfoType? Base => _baseInfo;

		TEventInfoType? IWriteableBaseItem<TEventInfoType>.Base
		{
			get => Base;
			set 
			{
				_baseInfo = value;

				if (value != null)
					CombineWithBaseInfo(value);
			}
		}

		protected GraphEventInfoBase(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder,
									 EventHandlerSignatureType signatureType, EventType eventType) :
								base(node, symbol, declarationOrder, signatureType, eventType)
		{		
		}

		protected GraphEventInfoBase(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder,
									 EventHandlerSignatureType signatureType, EventType eventType, TEventInfoType baseEventInfo)
							  : base(node, symbol, declarationOrder, signatureType, eventType)
		{
			_baseInfo = baseEventInfo.CheckIfNull();
			CombineWithBaseInfo(baseEventInfo);
		}

		void IWriteableBaseItem<TEventInfoType>.CombineWithBaseInfo(TEventInfoType baseInfo) => CombineWithBaseInfo(baseInfo);

		private void CombineWithBaseInfo(TEventInfoType baseInfo)
		{

		}
	}
}
