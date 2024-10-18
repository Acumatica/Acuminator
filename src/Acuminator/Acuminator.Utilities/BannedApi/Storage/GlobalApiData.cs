using System;
using System.Threading;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.BannedApi.Storage;

/// <summary>
/// A global API data storage. 
/// </summary>
/// <remarks>
/// This is specialized class that caches generic purpose <see cref="ApiStorageProvider"/> type.<br/>
/// It uses a specialized API storage provider that is created from the combination of API data embedded into the Utilities assembly as resources,<br/>
/// and an external file with API data. If a storage is a requested for a different external file, than the one that was used previously,<br/>
/// then a new API storage provider is created and cached.
/// </remarks>
public sealed class GlobalApiData
{
	private static readonly GlobalApiData _bannedApiStorage  = new();
	private static readonly GlobalApiData _allowedApiStorage = new();

	private readonly object _initializationLock = new object();

	private volatile string? _lastUsedApiDataFilePath;
	private volatile ApiStorageProvider? _cachedApiStorageProvider;

	private GlobalApiData() { }

	public static IApiStorage GetBannedApiData(string? externalBannedApiFilePath, CancellationToken cancellation) =>
		_bannedApiStorage.GetApiData(externalBannedApiFilePath, ApiConstants.Storage.BannedApiAssemblyResourceName, cancellation);

	public static IApiStorage GetAllowedApisData(string? externalAllowedApisFilePath, CancellationToken cancellation) =>
		_allowedApiStorage.GetApiData(externalAllowedApisFilePath, ApiConstants.Storage.AllowedApiAssemblyResourceName, cancellation);

	private IApiStorage GetApiData(string? externalApiFilePath, string assemblyResourceName, CancellationToken cancellation)
	{
		cancellation.ThrowIfCancellationRequested();

		externalApiFilePath = externalApiFilePath.NullIfWhiteSpace();

		if (_cachedApiStorageProvider != null && IsSameFileUsed(externalApiFilePath))
			return _cachedApiStorageProvider.GetStorage(cancellation);

		lock (_initializationLock)
		{
			if (_cachedApiStorageProvider != null && IsSameFileUsed(externalApiFilePath))
				return _cachedApiStorageProvider.GetStorage(cancellation);

			_cachedApiStorageProvider = new ApiStorageProvider(externalApiFilePath, typeof(GlobalApiData).Assembly, assemblyResourceName);
			_lastUsedApiDataFilePath = externalApiFilePath;
		}
		
		return _cachedApiStorageProvider.GetStorage(cancellation);
	}

	private bool IsSameFileUsed(string? externalBannedApiFilePath) =>
		string.Equals(_lastUsedApiDataFilePath, externalBannedApiFilePath, StringComparison.OrdinalIgnoreCase);
}
