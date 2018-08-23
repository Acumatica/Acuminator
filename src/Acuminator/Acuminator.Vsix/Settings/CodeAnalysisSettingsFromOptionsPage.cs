using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Utilities;

namespace Acuminator.Vsix.Settings
{
	[Export(typeof (CodeAnalysisSettings))]
	class CodeAnalysisSettingsFromOptionsPage : CodeAnalysisSettings
	{
		private readonly GeneralOptionsPage _optionsPage;

		[ImportingConstructor]
		public CodeAnalysisSettingsFromOptionsPage(GeneralOptionsPage optionsPage)
		{
			optionsPage.ThrowOnNull(nameof (optionsPage));

			_optionsPage = optionsPage;
		}

		public override bool RecursiveAnalysisEnabled => _optionsPage.RecursiveAnalysisEnabled;
	}
}
