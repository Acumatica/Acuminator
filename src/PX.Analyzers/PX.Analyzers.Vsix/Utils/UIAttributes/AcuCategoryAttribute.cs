using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.Shell;

using PX.Analyzers.Vsix;
using PX.Analyzers.Vsix.Utilities;


namespace PX.Analyzers.Vsix
{
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	internal sealed class AcuCategoryAttribute : CategoryAttribute
	{		
		public string ResourceKey { get; }

		protected override string GetLocalizedString(string value)
		{
			string resourceString = VSIXResource.ResourceManager.GetStringResourceSafe(ResourceKey);
			return resourceString ?? base.GetLocalizedString(value);
		}

		public AcuCategoryAttribute(string resourceKey, string nonLocalizedCategoryName) : base(nonLocalizedCategoryName) 
		{			
			resourceKey.ThrowOnNullOrWhiteSpace(nameof(resourceKey));
			ResourceKey = resourceKey;
		}	
	}
}
