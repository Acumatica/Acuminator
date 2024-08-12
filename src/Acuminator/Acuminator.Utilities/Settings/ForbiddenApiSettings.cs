#nullable enable

using System;
using System.Composition;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities
{
	[Export]
	public class ForbiddenApiSettings : IEquatable<ForbiddenApiSettings>
	{
		public static ForbiddenApiSettings Default { get; } = new ForbiddenApiSettings(bannedApiFilePath: null, whiteListApiFilePath: null);

		public string? BannedApiFilePath { get; }

		public string? WhiteListApiFilePath { get; }

		public ForbiddenApiSettings(string? bannedApiFilePath, string? whiteListApiFilePath)
		{
			BannedApiFilePath = bannedApiFilePath.NullIfWhiteSpace()?.Trim();
			WhiteListApiFilePath = whiteListApiFilePath.NullIfWhiteSpace()?.Trim();
		}

		public ForbiddenApiSettings WithBannedApiFilePath(string? bannedApiFilePath) =>
			new(bannedApiFilePath, WhiteListApiFilePath);

		public ForbiddenApiSettings WithWhiteListApiFilePath(string? whiteListApiFilePath) =>
			new(BannedApiFilePath, whiteListApiFilePath);

		public override bool Equals(object obj) => Equals(obj as ForbiddenApiSettings);

		public bool Equals(ForbiddenApiSettings? other) =>
			BannedApiFilePath == other?.BannedApiFilePath && WhiteListApiFilePath == other?.WhiteListApiFilePath;

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + (BannedApiFilePath?.GetHashCode() ?? 0);
				hash = 23 * hash + (WhiteListApiFilePath?.GetHashCode() ?? 0);
			}

			return hash;
		}
	}
}
