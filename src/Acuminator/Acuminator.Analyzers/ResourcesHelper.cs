using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;

namespace Acuminator.Analyzers
{
	public static class ResourcesHelper
	{
		public static LocalizableString GetLocalized(this string resourceName)
		{
			return new LocalizableResourceString(resourceName, Resources.ResourceManager, typeof(Resources));
		}

		public static LocalizableString GetLocalized<TResource>(this string resourceName, ResourceManager resourceManager)
		{
			resourceManager.ThrowOnNull(nameof(resourceManager));
			return new LocalizableResourceString(resourceName, resourceManager, typeof(TResource));
		}
	}
}
