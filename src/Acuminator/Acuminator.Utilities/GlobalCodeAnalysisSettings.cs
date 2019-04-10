using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;


namespace Acuminator.Utilities
{
	public class GlobalCodeAnalysisSettings
	{
		public static CodeAnalysisSettings Instance { get; }

		static GlobalCodeAnalysisSettings()
		{
			Instance = GetCodeAnalysisSettings();
		}

		public static CodeAnalysisSettings GetCodeAnalysisSettings()
		{
			CodeAnalysisSettings settings = null;

			try
			{
				if (ServiceLocator.IsLocationProviderSet)
				{
					settings = ServiceLocator.Current.GetInstance<CodeAnalysisSettings>();
				}
			}
			catch
			{
				// TODO: log the exception
			}

			return settings ?? CodeAnalysisSettings.Default;
		}
	}
}
