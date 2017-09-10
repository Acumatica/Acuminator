using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Analyzers.Coloriser
{
	public static class RegExpressions
	{
		private static readonly string[] bqlSelectNames = new[]
		{
               "(PX)?Select(GroupBy)?(OrderBy)?",
                "Search",
				"PXSetup",
				"PXUpdate",
				@"PXSelectReadonly\d?",
				"PXSelectGroupJoin",
				"PXSelectJoin(OrderBy|GroupBy)?",				
                "PX(Filtered)?Processing(Join)?"
        };

		private static readonly string[] bqlParameterNames = new[]
		{
			"Current2?",
			"Optional2?",
			"Required"
		};

        public static readonly string[] KeyWords = 
        {
            "Select",
            "Search"
        };

		public const string DacWithFieldPattern = @"<\W*?([A-Z]+\w*\.)?([A-Z]+\w*)+\d?\.\W*([a-z]+\w*\d*)([>|,])?";
		public const string DacOrConstantPattern = @"<\W*?([A-Z]+\w*\.)?([A-Z]+\w*\d?)\W*(>|\,)";
		public const string DacOperandPattern = @"(,|<)?([A-Z]+\w*)\d?<";

		public static string BQLSelectCommandPattern { get; }

		public static string BQLParametersPattern { get; }

		static RegExpressions()
		{
			BQLSelectCommandPattern = "(" + string.Join("|", bqlSelectNames) + ")" + @"<.*?>\.?[^;\{\}]*?(;|\{|\[)";
            BQLParametersPattern = "(" + string.Join("|", bqlParameterNames) + ")";
		}
	}
}
