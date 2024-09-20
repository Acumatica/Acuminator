#nullable enable

using System;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Acuminator.Utilities.Roslyn.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacInfo : DacOrDacExtInfoBase<DacInfo>
	{
		protected DacInfo(ClassDeclarationSyntax? node, INamedTypeSymbol dac, int declarationOrder, DacInfo baseInfo) :
					 base(node, dac, declarationOrder, baseInfo)
		{
		}

		protected DacInfo(ClassDeclarationSyntax? node, INamedTypeSymbol dac, int declarationOrder) :
					 base(node, dac, declarationOrder)
		{
		}

		public static DacInfo? Create(INamedTypeSymbol? dac, ClassDeclarationSyntax? dacNode, PXContext pxContext, 
									  int dacDeclarationOrder, CancellationToken cancellation)
		{
			if (dac == null)
				return null;

			cancellation.ThrowIfCancellationRequested();

			var dacBaseTypesFromBaseToDerived = dac.GetDacBaseTypesThatMayStoreDacProperties(pxContext)
												   .OfType<INamedTypeSymbol>()
												   .Reverse();
			bool isInSource = dacNode != null;
			DacInfo? aggregatedBaseDacInfo = null, prevDacInfo = null;

			foreach (INamedTypeSymbol baseType in dacBaseTypesFromBaseToDerived)
			{
				cancellation.ThrowIfCancellationRequested();

				var baseDacNode = isInSource
					? baseType.GetSyntax(cancellation) as ClassDeclarationSyntax
					: null;

				isInSource = baseDacNode != null;
				aggregatedBaseDacInfo = prevDacInfo != null 
					? new DacInfo(baseDacNode, baseType, declarationOrder: 0, prevDacInfo)
					: new DacInfo(baseDacNode, baseType, declarationOrder: 0);

				prevDacInfo = aggregatedBaseDacInfo;
			}

			var dacInfo = aggregatedBaseDacInfo != null
				? new DacInfo(dacNode, dac, dacDeclarationOrder, aggregatedBaseDacInfo)
				: new DacInfo(dacNode, dac, dacDeclarationOrder);

			return dacInfo;
		}
	}
}