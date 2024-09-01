#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.BannedApi.Providers;

namespace Acuminator.Utilities.BannedApi.Storage
{
    /// <summary>
    /// An API storage helper that keeps and retrieves the API storage.
    /// </summary>
    public partial class ApiStorageProvider
    {
		private readonly object _initializationRegularLock = new object();
		private readonly SemaphoreSlim _initializationSemaphoreLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private volatile IApiStorage? _instance;

		private readonly string? _dataFileRelativePath;
		private readonly string? _dataAssemblyResourceName;

		public ApiStorageProvider(string? dataFileRelativePath, string? dataAssemblyResourceName)
		{
			_dataFileRelativePath = dataFileRelativePath.NullIfWhiteSpace();
			_dataAssemblyResourceName = dataAssemblyResourceName.NullIfWhiteSpace();
		}

		public IApiStorage GetStorage(CancellationToken cancellation, IApiDataProvider? customApiDataProvider = null)
        {
			cancellation.ThrowIfCancellationRequested();

			if (_instance != null)
				return _instance;

			lock (_initializationRegularLock)
			{
				if (_instance == null)
					_instance = GetStorageWithoutLocking(cancellation, customApiDataProvider);

				return _instance;
			}
		}

		private IApiStorage GetStorageWithoutLocking(CancellationToken cancellation, IApiDataProvider? customBannedApiDataProvider)
		{
			var bannedApiDataProvider = customBannedApiDataProvider ?? GetDefaultDataProvider();
			var bannedApis = bannedApiDataProvider.GetApiData(cancellation);

			cancellation.ThrowIfCancellationRequested();

			return bannedApis == null
				? new DefaultApiStorage()
				: new DefaultApiStorage(bannedApis);
		}

		public async Task<IApiStorage> GetStorageAsync(CancellationToken cancellation, IApiDataProvider? customBannedApiDataProvider = null)
        {
			cancellation.ThrowIfCancellationRequested();

            if (_instance != null)
                return _instance;

			await _initializationSemaphoreLock.WaitAsync(cancellation).ConfigureAwait(false);

			try
			{
				if (_instance == null)
					_instance = await GetStorageAsyncWithoutLockingAsync(cancellation, customBannedApiDataProvider).ConfigureAwait(false);

				return _instance;
			}
			finally
			{
				_initializationSemaphoreLock.Release();
			}		
		}

        private async Task<IApiStorage> GetStorageAsyncWithoutLockingAsync(CancellationToken cancellation, IApiDataProvider? customBannedApiDataProvider)
        {
			var bannedApiDataProvider = customBannedApiDataProvider ?? GetDefaultDataProvider();

			var bannedApis = await bannedApiDataProvider.GetApiDataAsync(cancellation).ConfigureAwait(false);
			cancellation.ThrowIfCancellationRequested();

			return bannedApis == null
				? new DefaultApiStorage()
				: new DefaultApiStorage(bannedApis);
		}

		private IApiDataProvider GetDefaultDataProvider()
        {
			if (_dataFileRelativePath == null && _dataAssemblyResourceName == null)
				return new EmptyProvider(considerDataAvailable: false);

			Assembly assembly 		 = typeof(ApiStorageProvider).Assembly;
			var fileDataProvider 	 = MakeFileDataProvider(assembly);
			var assemblyDataProvider = MakeAssemblyDataProvider(assembly);

			if (fileDataProvider == null)
				return assemblyDataProvider ?? new EmptyProvider(considerDataAvailable: false) as IApiDataProvider;
			else if (assemblyDataProvider == null)
				return fileDataProvider;

			var apiDataProviders = new IApiDataProvider[] { fileDataProvider, assemblyDataProvider };
			var defaultProvider = new DataProvidersCoalesceCombinator(apiDataProviders);
			return defaultProvider;
        }

		private FileDataProvider? MakeFileDataProvider(Assembly currentAssembly)
		{
			if (_dataFileRelativePath == null || currentAssembly.Location.IsNullOrWhiteSpace())
				return null;

			string folderWithExtension = Path.GetDirectoryName(currentAssembly.Location);
			string filePath = Path.Combine(folderWithExtension, _dataFileRelativePath);

			return new FileDataProvider(filePath);
		}

		private AssemblyResourcesDataProvider? MakeAssemblyDataProvider(Assembly currentAssembly)
		{
			if (_dataAssemblyResourceName == null)
				return null;

			string assemblyName = currentAssembly.GetName().Name;
			string fullResourceName = $"{assemblyName}.{_dataAssemblyResourceName}";

			return new AssemblyResourcesDataProvider(currentAssembly, fullResourceName);
		}
	}
}
