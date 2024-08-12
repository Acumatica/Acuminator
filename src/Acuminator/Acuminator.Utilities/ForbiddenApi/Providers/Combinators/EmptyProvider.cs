#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.ForbiddenApi.Model;

namespace Acuminator.Utilities.ForbiddenApi.Providers
{
	/// <summary>
	/// A data provider that always provides empty data.
	/// </summary>
	internal class EmptyProvider : IApiDataProvider
	{
		private readonly Task<IEnumerable<Api>?> _resultTask;

		/// <inheritdoc/>
		public bool IsDataAvailable { get; }

		public EmptyProvider(bool considerDataAvailable)
        {
			IsDataAvailable = considerDataAvailable;
			_resultTask = IsDataAvailable
				? Task.FromResult<IEnumerable<Api>?>([])
				: Task.FromResult<IEnumerable<Api>?>(null);
		}

		/// <inheritdoc/>
		public Task<IEnumerable<Api>?> GetApiDataAsync(CancellationToken cancellation) => 
			_resultTask;

		public IEnumerable<Api>? GetApiData(CancellationToken cancellation) =>
			IsDataAvailable
				? []
				: null;
	}
}
