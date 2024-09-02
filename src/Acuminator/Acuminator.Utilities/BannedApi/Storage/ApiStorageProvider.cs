using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.BannedApi.Providers;

namespace Acuminator.Utilities.BannedApi.Storage
{
	/// <summary>
	/// An API storage helper that keeps and retrieves the API storage.
	/// </summary>
	/// <remarks>
	/// The API storage provider caches an instance of the API storage created by the provider.<br/>
	/// For the cases when the underlying API source is modified, the provider will create and change a new instance of the API storage.<br/>
	/// The update of the instance is based on the pull basis, i.e. the instance is updated only when it is requested by the consumer.<br/><br/>
	/// 
	/// This is OK for analyzers that request the API data at the beginning of the analysis, and then use it for the entire analysis iteration.<br/>
	/// They will get the updated API data only on the next analysis iteration.<br/><br/>
	/// 
	/// If a push model with a file watcher was used instead (which would require more system resources), then the analyzers would get the updated API data immediately after the change.<br/>
	/// This would lead to an inconsistent state of the analysis results, as the analyzers would report some results based on the old API data and some based on the new API data.<br/>
	/// </remarks>
	public partial class ApiStorageProvider
	{
		private readonly object _initializationLock = new object();

		private volatile IApiStorage? _instance;
		private readonly IApiDataProvider _apiDataProvider;

		public ApiStorageProvider(string? dataFilePath, Assembly? assemblyWithResource, string? dataAssemblyResourceName)
		{
			_apiDataProvider = CreateApiDataProvider(dataFilePath, assemblyWithResource, dataAssemblyResourceName);
		}

		public ApiStorageProvider(IApiDataProvider apiDataProvider)
		{
			_apiDataProvider = apiDataProvider.CheckIfNull();
		}

		public IApiStorage GetStorage(CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			if (_instance != null && !_apiDataProvider.HasApiDataSourceChanged())
				return _instance;

			lock (_initializationLock)
			{
				if (_instance == null || _apiDataProvider.HasApiDataSourceChanged())
				{
					_instance = GetStorageWithoutLocking(cancellation);
				}

				return _instance;
			}
		}

		private IApiStorage GetStorageWithoutLocking(CancellationToken cancellation)
		{
			var apiData = _apiDataProvider.GetApiData(cancellation);

			cancellation.ThrowIfCancellationRequested();

			return apiData == null
				? new DefaultApiStorage()
				: new DefaultApiStorage(apiData);
		}

		private static IApiDataProvider CreateApiDataProvider(string? dataFilePath, Assembly? assemblyWithResource, string? dataAssemblyResourceName)
		{
			dataFilePath = dataFilePath.NullIfWhiteSpace();
			dataAssemblyResourceName = dataAssemblyResourceName.NullIfWhiteSpace();
			bool hasAssemblyResource = assemblyWithResource != null && dataAssemblyResourceName != null;

			if (dataFilePath == null && !hasAssemblyResource)
				return new EmptyProvider(considerDataAvailable: false);

			var fileDataProvider = dataFilePath != null && System.IO.File.Exists(dataFilePath)
				? new FileDataProvider(dataFilePath)
				: null;
			var assemblyDataProvider = hasAssemblyResource
				? new AssemblyResourcesDataProvider(assemblyWithResource!, dataAssemblyResourceName!)
				: null;

			if (fileDataProvider == null)
				return assemblyDataProvider ?? new EmptyProvider(considerDataAvailable: false) as IApiDataProvider;
			else if (assemblyDataProvider == null)
				return fileDataProvider;

			var defaultProvider = new DataProvidersCoalesceCombinator([fileDataProvider, assemblyDataProvider]);
			return defaultProvider;
		}
	}
}
