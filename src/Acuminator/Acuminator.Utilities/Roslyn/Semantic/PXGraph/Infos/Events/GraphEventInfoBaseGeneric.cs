using System.Diagnostics;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// A common generic graph event info DTO base class.
	/// </summary>
	public abstract class GraphEventInfoBase<TDerived> : GraphEventInfoBase
	where TDerived : GraphEventInfoBase<TDerived>
	{
		public TDerived Base { get; }

		protected GraphEventInfoBase(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
									 EventHandlerSignatureType signatureType, EventType eventType) :
								base(node, symbol, declarationOrder, signatureType, eventType)
		{		
		}

		protected GraphEventInfoBase(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
									 EventHandlerSignatureType signatureType, EventType eventType, TDerived baseInfo)
							  : base(node, symbol, declarationOrder, signatureType, eventType)
		{
			baseInfo.ThrowOnNull(nameof(baseInfo));

			Base = baseInfo;
		}	
	}
}
