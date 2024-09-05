#nullable enable

using System.IO;
using System.Runtime.CompilerServices;
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
	public class BannedApiTestsIsvWithExternalFiles : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new BannedApiAnalyzer(customBannedApiStorage: null, customBannedApiDataProvider: null, 
								  customWhiteListStorage: null, customWhiteListDataProvider: null,
								  customBanInfoRetriever: null, customWhiteListInfoRetriever: null,
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled()
											.WithIsvSpecificAnalyzersEnabled(),
				BannedApiSettings.Default.WithBannedApiAnalysisEnabled()
										 .WithBannedApiFilePath(
											BannedApiFilePath())
										 .WithWhiteListApiFilePath(
											WhiteListFilePath())
				);

		private static string BannedApiFilePath([CallerFilePath] string? testFilePath = null)
		{
			string directory	 = Path.GetDirectoryName(testFilePath);
			string bannedApiFile = Path.Combine(directory, "BannedData", "CustomBannedApis.txt");
			return bannedApiFile;
		}

		private static string WhiteListFilePath([CallerFilePath] string? testFilePath = null)
		{
			string directory	 = Path.GetDirectoryName(testFilePath);
			string whiteListFile = Path.Combine(directory, "BannedData", "CustomWhiteList.txt");
			return whiteListFile;
		}

		[Theory]
		[EmbeddedFileData("CallsToApisForbiddenToIsv.cs")]
		public virtual async Task Calls_To_API_ForbiddenForISV(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(13, 22, Resources.PX1099Title_TypeFormatArg, "System.Reflection.MethodInfo",
					"Reflection usage is forbidden in Acumatica customizations."),
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(13, 57, Resources.PX1099Title_TypeFormatArg, "System.Reflection.MethodInfo",
					"Reflection usage is forbidden in Acumatica customizations."),
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(18, 10, Resources.PX1099Title_TypeFormatArg, "System.Reflection.MethodInfo",
					"Reflection usage is forbidden in Acumatica customizations."),
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(20, 4, Resources.PX1099Title_TypeFormatArg, "System.Reflection.MethodInfo",
					"Reflection usage is forbidden in Acumatica customizations."),
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(26, 30, Resources.PX1099Title_TypeFormatArg, "System.Environment",
					"Environment usage is forbidden in Acumatica customizations."),
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(26, 107, Resources.PX1099Title_TypeFormatArg, "System.Environment",
					"Environment usage is forbidden in Acumatica customizations."));

		[Theory]
		[EmbeddedFileData("CallsToMathRoundAPIs.cs")]
		public virtual async Task Calls_To_MathRound_API(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(15, 13, Resources.PX1099Title_MethodFormatArg, "System.Math.Round(System.Decimal,System.Int32)",
					"Math.Round uses Bankers Rounding by default, which rounds to the closest even number. Usually, this is not the desired rounding behavior. Use Math.Round overload with MidpointRounding parameter to explicitly specify the desired rounding behavior."),
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(24, 18, Resources.PX1099Title_MethodFormatArg, "System.Math.Round(System.Decimal)",
					"Math.Round uses Bankers Rounding by default, which rounds to the closest even number. Usually, this is not the desired rounding behavior. Use Math.Round overload with MidpointRounding parameter to explicitly specify the desired rounding behavior."));
	}
}
