#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal partial class XmlCommentsWalker : CSharpSyntaxWalker
	{
		private class ContainingTypeInfo
		{
			public INamedTypeSymbol? ContainingType { get; }

			public DacType? DacKind { get; }

			public bool IsProjectionDac { get; }

			public bool IsDacOrDacExtension => DacKind.HasValue;

			public ContainingTypeInfo(INamedTypeSymbol? containingType, PXContext pxContext)
			{
				ContainingType = containingType;	
				DacKind = containingType?.GetDacType(pxContext);

				if (containingType != null && DacKind == DacType.Dac)
				{
					IsProjectionDac = containingType.IsProjectionDac(pxContext, checkTypeIsDac: false);
				}
				else
					IsProjectionDac = false;
			}

			private ContainingTypeInfo(INamedTypeSymbol? containingType)
			{
				ContainingType = containingType;
				DacKind = null;
				IsProjectionDac = false;
			}

			public static ContainingTypeInfo MakeInfoForNonDacType(INamedTypeSymbol? nonDacAndNonDacExtType) =>
				new(nonDacAndNonDacExtType);
		}
	}
}
