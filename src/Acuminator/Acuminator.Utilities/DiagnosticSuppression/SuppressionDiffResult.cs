using System;
using System.Linq;
using Acuminator.Utilities.Common;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public readonly struct SuppressionDiffResult
	{
		public string Path { get; }

		public string Assembly { get; }

		public ImmutableArray<SuppressMessage> AddedMessages { get; }

		public ImmutableArray<SuppressMessage> DeletedMessages { get; }

		public SuppressionDiffResult(string path, string assembly, HashSet<SuppressMessage> addedMessages, HashSet<SuppressMessage> deletedMessages)
		{
			Path = path.CheckIfNull(nameof(path));
			Assembly = assembly.CheckIfNull(nameof(assembly));

			AddedMessages = addedMessages.CheckIfNull(nameof(addedMessages))
										 .Where(m => m.IsValid)
										 .ToImmutableArray();

			DeletedMessages = deletedMessages.CheckIfNull(nameof(deletedMessages))
											 .Where(m => m.IsValid)
											 .ToImmutableArray();
		}
	}
}
