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
			    "(PX)?Select",			   
				"Search",
				"PXSetup",
				"PXUpdate",
				@"PXSelectReadonly\d?",
				"PXSelectGroupJoin",
				"PXSelectJoin(OrderBy|GroupBy)?",
				"PXSelectGroupByOrderBy",
				"PXProcessing(Join)?"                           
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

		public const string DacWithFieldPattern = @"<[\r|\n|\t]*?([A-Z]+\w*\.)?([A-Z]+\w*)+\d?\.[\r|\t|\n]*([a-z]+\w*\d*)([>|,])?";
		public const string DacOrConstantPattern = @"<[\r|\n|\t]*?([A-Z]+\w*\.)?([A-Z]+\w*\d?)[\r|\n|\t]*(>|\,)";
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
