#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap.Dac
{
	public class DacSemanticModelForCodeMap : ISemanticModel
	{
		public DacSemanticModel DacModel { get; }

		public DacInfo? DacInfo { get; }

		public DacExtensionInfo? DacExtensionInfo { get; }
		
		public INamedTypeSymbol Symbol => DacModel.Symbol;

		public PXContext PXContext => DacModel.PXContext;

		public string Name => DacModel.Name;

		public DacType DacType => DacModel.DacType;

		public bool IsProjectionDac => DacModel.IsProjectionDac;

		public ImmutableArray<DacAttributeInfo> Attributes => DacModel.Attributes;

		public DacSemanticModelForCodeMap(DacSemanticModel dacSemanticModel)
		{
			DacModel = dacSemanticModel.CheckIfNull();

			if (DacType == DacType.Dac)
			{
				DacInfo = DacModel.DacOrDacExtInfo as DacInfo;
				DacExtensionInfo = null;
			}
			else
			{
				DacExtensionInfo = DacModel.DacOrDacExtInfo as DacExtensionInfo;
				DacInfo = DacExtensionInfo?.Dac;
			}
		}
	}
}
