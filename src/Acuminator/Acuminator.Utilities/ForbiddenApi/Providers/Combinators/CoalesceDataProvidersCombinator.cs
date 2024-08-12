#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.ForbiddenApi.Model;

namespace Acuminator.Utilities.ForbiddenApi.Providers
{
	/// <summary>
	/// A data providers coalesce combinator. Gets data sequentially from a list of providers until the first successful retrieval.
	/// </summary>
	public class DataProvidersCoalesceCombinator : IApiDataProvider
	{
		/// <summary>
		/// The providers to be combined. 
		/// Providers are ordered in the order they are passed to the combinator provider.
		/// </summary>
		private readonly IEnumerable<IApiDataProvider> _providers;

		/// <inheritdoc/>
		public bool IsDataAvailable => _providers.Any(p => p.IsDataAvailable);

		public DataProvidersCoalesceCombinator(IEnumerable<IApiDataProvider> providers)
        {
			_providers = providers.CheckIfNull();
        }

		/// <inheritdoc/>
		public async Task<IEnumerable<Api>?> GetApiDataAsync(CancellationToken cancellation)
		{
            foreach (var provider in _providers)
            {
				cancellation.ThrowIfCancellationRequested();

				if (!provider.IsDataAvailable) 
					continue;

				var bannedApiData = await provider.GetApiDataAsync(cancellation)
												  .WithCancellation(cancellation)
												  .ConfigureAwait(false);

				cancellation.ThrowIfCancellationRequested();

				if (bannedApiData != null)
					return bannedApiData;
            }

			return null;
		}

		public IEnumerable<Api>? GetApiData(CancellationToken cancellation)
		{
			foreach (var provider in _providers)
			{
				cancellation.ThrowIfCancellationRequested();

				if (!provider.IsDataAvailable)
					continue;

				var bannedApiData = provider.GetApiData(cancellation);

				cancellation.ThrowIfCancellationRequested();

				if (bannedApiData != null)
					return bannedApiData;
			}

			return null;
		}
	}
}
