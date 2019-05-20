using System.Diagnostics;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// A common generic graph event info DTO base class.
	/// </summary>
	public abstract class GraphEventInfoBase<TEventInfoType> : GraphEventInfoBase
	where TEventInfoType : GraphEventInfoBase<TEventInfoType>
	{
		public TEventInfoType BaseEvent { get; }

		protected GraphEventInfoBase(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
									 EventHandlerSignatureType signatureType, EventType eventType) :
								base(node, symbol, declarationOrder, signatureType, eventType)
		{		
		}

		protected GraphEventInfoBase(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
									 EventHandlerSignatureType signatureType, EventType eventType, TEventInfoType baseEventInfo)
							  : base(node, symbol, declarationOrder, signatureType, eventType)
		{
			baseEventInfo.ThrowOnNull(nameof(baseEventInfo));

			BaseEvent = baseEventInfo;
		}	
	}
}
