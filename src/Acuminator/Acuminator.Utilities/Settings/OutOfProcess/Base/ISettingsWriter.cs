using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;

namespace Acuminator.Utilities.Settings.OutOfProcess
{
	public interface ISettingsWriter
	{
		void WriteSettingsToOtherProcesses(CodeAnalysisSettings codeAnalysisSettings);
	}
}
