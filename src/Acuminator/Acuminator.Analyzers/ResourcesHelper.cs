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
		public static LocalizableString GetLocalized(this string resourceName) =>
			new LocalizableResourceString(resourceName, Resources.ResourceManager, typeof(Resources));

		public static LocalizableString GetLocalized(this string resourceName, params string[] formatArgs) =>
			new LocalizableResourceString(resourceName, Resources.ResourceManager, typeof(Resources), formatArgs);

		public static LocalizableString GetLocalized<TResource>(this string resourceName, ResourceManager resourceManager) =>
			new LocalizableResourceString(resourceName, resourceManager.CheckIfNull(nameof(resourceManager)), typeof(TResource));

		public static LocalizableString GetLocalized<TResource>(this string resourceName, ResourceManager resourceManager, 
																params string[] formatArgs) =>
			new LocalizableResourceString(resourceName, resourceManager.CheckIfNull(nameof(resourceManager)), typeof(TResource), formatArgs);
	}
}
