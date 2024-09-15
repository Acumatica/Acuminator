#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Vsix.ToolWindows.CodeMap.Dac
{
	public class DacSemanticModelForCodeMap : ISemanticModel
	{
		public DacSemanticModel DacModel { get; }

		public DacInfo? BaseDacInfo { get; }

		public DacExtensionInfo? BaseDacExtensionInfo { get; }

		public INamedTypeSymbol Symbol => DacModel.Symbol;

		public PXContext PXContext => DacModel.PXContext;

		private DacSemanticModelForCodeMap(DacSemanticModel dacSemanticModel, DacInfo? baseDacInfo, DacExtensionInfo? baseExtensionInfo)
		{
			DacModel			 = dacSemanticModel;
			BaseDacInfo 		 = baseDacInfo;
			BaseDacExtensionInfo = baseExtensionInfo;
		}

		public static DacSemanticModelForCodeMap Create(DacSemanticModel dacSemanticModel, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var (baseDacInfo, baseDacExtension) = GetDacAndDacExtensionBaseTypeInfos(dacSemanticModel.CheckIfNull(), cancellation);
			return new DacSemanticModelForCodeMap(dacSemanticModel, baseDacInfo, baseDacExtension);
		}

		private static (DacInfo? BaseDac, DacExtensionInfo? BaseDacExtension) GetDacAndDacExtensionBaseTypeInfos(DacSemanticModel dacSemanticModel, 
																												 CancellationToken cancellation)
		{
			if (dacSemanticModel.DacType == DacType.Dac)
			{
				var baseDacInfo = CreateDacInfoFromDacType(dacSemanticModel.Symbol.BaseType, dacSemanticModel.PXContext, cancellation);
				return baseDacInfo != null 
					? (BaseDac: baseDacInfo, BaseDacExtension: null)
					: (BaseDac: null,		 BaseDacExtension: null);
			}
			else
				return GetBaseDacAndDacExtensionInfos(dacSemanticModel, cancellation);
		}

		private static (DacInfo? BaseDacInfo, DacExtensionInfo? BaseDacExtension) GetBaseDacAndDacExtensionInfos(DacSemanticModel dacSemanticModel, 
																												 CancellationToken cancellation)
		{
			DacInfo? baseDacInfo = dacSemanticModel.DacSymbol != null
				? CreateDacInfoFromDacType(dacSemanticModel.DacSymbol as INamedTypeSymbol, dacSemanticModel.PXContext, cancellation)
				: null;

			var extensionBaseType = dacSemanticModel.Symbol.BaseType;

			if (extensionBaseType == null || extensionBaseType.SpecialType == SpecialType.System_Object ||
				extensionBaseType.IsDacExtensionBaseType())
			{
				return (baseDacInfo, BaseDacExtension: null);
			}

			int declarationOrder  = baseDacInfo != null ? 1 : 0;
			var baseExtensionNode = extensionBaseType.GetSyntax(cancellation) as ClassDeclarationSyntax;
			var baseExtensionInfo = new DacExtensionInfo(baseExtensionNode, extensionBaseType, declarationOrder);

			return (baseDacInfo, baseExtensionInfo);
		}

		private static DacInfo? CreateDacInfoFromDacType(INamedTypeSymbol? dacType, PXContext pxContext, CancellationToken cancellation)
		{
			if (dacType == null || dacType.SpecialType == SpecialType.System_Object)
				return null;

			var pxBqlTable = pxContext.PXBqlTable;

			if (pxBqlTable != null && SymbolEqualityComparer.Default.Equals(dacType, pxBqlTable))
				return null;

			var dacNode = dacType.GetSyntax(cancellation) as ClassDeclarationSyntax;

			return new DacInfo(dacNode, dacType, declarationOrder: 0);
		}
	}
}
