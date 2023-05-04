using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Acuminator.Utilities.Roslyn.Constants
{
	/// <summary>
	/// Constants representing DAC field names.
	/// </summary>
	public static class DacFieldNames
	{
		/// <summary>
		/// Names of system DAC fields.
		/// </summary>
		public static class System
		{
			public const string NoteID                 = "NoteID";
			public const string CreatedByID            = "CreatedByID";
			public const string CreatedByScreenID      = "CreatedByScreenID";
			public const string CreatedDateTime        = "CreatedDateTime";
			public const string LastModifiedByID       = "LastModifiedByID";
			public const string LastModifiedByScreenID = "LastModifiedByScreenID";
			public const string LastModifiedDateTime   = "LastModifiedDateTime";
			public const string Timestamp              = "tstamp";

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

		/// <summary>
		/// Names of well-known DAC fields.
		/// </summary>
		public static class WellKnown
		{
			public const string Selected = "Selected";
		}
	}
}
