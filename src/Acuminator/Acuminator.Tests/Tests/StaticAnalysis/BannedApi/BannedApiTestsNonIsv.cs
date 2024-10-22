#nullable enable
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.BannedApi;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

using Resources = Acuminator.Analyzers.Resources;

namespace Acuminator.Tests.Tests.StaticAnalysis.BannedApi
{
	public class BannedApiTestsDefaultFileNonIsv : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new BannedApiAnalyzer(customBannedApiStorage: null, customBannedApiDataProvider: null, 
								  customAllowedApiStorage: null, customAllowedApiDataProvider: null,
								  customBanInfoRetriever: null, customAllowedInfoRetriever: null,
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled()
											.WithIsvSpecificAnalyzersDisabled(),
				BannedApiSettings.Default.WithBannedApiAnalysisEnabled()
				);

		[Theory]
		[EmbeddedFileData("CallsToApisForbiddenToIsv.cs")]
		public virtual async Task Calls_To_API_ForbiddenForISV(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("CallsToMathRoundAPIs.cs")]
		public virtual async Task Calls_To_MathRound_API(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(15, 13, Resources.PX1099Title_MethodFormatArg, "System.Math.Round(decimal, int)",
					"Math.Round uses Banker's Rounding by default, which rounds to the closest even number." + 
					" Usually, this is not the desired rounding behavior. Use the Math.Round overload with the MidpointRounding parameter to explicitly specify the desired rounding behavior."),
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(24, 18, Resources.PX1099Title_MethodFormatArg, "System.Math.Round(decimal)",
					"Math.Round uses Banker's Rounding by default, which rounds to the closest even number." + 
					" Usually, this is not the desired rounding behavior. Use the Math.Round overload with the MidpointRounding parameter to explicitly specify the desired rounding behavior."),
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(34, 13, Resources.PX1099Title_MethodFormatArg, "System.Math.Round(double, int)",
					"Math.Round uses Banker's Rounding by default, which rounds to the closest even number." + 
					" Usually, this is not the desired rounding behavior. Use the Math.Round overload with the MidpointRounding parameter to explicitly specify the desired rounding behavior."),
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(43, 18, Resources.PX1099Title_MethodFormatArg, "System.Math.Round(double)",
					"Math.Round uses Banker's Rounding by default, which rounds to the closest even number." + 
					" Usually, this is not the desired rounding behavior. Use the Math.Round overload with the MidpointRounding parameter to explicitly specify the desired rounding behavior."));

		[Theory]
		[EmbeddedFileData("CallsToAsyncAPIs.cs")]
		public async virtual Task Calls_To_Async_APIs(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(12, 41, Resources.PX1099Title_MethodFormatArg,
					"System.Linq.ParallelEnumerable.AsParallel<TSource>(System.Collections.Generic.IEnumerable<TSource>)",
					"Usage of Parallel LINQ is forbidden in Acumatica because it may lead to deadlocks and a loss of Acumatica synchronization context." + 
					" It also prevents Acumatica request profiler and resource governor from correctly managing system resources."),

				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(14, 13, Resources.PX1099Title_TypeFormatArg,
					"System.Threading.Tasks.Parallel",
					"Usage of the Parallel type is forbidden in Acumatica because it may lead to deadlocks and a loss of Acumatica synchronization context." + 
					" It also prevents Acumatica request profiler and resource governor from correctly managing system resources."),

				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(16, 30, Resources.PX1099Title_MethodFormatArg,
					"System.Threading.Tasks.Task.Run<TResult>(System.Func<TResult>)",
					"Usage of the Task.Run method is forbidden in Acumatica because it may lead to deadlocks and a loss of Acumatica synchronization context." +
					" It also prevents Acumatica request profiler and resource governor from correctly managing system resources."),

				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(17, 35, Resources.PX1099Title_MethodFormatArg,
					"System.Threading.Tasks.Task<TResult>.ConfigureAwait(bool)",
					"Usage of the Task.ConfigureAwait method is forbidden in Acumatica because it may lead to deadlocks and a loss of Acumatica synchronization context." + 
					" It also prevents Acumatica request profiler and resource governor from correctly managing system resources."),

				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(19, 33, Resources.PX1099Title_MethodFormatArg, 
					"System.Threading.Tasks.Task.Run(System.Action)",
					"Usage of the Task.Run method is forbidden in Acumatica because it may lead to deadlocks and a loss of Acumatica synchronization context." +
					" It also prevents Acumatica request profiler and resource governor from correctly managing system resources."),

				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(20, 28, Resources.PX1099Title_MethodFormatArg,
					"System.Threading.Tasks.Task.ConfigureAwait(bool)",
					"Usage of the Task.ConfigureAwait method is forbidden in Acumatica because it may lead to deadlocks and a loss of Acumatica synchronization context." +
					" It also prevents Acumatica request profiler and resource governor from correctly managing system resources."),

				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(23, 20, Resources.PX1099Title_MethodFormatArg,
					"System.Threading.Tasks.Task.Start()",
					"Usage of the Task.Start method is forbidden in Acumatica because it may lead to deadlocks and a loss of Acumatica synchronization context." + 
					" It also prevents Acumatica request profiler and resource governor from correctly managing system resources."),

				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(25, 26, Resources.PX1099Title_MethodFormatArg,
					"System.Threading.Tasks.Task<TResult>.ConfigureAwait(bool)",
					"Usage of the Task.ConfigureAwait method is forbidden in Acumatica because it may lead to deadlocks and a loss of Acumatica synchronization context." +
					" It also prevents Acumatica request profiler and resource governor from correctly managing system resources."),

				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(28, 23, Resources.PX1099Title_MethodFormatArg,
					"System.Threading.Tasks.Task.Start(System.Threading.Tasks.TaskScheduler)",
					"Usage of the Task.Start method is forbidden in Acumatica because it may lead to deadlocks and a loss of Acumatica synchronization context." +
					" It also prevents Acumatica request profiler and resource governor from correctly managing system resources."),

				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(30, 29, Resources.PX1099Title_MethodFormatArg,
					"System.Threading.Tasks.Task.ConfigureAwait(bool)",
					"Usage of the Task.ConfigureAwait method is forbidden in Acumatica because it may lead to deadlocks and a loss of Acumatica synchronization context." +
					" It also prevents Acumatica request profiler and resource governor from correctly managing system resources.")
				);
	}
}
