#nullable enable

using System;
using System.Composition;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities
{
	[Export]
	public class BannedApiSettings : IEquatable<BannedApiSettings>
	{
		public static BannedApiSettings Default { get; } = new BannedApiSettings(bannedApiFilePath: null, whiteListApiFilePath: null);

		public virtual string? BannedApiFilePath { get; }

		public virtual string? WhiteListApiFilePath { get; }

		public BannedApiSettings(string? bannedApiFilePath, string? whiteListApiFilePath)
		{
			BannedApiFilePath = bannedApiFilePath.NullIfWhiteSpace()?.Trim();
			WhiteListApiFilePath = whiteListApiFilePath.NullIfWhiteSpace()?.Trim();
		}

		protected BannedApiSettings()
		{
		}

		public BannedApiSettings WithBannedApiFilePath(string? bannedApiFilePath) =>
			new(bannedApiFilePath, WhiteListApiFilePath);

		public BannedApiSettings WithWhiteListApiFilePath(string? whiteListApiFilePath) =>
			new(BannedApiFilePath, whiteListApiFilePath);

		public override bool Equals(object obj) => Equals(obj as BannedApiSettings);

		public bool Equals(BannedApiSettings? other) =>
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
