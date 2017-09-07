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
				"PXSelect",
				"PXSelectJoin",
				"PXSelectReadonly",
				"PXSelectReadonly2",
				"PXSelectReadonly3",
				"PXSelectGroupJoin",
				"PXSelectJoinOrderBy",
				"PXSelectGroupByOrderBy"
		};

		private static readonly string[] bqlParameterNames = new[]
		{
			"Current",
			"Optional",
			"Required"
		};

		public const string DacWithFieldPattern = @"<([A-Z][a-z]*)+\.[\r|\t|\n]*([a-z]+[A-Z]*)+([>|,])?";
		public const string DacOrConstantPattern = @"<([A-Z]\w*)[\r|\n|\t]*(>|\,)";

		public static string BQLSelectCommandPattern { get; }

		public static string BQLParametersPattern { get; }

		static RegExpressions()
		{
			BQLSelectCommandPattern = "(" + string.Join("|", bqlSelectNames) + @")[\r|\t|\n]*\<.*\>.*;";
			BQLParametersPattern = "(" + string.Join("|", bqlParameterNames) + ")";
		}
	}
}
