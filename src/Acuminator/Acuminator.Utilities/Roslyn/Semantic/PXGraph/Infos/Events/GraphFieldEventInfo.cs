using System;
using System.Diagnostics;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the graph events related to the DAC fields.
	/// </summary>
	public class GraphFieldEventInfo : GraphEventInfoBase<GraphFieldEventInfo>
	{
		public GraphFieldEventInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
								   EventHandlerSignatureType signatureType, EventType eventType) :
							  base(node, symbol, declarationOrder, signatureType, eventType)
		{
			ValidateEventType(eventType);
		}

		public GraphFieldEventInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
								   EventHandlerSignatureType signatureType, EventType eventType, GraphFieldEventInfo baseInfo)
							: base(node, symbol, declarationOrder, signatureType, eventType, baseInfo)
		{
			ValidateEventType(eventType);
		}

		private void ValidateEventType(EventType eventType)
		{
			if (!IsDacFieldEvent())
				throw new ArgumentOutOfRangeException(nameof(eventType), $"The {eventType.ToString()} is not a field event type.");
		}


	}
}
