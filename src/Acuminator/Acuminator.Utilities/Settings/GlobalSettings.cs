using System;
using System.Threading;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities
{
	public static class GlobalSettings
	{
		private const int NOT_INITIALIZED = 0, INITIALIZED = 1;
		private static int _isInitialized = NOT_INITIALIZED;

		private static CodeAnalysisSettings? _cachedCodeAnalysisSettings;
		private static BannedApiSettings? _cachedBannedApiSettings;

		public static AnalysisHostType HostType { get; private set; } = AnalysisHostType.NotSpecified;

		public static CodeAnalysisSettings AnalysisSettings => _cachedCodeAnalysisSettings ?? CodeAnalysisSettings.Default;

		public static BannedApiSettings BannedApiSettings => _cachedBannedApiSettings ?? BannedApiSettings.Default;

		/// <summary>
		/// Initializes the global settings once. Must be called on package initialization.
		/// </summary>
		/// <param name="codeAnalysisSettings">The code analysis settings.</param>
		/// <param name="bannedApiSettings">The banned API settings.</param>
		/// <param name="hostType">The type of the host in which the analysis is performed.</param>
		public static void InitializeGlobalSettingsOnce(CodeAnalysisSettings codeAnalysisSettings, BannedApiSettings bannedApiSettings,
														AnalysisHostType hostType)
		{
			codeAnalysisSettings.ThrowOnNull();
			bannedApiSettings.ThrowOnNull();

			if (Interlocked.CompareExchange(ref _isInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				_cachedCodeAnalysisSettings = codeAnalysisSettings;
				_cachedBannedApiSettings 	= bannedApiSettings;
				HostType = hostType;
			}
		}

		/// <summary>
		/// Initializes the global settings in a thread unsafe way. For tests only.
		/// </summary>
		/// <param name="codeAnalysisSettings">The code analysis settings.</param>
		/// <param name="bannedApiSettings">The banned API settings.</param>
		/// <param name="hostType">The type of the host in which the analysis is performed.</param>
		internal static void InitializeGlobalSettingsThreadUnsafeForTestsOnly(CodeAnalysisSettings codeAnalysisSettings, 
																			  BannedApiSettings bannedApiSettings)
		{
			_cachedCodeAnalysisSettings = codeAnalysisSettings.CheckIfNull();
			_cachedBannedApiSettings 	= bannedApiSettings.CheckIfNull();
			HostType = AnalysisHostType.NotSpecified;
		}
	}
}