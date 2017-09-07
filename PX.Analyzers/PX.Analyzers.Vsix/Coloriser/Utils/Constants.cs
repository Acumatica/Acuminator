using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Analyzers
{
	public static class Constants
	{
		public const string Priority = Microsoft.VisualStudio.Text.Classification.Priority.High;

		public const String DacFormat = "DACName";
		public const String DacFieldFormat = "DACFieldName";
		//public const String ExtensionMethodFormat = "s.roslyn.extension-method";
	}

	public static class Labels
	{
		public const String DacFormatLabel  = "DAC Format";
		public const String DacFieldFormatLabel = "DAC Field Format";
	}
}
