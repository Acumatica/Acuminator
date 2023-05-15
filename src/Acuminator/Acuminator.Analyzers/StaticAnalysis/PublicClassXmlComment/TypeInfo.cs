#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal class TypeInfo
	{
		public static TypeInfo NonDacTypeInfo { get; } = new();

		public INamedTypeSymbol? ContainingType { get; }

		public DacType? DacKind { get; }

		public bool IsProjectionDacOrExtensionToProjectionDac { get; }

		public bool IsDacOrDacExtension => DacKind.HasValue;

		public TypeInfo(INamedTypeSymbol? containingType, PXContext pxContext)
		{
			ContainingType 							  = containingType;
			DacKind 								  = containingType?.GetDacType(pxContext);
			IsProjectionDacOrExtensionToProjectionDac = IsProjectionDacOrDacExtensionToProjectionDac(pxContext);
		}

		private TypeInfo()
		{
			ContainingType 							  = null;
			DacKind 								  = null;
			IsProjectionDacOrExtensionToProjectionDac = false;
		}

		private bool IsProjectionDacOrDacExtensionToProjectionDac(PXContext pxContext)
		{
			if (ContainingType == null || DacKind == null)
				return false;
			else if (DacKind == DacType.Dac)
				return ContainingType.IsProjectionDac(pxContext, checkTypeIsDac: false);
			
			var dacTypeOfDacExtension = ContainingType.GetDacFromDacExtension(pxContext);
			return dacTypeOfDacExtension?.IsProjectionDac(pxContext, checkTypeIsDac: false) ?? false;
		}
	}
}
