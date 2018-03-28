using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers
{
	internal static class ResourcesHelper
	{
		public static LocalizableString GetLocalized(this string resourceName)
		{
			return new LocalizableResourceString(resourceName, Resources.ResourceManager, typeof(Resources));
		}
	}
}
