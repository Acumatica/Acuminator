using System.Diagnostics;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the event in graph. Used for all events except events related to the DAC fields.
	/// </summary>
	public class GraphEventInfo : GraphEventInfoBase<GraphEventInfo>
	{
		
		public GraphEventInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
							  EventHandlerSignatureType signatureType, EventType eventType) :
						 base(node, symbol, declarationOrder, signatureType, eventType)
		{			
		}

		public GraphEventInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
							  EventHandlerSignatureType signatureType, EventType eventType, GraphEventInfo baseInfo)
					   : base(node, symbol, declarationOrder, signatureType, eventType, baseInfo)
		{		
		}
	}
}
