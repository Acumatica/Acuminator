using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// A simple information for code map about the px override method in graph. 
	/// Do not store extra info about a method being overriden or info about other overrides 
	/// </summary>
	public class PXOverrideInfo : SymbolItem<IMethodSymbol>
	{
		public PXOverrideInfo(IMethodSymbol symbol, int declarationOrder) : base(symbol, declarationOrder)
		{
		}

		internal static IEnumerable<PXOverrideInfo> GetPXOverrides(INamedTypeSymbol graphOrExtension, PXContext context, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();
			var pxOverrideAttribute = context.AttributeTypes.PXOverrideAttribute;

			if (pxOverrideAttribute == null)
				yield break;

			var declaredMethods = graphOrExtension.GetMethods();
			int declarationOrder = 0;

			foreach (IMethodSymbol method in declaredMethods)
			{
				cancellation.ThrowIfCancellationRequested();
				var attributes = method.GetAttributes();

				if (!attributes.IsEmpty && attributes.Any(a => a.AttributeClass.Equals(pxOverrideAttribute)))
				{
					yield return new PXOverrideInfo(method, declarationOrder);
					declarationOrder++;
				}
			}
		}
	}
}
