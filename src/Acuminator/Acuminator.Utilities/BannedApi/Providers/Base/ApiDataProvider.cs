#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.BannedApi.Model;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.BannedApi.Providers
{
    /// <summary>
    /// An API data provider base class to share some common logic.
    /// </summary>
    public abstract class ApiDataProvider : IApiDataProvider
	{
		/// <inheritdoc/>
		public abstract bool IsDataAvailable { get; }

		/// <inheritdoc/>
		public abstract IEnumerable<Api>? GetApiData(CancellationToken cancellation);

		/// <inheritdoc/>
		public abstract Task<IEnumerable<Api>?> GetApiDataAsync(CancellationToken cancellation);

		protected IEnumerable<Api> ParseTextIntoApis(string text, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			int lineNumber = 1;
			using StringReader wholeTextReader = new StringReader(text);

			while (wholeTextReader.ReadLine() is string rawDocID)
			{
				cancellation.ThrowIfCancellationRequested();

				Api? api = ParseApiFromLine(rawDocID, lineNumber);

				if (api != null)
					yield return api;

				lineNumber++;
			}
		}

		protected IEnumerable<Api> ParseLinesIntoApis(IEnumerable<string> rawLines, CancellationToken cancellation)
		{
			int lineNumber = 1;

			foreach (string rawDocID in rawLines)
			{
				cancellation.ThrowIfCancellationRequested();

				Api? api = ParseApiFromLine(rawDocID, lineNumber);

				if (api != null)
					yield return api;

				lineNumber++;
			}
		}

		protected IEnumerable<Api> ParseStreamIntoApis(Stream resourceStream, CancellationToken cancellation)
		{
			int lineNumber = 1;
			using var reader = new StreamReader(resourceStream);

			while (reader.ReadLine() is string rawDocID)
			{
				cancellation.ThrowIfCancellationRequested();

				Api? api = ParseApiFromLine(rawDocID, lineNumber);
				
				if (api != null)
					yield return api;

				lineNumber++;
			}
		}

		protected virtual Api? ParseApiFromLine(string rawDocID, int lineNumber)
		{
			if (rawDocID.IsNullOrWhiteSpace())
				return null;
			try
			{
				return new Api(rawDocID);
			}
			catch (Exception exception)
			{
				string errorMessage = GetParseErrorMessage(exception, lineNumber);
				throw new Errors.ApiReaderException(errorMessage, exception);
			}
		}

		protected abstract string GetParseErrorMessage(Exception originalException, int lineNumber);
	}
}
