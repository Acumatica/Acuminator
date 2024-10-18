#nullable enable

using System;
using System.Composition;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities
{
	[Export]
	public class BannedApiSettings : IEquatable<BannedApiSettings>
	{
		public const bool DefaultBannedApiAnalysisEnabled = true;

		public static BannedApiSettings Default { get; } = 
			new BannedApiSettings(DefaultBannedApiAnalysisEnabled, bannedApiFilePath: null, allowedApisFilePath: null);

		public virtual bool BannedApiAnalysisEnabled { get; }

		public virtual string? BannedApiFilePath { get; }

		public virtual string? AllowedApisFilePath { get; }

		public BannedApiSettings(bool bannedApiAnalysisEnabled, string? bannedApiFilePath, string? allowedApisFilePath)
		{
			BannedApiAnalysisEnabled = bannedApiAnalysisEnabled;
			BannedApiFilePath 		 = bannedApiFilePath.NullIfWhiteSpace()?.Trim();
			AllowedApisFilePath 	 = allowedApisFilePath.NullIfWhiteSpace()?.Trim();
		}

		protected BannedApiSettings()
		{
		}

		public BannedApiSettings WithBannedApiAnalysisEnabled() => WithBannedApiAnalysisEnabledValue(true);

		public BannedApiSettings WithBannedApiAnalysisDisabled() => WithBannedApiAnalysisEnabledValue(false);

		protected BannedApiSettings WithBannedApiAnalysisEnabledValue(bool value) =>
			new(value, BannedApiFilePath, AllowedApisFilePath);

		public BannedApiSettings WithBannedApiFilePath(string? bannedApiFilePath) =>
			new(BannedApiAnalysisEnabled, bannedApiFilePath, AllowedApisFilePath);

		public BannedApiSettings WithAllowedApisFilePath(string? allowedApisFilePath) =>
			new(BannedApiAnalysisEnabled, BannedApiFilePath, allowedApisFilePath);

		public override bool Equals(object obj) => Equals(obj as BannedApiSettings);

		public bool Equals(BannedApiSettings? other) =>
			BannedApiAnalysisEnabled == other?.BannedApiAnalysisEnabled && 
			BannedApiFilePath == other.BannedApiFilePath && 
			AllowedApisFilePath == other.AllowedApisFilePath;

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + BannedApiAnalysisEnabled.GetHashCode();
				hash = 23 * hash + (BannedApiFilePath?.GetHashCode() ?? 0);
				hash = 23 * hash + (AllowedApisFilePath?.GetHashCode() ?? 0);
			}

			return hash;
		}
	}
}
