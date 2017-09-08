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
				"(PX)?Select(Join)?",
				"Search",
				@"PXSelectReadonly\d?",
				"PXSelectGroupJoin",
				"PXSelectJoinOrderBy",
				"PXSelectGroupByOrderBy"
		};

		private static readonly string[] bqlParameterNames = new[]
		{
			"Current2?",
			"Optional2?",
			"Required"
		};

		//private static readonly string[] bqlOperatorNames = new[]
		//{
		//	"InnerJoin",
		//	"RightJoin",
		//	"LeftJoin",
		//	"Where",
		//	"On",
		//	"Equal",
		//	"OrderBy",
		//	"Asc",
		//	"Desc",
		//	"Equal",
		//	""

		//};

		public const string DacWithFieldPattern = @"<[\r|\n|\t]*?([A-Z][a-z]*)+\.[\r|\t|\n]*([a-z]+[A-Z]*)+([>|,])?";
		public const string DacOrConstantPattern = @"<[\r|\n|\t]*?([A-Z]\w*)[\r|\n|\t]*(>|\,)";
		public const string DacOperandPattern = @"(,|<)?([A-Z][A-Za-z]*[\d]?)<";

		public static string BQLSelectCommandPattern { get; }

		public static string BQLParametersPattern { get; }

		static RegExpressions()
		{
			BQLSelectCommandPattern = "(" + string.Join("|", bqlSelectNames) + @")<.*?>[^;]*?;";
			BQLParametersPattern = "(" + string.Join("|", bqlParameterNames) + ")";
		}
	}
}
