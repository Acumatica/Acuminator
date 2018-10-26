using System.Diagnostics;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the cache attached event in graph.
	/// </summary>
	public class GraphEventInfo : GraphNodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol>
	{
		public GraphEventInfo Base { get; }

		public EventHandlerSignatureType SignatureType { get; }

		public EventType EventType { get; }


		public GraphEventInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder, 
							  EventHandlerSignatureType signatureType, EventType eventType) :
						 base(node, symbol, declarationOrder)
		{
			SignatureType = signatureType;
			EventType = EventType;
		}

		public GraphEventInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
							  EventHandlerSignatureType signatureType, EventType eventType, GraphEventInfo baseInfo)
			: this(node, symbol, declarationOrder, signatureType, eventType)
		{
			baseInfo.ThrowOnNull(nameof(baseInfo));

			Base = baseInfo;
		}
	}
}
