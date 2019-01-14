using Acuminator.Utilities.Common;
using System;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public class SuppressionFileEventArgs : EventArgs
	{
		public string FullPath { get; }

		public string Name { get; }

		public SuppressionFileEventArgs(string fullPath, string name)
		{
			fullPath.ThrowOnNullOrWhiteSpace(nameof(fullPath));
			name.ThrowOnNullOrWhiteSpace(nameof(name));

			FullPath = fullPath;
			Name = name;
		}
	}
}
