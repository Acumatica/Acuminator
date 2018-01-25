using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Analyzers.Coloriser
{
	public static class Constants
	{
		public const string Priority = Microsoft.VisualStudio.Text.Classification.Priority.High;

		public const string DacFormat = nameof(DacFormat);
        public const string DacExtensionFormat = nameof(DacExtensionFormat);
        public const string DacFieldFormat = nameof(DacFieldFormat);
		public const string BQLParameterFormat = nameof(BQLParameterFormat);
		public const string BQLOperatorFormat = nameof(BQLOperatorFormat);
		public const string BQLConstantPrefixFormat = nameof(BQLConstantPrefixFormat);
		public const string BQLConstantEndingFormat = nameof(BQLConstantEndingFormat);

        public const string BraceLevel_1_Format = nameof(BraceLevel_1_Format);
        public const string BraceLevel_2_Format = nameof(BraceLevel_2_Format);
        public const string BraceLevel_3_Format = nameof(BraceLevel_3_Format);

        public const string BraceLevel_4_Format = nameof(BraceLevel_4_Format);
        public const string BraceLevel_5_Format = nameof(BraceLevel_5_Format);
        public const string BraceLevel_6_Format = nameof(BraceLevel_6_Format);

        public const string BraceLevel_7_Format = nameof(BraceLevel_7_Format);
        public const string BraceLevel_8_Format = nameof(BraceLevel_8_Format);
        public const string BraceLevel_9_Format = nameof(BraceLevel_9_Format);

        public const string BraceLevel_10_Format = nameof(BraceLevel_10_Format);
        public const string BraceLevel_11_Format = nameof(BraceLevel_11_Format);
        public const string BraceLevel_12_Format = nameof(BraceLevel_12_Format);

        public const string BraceLevel_13_Format = nameof(BraceLevel_13_Format);
        public const string BraceLevel_14_Format = nameof(BraceLevel_14_Format);

        public const int MaxBraceLevel = 14;

        /// <summary>
        /// Count of visited nodes for Text editor update in Roslyn colorizer
        /// </summary>
        public const long ChunkSize = 200;
    }

	public static class Labels
	{
		public const string DacFormatLabel  = "Acuminator - DAC Name";
        public const string DacExtensionFormatLabel = "Acuminator - DAC Extension Name";
        public const string DacFieldFormatLabel = "Acuminator - DAC Field Name";
		public const string BQLParameterFormatLabel = "Acuminator - BQL parameters";
		public const string BQLOperatorFormatLabel = "Acuminator - BQL operators";
		public const string BQLConstantPrefixFormatLabel = "Acuminator - BQL constant - prefix";
		public const string BQLConstantEndingFormatLabel = "Acuminator - BQL constant - ending";

        public const string BraceLevel_1_FormatLabel = "Acuminator - BQL angle braces level 1";
        public const string BraceLevel_2_FormatLabel = "Acuminator - BQL angle braces level 2";
        public const string BraceLevel_3_FormatLabel = "Acuminator - BQL angle braces level 3";

        public const string BraceLevel_4_FormatLabel = "Acuminator - BQL angle braces level 4";
        public const string BraceLevel_5_FormatLabel = "Acuminator - BQL angle braces level 5";
        public const string BraceLevel_6_FormatLabel = "Acuminator - BQL angle braces level 6";

        public const string BraceLevel_7_FormatLabel = "Acuminator - BQL angle braces level 7";
        public const string BraceLevel_8_FormatLabel = "Acuminator - BQL angle braces level 8";
        public const string BraceLevel_9_FormatLabel = "Acuminator - BQL angle braces level 9";

        public const string BraceLevel_10_FormatLabel = "Acuminator - BQL angle braces level 10";
        public const string BraceLevel_11_FormatLabel = "Acuminator - BQL angle braces level 11";
        public const string BraceLevel_12_FormatLabel = "Acuminator - BQL angle braces level 12";

        public const string BraceLevel_13_FormatLabel = "Acuminator - BQL angle braces level 13";
        public const string BraceLevel_14_FormatLabel = "Acuminator - BQL angle braces level 14";      
    }
}
