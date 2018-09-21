using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Utilities
{
	[Export]
	public class CodeAnalysisSettings
	{
		public static CodeAnalysisSettings Default => 
			new CodeAnalysisSettings(
				recursiveAnalysisEnabled: true, 
				isvSpecificAnalyzersEnabled: false);

		public CodeAnalysisSettings()
		{
		}

		private CodeAnalysisSettings(bool recursiveAnalysisEnabled, bool isvSpecificAnalyzersEnabled)
		{
			RecursiveAnalysisEnabled = recursiveAnalysisEnabled;
			IsvSpecificAnalyzersEnabled = isvSpecificAnalyzersEnabled;
		}

		public virtual bool RecursiveAnalysisEnabled { get; }
		public virtual bool IsvSpecificAnalyzersEnabled { get; }

		public CodeAnalysisSettings WithRecursiveAnalysisEnabled()
		{
			return WithRecursiveAnalysisEnabledValue(true);
		}

		public CodeAnalysisSettings WithRecursiveAnalysisDisabled()
		{
			return WithRecursiveAnalysisEnabledValue(false);
		}

		public CodeAnalysisSettings WithIsvSpecificAnalyzersEnabled()
		{
			return WithIsvSpecificAnalyzersEnabledValue(true);
		}

		public CodeAnalysisSettings WithIsvSpecificAnalyzersDisabled()
		{
			return WithIsvSpecificAnalyzersEnabledValue(false);
		}

		protected virtual CodeAnalysisSettings WithRecursiveAnalysisEnabledValue(bool value)
		{
			return new CodeAnalysisSettings(value, IsvSpecificAnalyzersEnabled);
		}

		protected virtual CodeAnalysisSettings WithIsvSpecificAnalyzersEnabledValue(bool value)
		{
			return new CodeAnalysisSettings(RecursiveAnalysisEnabled, value);
		}
	}
}
