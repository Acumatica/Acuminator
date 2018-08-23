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
			new CodeAnalysisSettings(recursiveAnalysisEnabled: true);

		public CodeAnalysisSettings()
		{
		}

		private CodeAnalysisSettings(bool recursiveAnalysisEnabled)
		{
			RecursiveAnalysisEnabled = recursiveAnalysisEnabled;
		}

		public virtual bool RecursiveAnalysisEnabled { get; }
	}
}
