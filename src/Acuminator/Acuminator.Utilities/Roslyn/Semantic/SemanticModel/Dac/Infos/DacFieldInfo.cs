#nullable enable

using System;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	/// <summary>
	/// Information about a DAC field - a pair consisting of a DAC field property and a DAC BQL field.
	/// </summary>
	public class DacFieldInfo : IWriteableBaseItem<DacFieldInfo>
	{
		public string Name { get; }

		public ITypeSymbol? DacType { get; }

		public DacPropertyInfo? PropertyInfo { get; }

		public DacBqlFieldInfo? FieldInfo { get; }

		public DacFieldInfo? Base { get; set; }

		DacFieldInfo? IOverridableItem<DacFieldInfo>.Base => Base;

		public int DeclarationOrder => PropertyInfo?.DeclarationOrder ?? FieldInfo!.DeclarationOrder;

		public DacFieldInfo(DacPropertyInfo? dacPropertyInfo, DacBqlFieldInfo? dacFieldInfo)
		{
			if (dacPropertyInfo == null && dacFieldInfo == null)
				throw new ArgumentNullException($"Both {nameof(dacPropertyInfo)} and {nameof(dacFieldInfo)} parameters cannot be null.");

			PropertyInfo = dacPropertyInfo;
			FieldInfo 	 = dacFieldInfo;
			Name 		 = PropertyInfo?.Name ?? FieldInfo!.Name.ToPascalCase();
			DacType 	 = PropertyInfo?.Symbol.ContainingType ?? FieldInfo!.Symbol.ContainingType;
		}

		public bool IsDeclaredInType(ITypeSymbol? type) =>
			 PropertyInfo?.Symbol.IsDeclaredInType(type) ?? FieldInfo!.Symbol.IsDeclaredInType(type);
	}
}
