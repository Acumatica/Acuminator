using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Analyzers.Coloriser
{
	public static class ColoringConstants  
	{
		public const string Priority = Microsoft.VisualStudio.Text.Classification.Priority.High;

		public const string DacFormat = nameof(DacFormat);
        public const string DacExtensionFormat = nameof(DacExtensionFormat);
        public const string DacFieldFormat = nameof(DacFieldFormat);
		public const string BQLParameterFormat = nameof(BQLParameterFormat);
		public const string BQLOperatorFormat = nameof(BQLOperatorFormat);
		public const string BQLConstantPrefixFormat = nameof(BQLConstantPrefixFormat);
		public const string BQLConstantEndingFormat = nameof(BQLConstantEndingFormat);

        public const string PXGraphFormat = nameof(PXGraphFormat);
        public const string PXActionFormat = nameof(PXActionFormat);

        //******************************************************************************
        #region Brace Constants
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
        #endregion
        //******************************************************************************
        /// <summary>
        /// Count of visited nodes for Text editor update in Roslyn colorizer
        /// </summary>
        public const long ChunkSize = 400;

        public const string PlatformDllName = "PX.Data";
        public const string AppDllName = "PX.Objects";
    }
}
