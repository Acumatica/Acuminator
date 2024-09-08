#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExternalRunner;

internal class Program
{
	public const string EmptyStringPlaceHolder = "#";

	public const string _code =
		"""
		using System;

		namespace Demo
		{
			public class InfoService
			{
				
			}
		}
		""";

	static async Task<int> Main(string[] args)
	{
		var (expectedAnalysisSettings, expectedBannedApiSettings) = GetExpectedSettings(args);

		var bannedApiAnalyzer = new ReadingFromMemoryAnalyzer(expectedAnalysisSettings, expectedBannedApiSettings);
		var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(bannedApiAnalyzer);

		Document[] documents = DocumentCreator.GetDocuments([_code], LanguageNames.CSharp);
		Document document = documents[0];
		Project project = document.Project;

		var compilationOptions = CreateCompilationWithAnalyzersOptions();

		var compilation = await project.GetCompilationAsync().ConfigureAwait(false);
		var compilationWithAnalyzers = compilation.CheckIfNull().WithAnalyzers(analyzers, compilationOptions);
		var projectDiagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);

		if (projectDiagnostics.Length == 0)
			return 0;

		bool hasErrorDiagnostics = projectDiagnostics.Any(d => d.Id.StartsWith("AD"));
		return hasErrorDiagnostics ? 1 : 0;
	}

	private static (CodeAnalysisSettings AnalysisSettings, BannedApiSettings BannedApiSettings) GetExpectedSettings(string[] args)
	{
		if (args.Length < 6 || args.Length > 8)
			throw new InvalidOperationException("Invalid amount of arguments");

		if (!bool.TryParse(args[0], out bool recursiveAnalysisEnabled))
			throw new InvalidOperationException($"Invalid argument \"{nameof(recursiveAnalysisEnabled)}\"");

		if (!bool.TryParse(args[1], out bool isvSpecificAnalyzersEnabled))
			throw new InvalidOperationException($"Invalid argument \"{nameof(isvSpecificAnalyzersEnabled)}\"");

		if (!bool.TryParse(args[2], out bool staticAnalysisEnabled))
			throw new InvalidOperationException($"Invalid argument \"{nameof(staticAnalysisEnabled)}\"");

		if (!bool.TryParse(args[3], out bool suppressionMechanismEnabled))
			throw new InvalidOperationException($"Invalid argument \"{nameof(suppressionMechanismEnabled)}\"");

		if (!bool.TryParse(args[4], out bool px1007DocumentationDiagnosticEnabled))
			throw new InvalidOperationException($"Invalid argument \"{nameof(px1007DocumentationDiagnosticEnabled)}\"");

		if (!bool.TryParse(args[5], out bool bannedApiAnalysisEnabled))
			throw new InvalidOperationException($"Invalid argument \"{nameof(bannedApiAnalysisEnabled)}\"");

		string? bannedApiFilePath = args.Length > 6
			? args[6]
			: null;

		if (bannedApiFilePath == EmptyStringPlaceHolder)
			bannedApiFilePath = null;

		string? whiteListFilePath = args.Length > 7
			? args[7]
			: null;

		if (whiteListFilePath == EmptyStringPlaceHolder)
			whiteListFilePath = null;

		var analysisSettings = new CodeAnalysisSettings(recursiveAnalysisEnabled, isvSpecificAnalyzersEnabled, staticAnalysisEnabled, 
														suppressionMechanismEnabled, px1007DocumentationDiagnosticEnabled);
		var bannedApiSettings = new BannedApiSettings(bannedApiAnalysisEnabled, bannedApiFilePath, whiteListFilePath);

		return (analysisSettings, bannedApiSettings);
	}

	private static CompilationWithAnalyzersOptions CreateCompilationWithAnalyzersOptions()
	{
		bool isUnderDebug = Debugger.IsAttached;
		return new CompilationWithAnalyzersOptions(options: null, onAnalyzerException: null,
												   concurrentAnalysis: !isUnderDebug, logAnalyzerExecutionTime: true,
												   reportSuppressedDiagnostics: false, analyzerExceptionFilter: null);
	}
}
