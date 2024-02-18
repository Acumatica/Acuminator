#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

		public ImmutableDictionary<string, IPropertySymbol> PropertiesFromBaseDacs { get; }

		public TypeInfo(INamedTypeSymbol? containingType, PXContext pxContext)
		{
			ContainingType 							  = containingType;
			DacKind 								  = containingType?.GetDacType(pxContext);
			IsProjectionDacOrExtensionToProjectionDac = IsProjectionDacOrDacExtensionToProjectionDac(pxContext);
			PropertiesFromBaseDacs 					  = GetPropertiesFromBaseDacs(pxContext);
		}

		private TypeInfo()
		{
			ContainingType 							  = null;
			DacKind 								  = null;
			IsProjectionDacOrExtensionToProjectionDac = false;
			PropertiesFromBaseDacs 					  = ImmutableDictionary<string, IPropertySymbol>.Empty;
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

		private ImmutableDictionary<string, IPropertySymbol> GetPropertiesFromBaseDacs(PXContext pxContext)
		{
			if (DacKind != DacType.Dac || !IsProjectionDacOrExtensionToProjectionDac || ContainingType?.BaseType == null ||
				ContainingType.BaseType.SpecialType == SpecialType.System_Object)
			{
				return ImmutableDictionary<string, IPropertySymbol>.Empty;
			}

			if (pxContext.IsAcumatica2024R1_OrGreater && ContainingType.BaseType.Equals(pxContext.PXBqlTable))
				return ImmutableDictionary<string, IPropertySymbol>.Empty;

			var builder		 = ImmutableDictionary.CreateBuilder<string, IPropertySymbol>(StringComparer.OrdinalIgnoreCase);
			var dacBaseTypes = ContainingType.GetDacBaseTypesThatMayStoreDacProperties(pxContext)
											 .OfType<INamedTypeSymbol>();

			foreach (INamedTypeSymbol dacType in dacBaseTypes)
			{
				var properties = dacType.GetProperties();

				foreach (IPropertySymbol property in properties)
				{
					if (!builder.ContainsKey(property.Name))
						builder.Add(property.Name, property);
				}
			}

			return builder.ToImmutable();
		}
	}
}
