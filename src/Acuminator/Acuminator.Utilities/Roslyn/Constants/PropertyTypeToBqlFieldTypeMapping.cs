using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Constants
{
	/// <summary>
	/// A property type to BQL field type mapping table. 
	/// Contains mapping between DAC field property type and BQL field type.
	/// </summary>
	public static class PropertyTypeToBqlFieldTypeMapping
	{
		private static readonly Dictionary<string, string> _propertyTypeToBqlFieldType = new(StringComparer.OrdinalIgnoreCase)
		{
			{ "String", "BqlString" },
			{ "Guid", "BqlGuid" },
			{ "DateTime", "BqlDateTime" },

			{ "Bool", "BqlBool" },
			{ "Boolean", "BqlBool" },

			{ "Byte", "BqlByte" },

			{ "Short", "BqlShort" },
			{ "Int16", "BqlShort" },

			{ "Int", "BqlInt" },
			{ "Int32", "BqlInt" },

			{ "Long", "BqlLong" },
			{ "Int64", "BqlLong" },

			{ "Float", "BqlFloat" },
			{ "Single", "BqlFloat" },

			{ "Double", "BqlDouble" },
			{ "Decimal", "BqlDecimal" },

			{ "Byte[]", "BqlByteArray" },
			{ "ByteArray", "BqlByteArray" }
		};

		private static readonly Dictionary<string, string> _bqlFieldTypeToPropertyType = new(StringComparer.OrdinalIgnoreCase)
		{
			{ "BqlString"	 , "String" },
			{ "BqlGuid"		 , "Guid" },
			{ "BqlDateTime"	 , "DateTime" },
			{ "BqlBool"		 , "Boolean" },
			{ "BqlByte" 	 , "Byte" },
			{ "BqlShort"	 , "Int16" },
			{ "BqlInt"		 , "Int32" },
			{ "BqlLong"		 , "Int64" },
			{ "BqlFloat"	 , "Single" },
			{ "BqlDouble" 	 , "Double" },
			{ "BqlDecimal" 	 , "Decimal" },
			{ "BqlByteArray" , "Byte[]" },
		};

		public static bool ContainsPropertyType(PropertyTypeName propertyType) =>
			_propertyTypeToBqlFieldType.ContainsKey(propertyType);

		public static bool ContainsBqlFieldType(BqlTypeName bqlFieldType) =>
			_bqlFieldTypeToPropertyType.ContainsKey(bqlFieldType);

		public static string? GetBqlFieldType(PropertyTypeName propertyType) =>
			_propertyTypeToBqlFieldType.TryGetValue(propertyType, out var bqlFieldType)
				? bqlFieldType
				: null;

		public static string? GetPropertyTypeType(BqlTypeName bqlFieldType) =>
			_bqlFieldTypeToPropertyType.TryGetValue(bqlFieldType, out var propertyType)
				? propertyType
				: null;
	}
}