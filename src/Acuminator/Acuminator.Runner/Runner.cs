using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CommandLine;

using Serilog;
using Serilog.Events;

using Acuminator.Runner.Analysis;
using Acuminator.Runner.Analysis.CodeSources;
using Acuminator.Runner.Input;
using Acuminator.Runner.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Runner.Resources;

namespace Acuminator.Runner
{
	internal class Runner
	{
		public static async Task<int> Main(string[] args)
		{
			Console.WriteLine(Resources.Messages.WelcomeMessage);

			try
			{
				ParserResult<CommandLineOptions> argsParsingResult = Parser.Default.ParseArguments<CommandLineOptions>(args);
				RunResult runResult = await argsParsingResult.MapResult(parsedFunc: RunValidationWithParsedOptionsAsync,
																		notParsedFunc: OnParsingErrorsAsync);
				return runResult.ToExitCode();
			}
			catch (Exception e)
			{
				if (Log.Logger != null)
					Log.Error(e, "An unhandled runtime error was encountered during the validation.");
				else
					Console.WriteLine($"ERROR: An unhandled runtime error was encountered during the validation. Details:{Environment.NewLine}{e}");

				return RunResult.RunTimeError.ToExitCode();
			}
		}

		private static Task<RunResult> RunValidationWithParsedOptionsAsync(CommandLineOptions commandLineOptions)
		{
			var logger = TryInitalizeLogger(commandLineOptions);

			if (logger == null)
				return Task.FromResult(RunResult.RunTimeError);

			Log.Logger = logger;

			if (commandLineOptions.UseNonInteractiveMode)
			{
				return RunValidation(CancellationToken.None);
			}
			else
			{
				using var consoleCancellationSubscription = new ConsoleCancellationSubscription(logger);
				return RunValidation(consoleCancellationSubscription.CancellationToken);
			}
			
			//------------------------------------------Local Function-------------------------------------------
			async Task<RunResult> RunValidation(CancellationToken cancellation)
			{
				AnalysisContext? analysisContext = CreateAnalysisContextFromCommandLineArguments(commandLineOptions);

				if (analysisContext == null)
					return RunResult.RunTimeError;

				var analyzer = new SolutionAnalysisRunner(logger);
				var analysisResult = await analyzer.RunAnalysisAsync(analysisContext, cancellation);

				OutputValidationResult(logger, analysisResult, analysisContext);
				return analysisResult;
			}
		}

		private static ILogger? TryInitalizeLogger(CommandLineOptions commandLineOptions)
		{
			try
			{
				var loggerConfiguration = new LoggerConfiguration().WriteTo.Console(formatProvider: CultureInfo.InvariantCulture);
				LogEventLevel logLevel = LogEventLevel.Information;

				if (!commandLineOptions.Verbosity.IsNullOrWhiteSpace() &&
					!Enum.TryParse(commandLineOptions.Verbosity, ignoreCase: true, out logLevel))
				{
					Console.WriteLine($"ERROR: The logger verbosity value \"{commandLineOptions.Verbosity}\" is not supported. " +
									  "Use help to see the list of allowed verbosity values.");
					return null;
				}

				loggerConfiguration = loggerConfiguration.MinimumLevel.Is(logLevel)
														 .Enrich.FromLogContext();
				var logger = loggerConfiguration.CreateLogger();
				return logger;
			}
			catch (Exception e)
			{
				Console.WriteLine($"ERROR: Unhandled error during the initialization of the logger. Details:{Environment.NewLine}{e}");   // Log failed serilog initialization directly to console
				return null;
			}
		}

		private static AnalysisContext? CreateAnalysisContextFromCommandLineArguments(CommandLineOptions commandLineOptions)
		{
			try
			{
				AnalysisContextBuilder analysisContextBuilder = new AnalysisContextBuilder();
				AnalysisContext analysisContext = analysisContextBuilder.CreateContext(commandLineOptions);
				return analysisContext;
			}
			catch (Exception e)
			{
				Log.Error(e, "An error happened during the processing of input command line arguments and initialization of analysis context.");
				return null;
			}
		}

		private static Task<RunResult> OnParsingErrorsAsync(IEnumerable<Error> parsingErrors)
		{
			foreach (var error in parsingErrors)
			{
				Log.Error("Parsing error type: {ErrorType}.", error.Tag);
			}

			return Task.FromResult(RunResult.RunTimeError);
		}

		[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", 
						 Justification = "Resource strings are used to simplify review by Doc Team")]
		private static void OutputValidationResult(ILogger logger, RunResult runResult, AnalysisContext analysisContext)
		{
			switch (runResult)
			{
				case RunResult.Success:
					logger.Information(Messages.ValidationPassedSuccessfullyMessage);
					return;
				case RunResult.RequirementsNotMet:
					logger.Error(Messages.CodeSourceFailedValidationMessage, analysisContext.CodeSource.Location);
					return;
				case RunResult.Cancelled:
					logger.Warning(Messages.CodeSourceValidationWasCancelled, analysisContext.CodeSource.Location);
					return;
				case RunResult.RunTimeError:
					logger.Error(Messages.RuntimeErrorHappenedDuringValidationMessage);
					return;
			}
		}
	}
}
