using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Resources;




namespace PX.Analyzers.Vsix.Utilities
{
    public static class ResourceManagerExtensions
	{       
        public static string GetStringResourceSafe(this ResourceManager resourceManager, string resourceKey)
        {
			string result = null;

			try
			{
				result = resourceManager.GetString(resourceKey, CultureInfo.CurrentUICulture);
			}
			catch (Exception e)
			when (e is MissingSatelliteAssemblyException || e is MissingManifestResourceException)
			{
				result = null;
				Debug.Assert(false, $"String resource '{resourceKey}' is missing", e.Message);   //TODO Log warning here instead of Debug.Assert				
			}
			catch (Exception e)
			{
				result = null;
				Debug.Assert(false, $"Error on acquiring resource by key '{resourceKey}'", e.Message);  //TODO Log warning here instead of Debug.Assert
			}

			return result ?? resourceKey;
		}
    }
}
