using System;

namespace Acuminator.Runner.Output.Grouping
{
	/// <summary>
	/// Report grouping modes.
	/// </summary>
	[Flags]
	internal enum GroupingMode
	{
		/// <summary>
		/// No grouping is specified for the report.
		/// </summary>
		None = 0b0000,

		/// <summary>
		/// Group found errors by source file.
		/// </summary>
		Files = 0b0001,

		/// <summary>
		/// Group found errors by diagnostic ID.
		/// </summary>
		DiagnosticIDs = 0b0010
	}

	internal static class GroupingModeExtensions
	{
		public static bool HasGrouping(this GroupingMode groupingMode, GroupingMode groupingToCheck) =>
			(groupingMode & groupingToCheck) == groupingToCheck;
	}
}