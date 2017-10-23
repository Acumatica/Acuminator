using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Analyzers
{
	public static class Constants
	{
		public const string Priority = Microsoft.VisualStudio.Text.Classification.Priority.High;

		public const string DacFormat = nameof(DacFormat);
		public const string DacFieldFormat = nameof(DacFieldFormat);
		public const string BQLParameterFormat = nameof(BQLParameterFormat);
		public const string BQLOperatorFormat = nameof(BQLOperatorFormat);
	}

	public static class Labels
	{
		public const string DacFormatLabel  = "NoBrains - DAC Format";
		public const string DacFieldFormatLabel = "NoBrains - DAC Field Format";
		public const string BQLParameterFormatLabel = "NoBrains - BQL parameters";
		public const string BQLOperatorFormatLabel = "NoBrains - BQL operators";
	}
}
