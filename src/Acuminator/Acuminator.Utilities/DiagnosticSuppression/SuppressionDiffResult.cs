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
			path.ThrowOnNull(nameof(path));
			assembly.ThrowOnNull(nameof(assembly));
			addedMessages.ThrowOnNull(nameof(addedMessages));
			deletedMessages.ThrowOnNull(nameof(deletedMessages));

			Path = path;
			Assembly = assembly;
			AddedMessages = addedMessages.ToImmutableArray();
			DeletedMessages = deletedMessages.ToImmutableArray();
		}
	}
}
