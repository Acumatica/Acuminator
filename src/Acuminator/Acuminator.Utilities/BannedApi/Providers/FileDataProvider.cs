using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.BannedApi.Model;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.BannedApi.Providers
{
	[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "Need to load banned API database")]
	public class FileDataProvider : ApiDataProvider
	{
		private readonly string _filePath;
		private DateTime? _lastFileWriteTimeUtcWhenApiWasRead;

		/// <inheritdoc/>
		public override bool IsDataAvailable => File.Exists(_filePath);

		public FileDataProvider(string filePath)
		{
			_filePath = filePath.CheckIfNullOrWhiteSpace();
		}

		public override bool HasApiDataSourceChanged()
		{
			DateTime? currentLastWriteTimeUtcs = GetLastWriteTimeUtcSafely();
			return _lastFileWriteTimeUtcWhenApiWasRead == currentLastWriteTimeUtcs;
		}

		/// <inheritdoc/>
		public override async Task<IEnumerable<Api>?> GetApiDataAsync(CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			DateTime? currentLastWriteTimeUtcs = GetLastWriteTimeUtcSafely();

			if (currentLastWriteTimeUtcs == null)
			{
				_lastFileWriteTimeUtcWhenApiWasRead = null;
				return null;
			}
			
			string wholeText;

			using (var reader = new StreamReader(_filePath))
			{
				wholeText = await reader.ReadToEndAsync().WithCancellation(cancellation)
														 .ConfigureAwait(false);
			}

			_lastFileWriteTimeUtcWhenApiWasRead = currentLastWriteTimeUtcs;
			cancellation.ThrowIfCancellationRequested();

			if (wholeText.IsNullOrWhiteSpace())
				return [];

			var apis = ParseTextIntoApis(wholeText, cancellation);
			return apis;
		}

		/// <inheritdoc/>
		public override IEnumerable<Api>? GetApiData(CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			if (!IsDataAvailable)
				return null;

			var lines = File.ReadLines(_filePath);
			var bannedApis = ParseLinesIntoApis(lines, cancellation);

			return bannedApis;
		}

		protected override string GetParseErrorMessage(Exception originalException, int lineNumber) =>
			$"An error happened during the reading of the line {lineNumber} from the file \"{_filePath}\"";

		private DateTime? GetLastWriteTimeUtcSafely()
		{
			if (!IsDataAvailable)
				return null;

			try
			{
				return File.GetLastWriteTimeUtc(_filePath);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
