#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

using Acuminator.Vsix.Logger;

namespace Acuminator.Vsix.Utilities
{
    public static class ResourceManagerExtensions
	{       
        public static string GetStringResourceSafe(this ResourceManager resourceManager, string resourceKey)
        {
			string? result = null;

			try
			{
				result = resourceManager.GetString(resourceKey, CultureInfo.CurrentUICulture);
			}
			catch (Exception e)
			when (e is MissingSatelliteAssemblyException || e is MissingManifestResourceException)
			{
				result = null;
				string errorMessage = $"String resource '{resourceKey}' is missing.{Environment.NewLine}{e}";
				AcuminatorLogger.LogMessage(errorMessage, LogMode.Warning);
			}
			catch (Exception e)
			{
				result = null;
				string errorMessage = $"Error on acquiring resource by key '{resourceKey}'.{Environment.NewLine}{e}";
				AcuminatorLogger.LogMessage(errorMessage, LogMode.Warning);
			}

			return result ?? resourceKey;
		}
    }
}
