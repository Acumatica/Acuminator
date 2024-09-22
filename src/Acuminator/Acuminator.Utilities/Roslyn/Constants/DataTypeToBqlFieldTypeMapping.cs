using System;
using System.Collections.Generic;

namespace Acuminator.Utilities.Roslyn.Constants
{
	/// <summary>
	/// A mapping between data types (such as C# property type) to BQL field type (such as BqlString).
	/// </summary>
	public static class DataTypeToBqlFieldTypeMapping
	{
		private static readonly Dictionary<string, string> _dataTypeToBqlFieldType = new(StringComparer.OrdinalIgnoreCase)
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

			{ "byte[]"   , "BqlByteArray" },
			{ "ByteArray", "BqlByteArray" },

			{ "string[]"   , TypeNames.BqlField.BqlAttributes },
			{ "StringArray", TypeNames.BqlField.BqlAttributes }
		};

		private static readonly Dictionary<string, string> _bqlFieldTypeToDataType = new(StringComparer.OrdinalIgnoreCase)
		{
			{ "BqlString"	 , nameof(String)  },
			{ "BqlGuid" 	 , nameof(Guid)    },
			{ "BqlDateTime"	 , nameof(DateTime)},
			{ "BqlBool"		 , nameof(Boolean) },
			{ "BqlByte"		 , nameof(Byte)    },
			{ "BqlShort"	 , nameof(Int16)   },
			{ "BqlInt"		 , nameof(Int32)   },
			{ "BqlLong"		 , nameof(Int64)   },
			{ "BqlFloat"	 , nameof(Single)  },
			{ "BqlDouble"	 , nameof(Double)  },
			{ "BqlDecimal"	 , nameof(Decimal) },
			{ "BqlByteArray" , $"byte[]" 	   },

			{ TypeNames.BqlField.BqlAttributes, $"string[]" }
		};

		public static bool ContainsDataType(DataTypeName dataType) =>
			_dataTypeToBqlFieldType.ContainsKey(dataType.Value);

		public static bool ContainsBqlFieldType(BqlFieldTypeName bqlFieldType) =>
			_bqlFieldTypeToDataType.ContainsKey(bqlFieldType.Value);

		public static string? GetBqlFieldType(DataTypeName dataType) =>
			_dataTypeToBqlFieldType.TryGetValue(dataType.Value, out var bqlFieldType)
				? bqlFieldType
				: null;

		public static string? GetDataType(BqlFieldTypeName bqlFieldType) =>
			_bqlFieldTypeToDataType.TryGetValue(bqlFieldType.Value, out var dataType)
				? dataType
				: null;
	}
}