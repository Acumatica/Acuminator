using System;

namespace Acuminator.Utilities.Roslyn.Constants
{
	public static class PropertyNames
	{
		public static class Attributes
		{
			public const string IsKey 	 = "IsKey";
			public const string BqlTable = "BqlTable";
			public const string BqlField = "BqlField";
		}

		public static class Exception
		{
			public const string Message 		 = nameof(System.Exception.Message);
			public const string MessageFormat 	 = "MessageFormat";
			public const string MessageArguments = "MessageArguments";
		}
	}
}
