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
			Path = path.CheckIfNull();
			Assembly = assembly.CheckIfNull();

			AddedMessages = addedMessages.CheckIfNull()
										 .Where(m => m.IsValid)
										 .ToImmutableArray();

			DeletedMessages = deletedMessages.CheckIfNull()
											 .Where(m => m.IsValid)
											 .ToImmutableArray();
		}
	}
}
