#nullable enable

using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacExtensionInfo : DacOrDacExtInfoBase<DacExtensionInfo>
	{
		public DacInfo? Dac { get; }

		protected DacExtensionInfo(ClassDeclarationSyntax? node, INamedTypeSymbol dacExtension, DacInfo? dac, int declarationOrder, 
								   DacExtensionInfo baseInfo) :
							  this(node, dacExtension, dac, declarationOrder)
		{
			_baseInfo = baseInfo.CheckIfNull();
			CombineWithBaseInfo(baseInfo);
		}

		protected DacExtensionInfo(ClassDeclarationSyntax? node, INamedTypeSymbol dacExtension, DacInfo? dac, int declarationOrder) :
							  base(node, dacExtension, declarationOrder)
		{
			Dac = dac;
		}

		public static DacExtensionInfo? Create(INamedTypeSymbol? dacExtension, ClassDeclarationSyntax? dacExtensionNode, ITypeSymbol? dac, 
											   PXContext pxContext, int dacExtDeclarationOrder, CancellationToken cancellation)
		{
			if (dacExtension == null)
				return null;

			cancellation.ThrowIfCancellationRequested();

			var dacNode = dac.GetSyntax(cancellation) as ClassDeclarationSyntax;
			var dacInfo = DacInfo.Create(dac as INamedTypeSymbol, dacNode, pxContext, dacDeclarationOrder: 0, cancellation);
			
			var extensionTypesFromFirstToLastLevel = dacExtension.GetBaseExtensions(pxContext, SortDirection.Ascending, includeDac: false);
			DacExtensionInfo? aggregatedBaseDacInfo = null, prevDacInfo = null;

			foreach (INamedTypeSymbol baseExtensionType in extensionTypesFromFirstToLastLevel)
			{
				cancellation.ThrowIfCancellationRequested();

				var baseDacExtNode = baseExtensionType.GetSyntax(cancellation) as ClassDeclarationSyntax;

				aggregatedBaseDacInfo = prevDacInfo != null
					? new DacExtensionInfo(baseDacExtNode, baseExtensionType, dacInfo, declarationOrder: 1, prevDacInfo)
					: new DacExtensionInfo(baseDacExtNode, baseExtensionType, dacInfo, declarationOrder: 1);

				prevDacInfo = aggregatedBaseDacInfo;
			}

			var dacExtensionInfo = aggregatedBaseDacInfo != null
				? new DacExtensionInfo(dacExtensionNode, dacExtension, dacInfo, dacExtDeclarationOrder, aggregatedBaseDacInfo)
				: new DacExtensionInfo(dacExtensionNode, dacExtension, dacInfo, dacExtDeclarationOrder);

			return dacExtensionInfo;
		}
	}
}