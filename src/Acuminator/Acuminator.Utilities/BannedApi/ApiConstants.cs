namespace Acuminator.Utilities.BannedApi
{
	public static class ApiConstants
	{
		public static class Storage
		{
			public const string BannedApiFile = "BannedApis.txt";
			public const string WhiteListFile = "WhiteList.txt";

			public const string BannedApiAssemblyResourceName = $"BannedApi.Data.{BannedApiFile}";
			public const string WhiteListAssemblyResourceName = $"BannedApi.Data.{WhiteListFile}";
		}

		public static class Format
		{
			public static class Chars
			{
				public const char NamespaceSeparator = '-';
				public const char NestedTypesSeparator = '+';
			}

			public static class Strings
			{
				public const string NamespaceSeparator = "-";
				public const string NestedTypesSeparator = "+";
			}
		}
	}
}
