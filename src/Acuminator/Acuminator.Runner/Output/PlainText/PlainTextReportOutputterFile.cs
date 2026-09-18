using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using Acuminator.Runner.Input;
using Acuminator.Runner.Output.Data;
using Acuminator.Runner.Resources;
using Acuminator.Utilities.Common;

using Serilog;

using static Acuminator.Runner.Constants.Constant.Output;

namespace Acuminator.Runner.Output.PlainText
{
	/// <summary>
	/// TThe report outputter to file in the plain text format.
	/// </summary>
	internal class PlainTextReportOutputterFile : PlainTextReportOutputterBase
	{
		private readonly StreamWriter _streamWriter;
		private bool _disposed;

		public PlainTextReportOutputterFile(string outputFile)
		{
			DeleteExistingFile(outputFile.CheckIfNullOrWhiteSpace());

			_streamWriter = GetStreamWriter(outputFile);
		}

		public sealed override void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				_streamWriter.Dispose();
			}
		}

		public sealed override void OutputReport(CodeSourceReport codeSourceReport, AnalysisContext analysisContext, CancellationToken cancellation)
		{
			if (_disposed)
				throw new ObjectDisposedException(objectName: GetType().FullName);

			base.OutputReport(codeSourceReport, analysisContext, cancellation);
		}

		public sealed override void OutputReport(ProjectReport projectReport, AnalysisContext analysisContext, CancellationToken cancellation)
		{
			if (_disposed)
				throw new ObjectDisposedException(objectName: GetType().FullName);
			else if (analysisContext.OutputFileName.IsNullOrWhiteSpace())
				return;

			base.OutputReport(projectReport, analysisContext, cancellation);
		}

		protected override void WriteCodeSourceTitle(string codeSourceTitle) => 
			WriteLine(codeSourceTitle);
		
		protected override void WriteProjectTitle(string projectTitle) => 
			WriteLine(projectTitle);

		protected override void WriteTitle(in Title? title, int indentationLevel, int diagnosticsCount, int? distinctDiagnosticsCount, bool hasContent)
		{
			if (title == null)
				return;

			string padding = GetPadding(indentationLevel);
			string suffix  = hasContent ? ":" : string.Empty;
			string titleWithPadding = distinctDiagnosticsCount.HasValue
				? $"{padding}{title.Value.Text}({Messages.ErrorsCountReportTitlePart}: {diagnosticsCount}, " + 
						$"{Messages.DistinctDiagnosticsReportTitlePart}: {distinctDiagnosticsCount.Value}){suffix}"
				: $"{padding}{title.Value.Text}({Messages.ErrorsCountReportTitlePart}: {diagnosticsCount}){suffix}";

			WriteLine(titleWithPadding);
		}

		protected override void WriteLine(in Line line, int indentationLevel)
		{
			if (line.Spans.IsDefaultOrEmpty)
			{
				WriteLine();
				return;
			}

			string padding = GetPadding(indentationLevel);
			if (line.Spans.Length == 3)
			{
				var (diagnosticId, diagnosticMessage, location) =
					(line.Spans[0].ToString(), line.Spans[1].ToString(), line.Spans[2].ToString());
				WriteLine($"{padding}{diagnosticId}{LinePartsSeparator}{diagnosticMessage}{LinePartsSeparator}{location}");
			}
			else if(line.Spans.Length == 4)
			{ 
				var (severity, diagnosticId, diagnosticMessage, location) = 
					(line.Spans[0].ToString(), line.Spans[1].ToString(), line.Spans[2].ToString(), line.Spans[3].ToString());
				WriteLine($"{padding}{string.Format(SeverityTemplate, severity)}{diagnosticId}{LinePartsSeparator}{diagnosticMessage}{LinePartsSeparator}{location}");
				return;			
			}
			else
			{
				WriteLine(padding + line.ToString());
			}
		}

		protected override void WriteLine() => _streamWriter?.WriteLine();

		protected override void WriteLine(string text)
		{
			if (text.IsNullOrWhiteSpace())
				_streamWriter?.WriteLine();
			else
				_streamWriter?.WriteLine(text);
		}

		private void DeleteExistingFile(string outputFile)
		{
			try
			{
				if (File.Exists(outputFile))
					File.Delete(outputFile);
			}
			catch (Exception e)
			{
				Log.Error(e, "Failed to delete the existing output file {OutputFile}", outputFile);
				throw;
			}
		}

		private StreamWriter GetStreamWriter(string outputFile)
		{
			try
			{
				FileStream outputFileStream = File.OpenWrite(outputFile);
				return new StreamWriter(outputFileStream);
			}
			catch (Exception e)
			{
				Log.Error(e, "Failed to open the output file {OutputFile}", outputFile);
				throw;
			}
		}
	}
}