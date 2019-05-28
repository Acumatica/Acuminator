using System.Diagnostics;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the row event in graph.
	/// </summary>
	public class GraphRowEventInfo : GraphEventInfoBase<GraphRowEventInfo>
	{
		
		public GraphRowEventInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
							  EventHandlerSignatureType signatureType, EventType eventType) :
						 base(node, symbol, declarationOrder, signatureType, eventType)
		{			
		}

		public GraphRowEventInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
							  EventHandlerSignatureType signatureType, EventType eventType, GraphRowEventInfo baseInfo)
					   : base(node, symbol, declarationOrder, signatureType, eventType, baseInfo)
		{		
		}

		internal override string GetEventGroupingKey() => $"{DacName}_{EventType.ToString()}";
	}
}
