#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.Settings
{
	[Export(typeof(CodeAnalysisSettings))]
	internal class CodeAnalysisSettingsFromOptionsPage : CodeAnalysisSettings
	{
		private readonly GeneralOptionsPage _optionsPage;

		[ImportingConstructor]
		public CodeAnalysisSettingsFromOptionsPage(GeneralOptionsPage optionsPage)
		{
			_optionsPage = optionsPage.CheckIfNull();
		}

		public override bool RecursiveAnalysisEnabled => _optionsPage.RecursiveAnalysisEnabled;

		public override bool IsvSpecificAnalyzersEnabled => _optionsPage.IsvSpecificAnalyzersEnabled;

		public override bool StaticAnalysisEnabled => _optionsPage.StaticAnalysisEnabled;

		public override bool SuppressionMechanismEnabled => _optionsPage.SuppressionMechanismEnabled;

		public override bool PX1007DocumentationDiagnosticEnabled => _optionsPage.PX1007DocumentationDiagnosticEnabled;
	}
}
