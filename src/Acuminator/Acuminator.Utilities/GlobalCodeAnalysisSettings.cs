using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities
{
	public class GlobalCodeAnalysisSettings
	{
		private const int NOT_INITIALIZED = 0, INITIALIZED = 1;
		private static int _isInitialized = NOT_INITIALIZED;

		private static CodeAnalysisSettings _cachedSettings;

		public static CodeAnalysisSettings Instance => _cachedSettings ?? CodeAnalysisSettings.Default;

		/// <summary>
		/// Initializes the global settings once. Must be called on package initialization.
		/// </summary>
		/// <param name="instance">The instance.</param>
		public static void InitializeGlobalSettingsOnce(CodeAnalysisSettings instance)
		{
			instance.ThrowOnNull(nameof(instance));

			if (Interlocked.CompareExchange(ref _isInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				_cachedSettings = instance;
			}
		}
	}
}