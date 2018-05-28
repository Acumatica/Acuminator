using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using PX.Data;
using Acuminator.Analyzers;

namespace Acuminator.Utilities
{
	/// <summary>
	/// Information about the Acumatica field attributes.
	/// </summary>
	public readonly struct FieldAttributeDTO
	{
		public bool IsFieldAttribute { get; }

		public bool IsBoundField { get; }

		public ITypeSymbol FieldType { get; }

		public FieldAttributeDTO(bool isFieldAttribute, bool isBoundField, ITypeSymbol fieldType)
		{
			IsFieldAttribute = isFieldAttribute;
			IsBoundField = isBoundField;
			FieldType = fieldType;
		}		
	}
}
