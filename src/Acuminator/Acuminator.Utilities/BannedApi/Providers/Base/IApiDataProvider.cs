using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.BannedApi.Model;

namespace Acuminator.Utilities.BannedApi.Providers
{
    public interface IApiDataProvider
	{
		/// <summary>
		/// Gets a value indicating whether the provider's API data is available.
		/// </summary>
		/// <value>
		/// True if the API data is available, false if not.
		/// </value>
		bool IsDataAvailable { get; }

		/// <summary>
		/// Gets a value indicating whether the API data source has changed and the data obtained from it by the provider may require refresh.
		/// </summary>
		/// <value>
		/// True if the API data source has changed, false if not.
		/// </value>
		bool HasApiDataSourceChanged();

		/// <summary>
		/// Gets API data synchronously from the provider or <see langword="null"/> if the provider's API data is not available. <br/>
		/// On the latter case the <see cref="IsDataAvailable"/> flag value is <see langword="false"/>.
		/// </summary>
		/// <param name="cancellation">A token that allows processing to be cancelled.</param>
		/// <returns>
		/// The API data.
		/// </returns>
		IEnumerable<Api>? GetApiData(CancellationToken cancellation);

		/// <summary>
		/// Gets the API data asynchronously from the provider or <see langword="null"/> if the provider's API data is not available. <br/>
		/// On the latter case the <see cref="IsDataAvailable"/> flag value is <see langword="false"/>.
		/// </summary>
		/// <param name="cancellation">A token that allows processing to be cancelled.</param>
		/// <returns>
		/// The task with API data.
		/// </returns>
		Task<IEnumerable<Api>?> GetApiDataAsync(CancellationToken cancellation);
    }
}
