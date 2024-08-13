#nullable enable

using System;

using System.Threading;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities
{
	public class GlobalSettings
	{
		private const int NOT_INITIALIZED = 0, INITIALIZED = 1;
		private static int _isInitialized = NOT_INITIALIZED;

		private static CodeAnalysisSettings? _cachedCodeAnalysisSettings;
		private static BannedApiSettings? _cachedBannedApiSettings;

		public static CodeAnalysisSettings AnalysisSettings => _cachedCodeAnalysisSettings ?? CodeAnalysisSettings.Default;

		public static BannedApiSettings BannedApiSettings => _cachedBannedApiSettings ?? BannedApiSettings.Default;

		/// <summary>
		/// Initializes the global settings once. Must be called on package initialization.
		/// </summary>
		/// <param name="codeAnalysisSettings">The instance.</param>
		public static void InitializeGlobalSettingsOnce(CodeAnalysisSettings codeAnalysisSettings, BannedApiSettings bannedApiSettings)
		{
			codeAnalysisSettings.ThrowOnNull();

			if (Interlocked.CompareExchange(ref _isInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				_cachedCodeAnalysisSettings = codeAnalysisSettings;
			}
		}
	}
}