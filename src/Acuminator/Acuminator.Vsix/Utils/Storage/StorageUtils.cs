#nullable enable

using System;
using System.IO;

using Acuminator.Vsix.Logger;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.Utilities.Storage
{
	/// <summary>
	/// Storage utilities.
	/// </summary>
	internal static class StorageUtils
	{
		public static bool ReCreateDirectory(string directory)
		{
			try
			{
				if (Directory.Exists(directory.CheckIfNullOrWhiteSpace()))
					Directory.Delete(directory, recursive: true);

				Directory.CreateDirectory(directory);
				return true;
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return false;
			}
		}

		public static bool CreateDirectory(string directory)
		{
			try
			{
				if (!Directory.Exists(directory.CheckIfNullOrWhiteSpace()))
					Directory.CreateDirectory(directory);

				return true;
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return false;
			}
		}
	}
}
