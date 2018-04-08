using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Acuminator.Vsix.Coloriser
{
	public static class RegExpressions
	{
		private static readonly string[] bqlSelectNames = new[]
		{
               "(PX)?Select(GroupBy)?(OrderBy)?",
                @"Search\d?",
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

		private const string DacWithFieldPattern = @"<\W*?([A-Z]+\w*\.)?([A-Z]+\w*)+\d?\.\W*([a-z]+\w*\d*)([>|,])?";
        private const string DacOrConstantPattern = @"<\W*?([A-Z]+\w*\.)?([A-Z]+\w*\d?)\W*(>|\,)";
        private const string DacOperandPattern = @"(,|<)?([A-Z]+\w*)\d?<";
        private const string bqlAllowedSymbols = @"[^;\{\}\(\)\[\]]";
        private const string afterBqlAllowedSymbols = @"[^;\{\}\(\)\[\]]";
        private const string bqlEndingSymbol = @"(;|\{|\]|\[|\(|\))";

        public static Regex DacWithFieldRegex { get; } = new Regex(DacWithFieldPattern, RegexOptions.Compiled);

        public static Regex DacOrConstantRegex { get; } = new Regex(DacOrConstantPattern, RegexOptions.Compiled);

        public static Regex DacOperandRegex { get; } = new Regex(DacOperandPattern, RegexOptions.Compiled);

        public static Regex BQLSelectCommandRegex { get; }

		public static Regex BQLParametersRegex { get; }

		static RegExpressions()
		{
			string bqlSelectCommandPattern = "(" + string.Join("|", bqlSelectNames) + ")" + $"<{bqlAllowedSymbols}*>" + @"\.?" +
                                             $"{afterBqlAllowedSymbols}*?" + bqlEndingSymbol;
            string bqlParametersPattern = "(" + string.Join("|", bqlParameterNames) + ")";
            BQLSelectCommandRegex = new Regex(bqlSelectCommandPattern, RegexOptions.Compiled | RegexOptions.Singleline);
            BQLParametersRegex = new Regex(bqlParametersPattern, RegexOptions.Compiled);
        }
	}
}
