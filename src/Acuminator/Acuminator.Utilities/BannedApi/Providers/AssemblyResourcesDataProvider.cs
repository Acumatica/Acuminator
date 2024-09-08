#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.BannedApi.Errors;
using Acuminator.Utilities.BannedApi.Model;

namespace Acuminator.Utilities.BannedApi.Providers
{
	public class AssemblyResourcesDataProvider : ApiDataProvider
	{
		private readonly Assembly _assembly;
		private readonly string _apiResourceName;
		private bool? _isDataAvailable;

		/// <inheritdoc/>
		public override bool IsDataAvailable
		{
			get 
			{
				if (_isDataAvailable.HasValue)
					return _isDataAvailable.Value;

				var resourceInfo = _assembly.GetManifestResourceInfo(_apiResourceName);
				_isDataAvailable = resourceInfo != null;

				return _isDataAvailable.Value;
			}
		}

		public AssemblyResourcesDataProvider(Assembly assembly, string apiResourceName)
        {
			_assembly = assembly.CheckIfNull();
			_apiResourceName = apiResourceName.CheckIfNullOrWhiteSpace();
		}

		/// <inheritdoc/>
		public override async Task<IEnumerable<Api>?> GetApiDataAsync(CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			if (!IsDataAvailable)
				return null;

			string wholeText;

			using (var resourceStream = _assembly.GetManifestResourceStream(_apiResourceName))
			{
				if (resourceStream == null)
					throw new ApiReaderException($"Can't find the source text with Resource ID \"{_apiResourceName}\".");

				using (var reader = new StreamReader(resourceStream))
				{
					wholeText = await reader.ReadToEndAsync().WithCancellation(cancellation)
															 .ConfigureAwait(false);
				}
			}

			cancellation.ThrowIfCancellationRequested();

			if (wholeText.IsNullOrWhiteSpace())
				return [];

			var bannedApis = ParseTextIntoApis(wholeText, cancellation);
			return bannedApis;
		}

		/// <inheritdoc/>
		public override IEnumerable<Api>? GetApiData(CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			if (!IsDataAvailable)
				return null;

			using (var resourceStream = _assembly.GetManifestResourceStream(_apiResourceName))
			{
				if (resourceStream == null)
					throw new ApiReaderException($"Can't find the source text with Resource ID \"{_apiResourceName}\".");

				return ParseStreamIntoApis(resourceStream, cancellation).ToList();
			}
		}

		public override bool HasApiDataSourceChanged() => false;

		protected override string GetParseErrorMessage(Exception originalException, int lineNumber)
		{
			var assemblyName = _assembly.GetName().Name;
			return $"An error happened during the reading of the line {lineNumber} from the" + 
				   $" resource \"{_apiResourceName}\" of the assembly \"{assemblyName}\"";
		}
	}
}
