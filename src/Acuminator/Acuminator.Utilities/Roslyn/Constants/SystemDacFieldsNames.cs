using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Acuminator.Utilities.Roslyn.Constants
{
	/// <summary>
	/// A system DAC fields' names.
	/// </summary>
	internal static class SystemDacFieldsNames
	{
		public const string NoteID = "NoteID";
		public const string CreatedByID = "CreatedByID";
		public const string CreatedByScreenID = "CreatedByScreenID";
		public const string CreatedDateTime = "CreatedDateTime";
		public const string LastModifiedByID = "LastModifiedByID";
		public const string LastModifiedByScreenID = "LastModifiedByScreenID";
		public const string LastModifiedDateTime = "LastModifiedDateTime";
		public const string Timestamp = "tstamp";

		public static ImmutableHashSet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			NoteID,
			CreatedByID,
			CreatedByScreenID,
			CreatedDateTime,
			LastModifiedByID,
			LastModifiedByScreenID,
			LastModifiedDateTime,
			Timestamp,
		}
		.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
