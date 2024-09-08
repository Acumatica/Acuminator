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
			{ nameof(String)  , "BqlString"   },
			{ nameof(Guid)	  , "BqlGuid"	  },
			{ nameof(DateTime), "BqlDateTime" },

			{ "Bool"		 , "BqlBool" },
			{ nameof(Boolean), "BqlBool" },

			{ nameof(Byte), "BqlByte" },

			{ "Short"	   , "BqlShort" },
			{ nameof(Int16), "BqlShort" },

			{ "Int"		   , "BqlInt" },
			{ nameof(Int32), "BqlInt" },

			{ "Long"	   , "BqlLong" },
			{ nameof(Int64), "BqlLong" },

			{ "Float"		, "BqlFloat" },
			{ nameof(Single), "BqlFloat" },

			{ nameof(Double) , "BqlDouble" },
			{ nameof(Decimal), "BqlDecimal" },

			{ $"{nameof(Byte)}[]", "BqlByteArray" },
			{ "ByteArray"		 , "BqlByteArray" }
		};

		private static readonly Dictionary<string, string> _bqlFieldTypeToPropertyType = new(StringComparer.OrdinalIgnoreCase)
		{
			{ "BqlString"	 , nameof(String) 	   },
			{ "BqlGuid"		 , nameof(Guid) 	   },
			{ "BqlDateTime"	 , nameof(DateTime)	   },
			{ "BqlBool"		 , nameof(Boolean) 	   },
			{ "BqlByte" 	 , nameof(Byte) 	   },
			{ "BqlShort"	 , nameof(Int16)	   },
			{ "BqlInt"		 , nameof(Int32) 	   },
			{ "BqlLong"		 , nameof(Int64) 	   },
			{ "BqlFloat"	 , nameof(Single) 	   },
			{ "BqlDouble" 	 , nameof(Double)	   },
			{ "BqlDecimal" 	 , nameof(Decimal) 	   },
			{ "BqlByteArray" , $"{nameof(Byte)}[]" },
		};

		public static bool ContainsPropertyType(PropertyTypeName propertyType) =>
			_propertyTypeToBqlFieldType.ContainsKey(propertyType.Value);

		public static bool ContainsBqlFieldType(BqlFieldTypeName bqlFieldType) =>
			_bqlFieldTypeToPropertyType.ContainsKey(bqlFieldType.Value);

		public static string? GetBqlFieldType(PropertyTypeName propertyType) =>
			_propertyTypeToBqlFieldType.TryGetValue(propertyType.Value, out var bqlFieldType)
				? bqlFieldType
				: null;

		public static string? GetPropertyType(BqlFieldTypeName bqlFieldType) =>
			_bqlFieldTypeToPropertyType.TryGetValue(bqlFieldType.Value, out var propertyType)
				? propertyType
				: null;
	}
}