#nullable enable
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis.BannedApi;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.BannedApi
{
	public class BannedApiAnalysisDisabledTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new BannedApiAnalyzer(customBannedApiStorage: null, customBannedApiDataProvider: null, 
								  customAllowedApiStorage: null, customAllowedApiDataProvider: null,
								  customBanInfoRetriever: null, customAllowedInfoRetriever: null,
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				BannedApiSettings.Default.WithBannedApiAnalysisDisabled());

		[Theory]
		[EmbeddedFileData("CallsToApisForbiddenToIsv.cs")]
		public virtual async Task Calls_To_API_ForbiddenForISV(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("CallsToMathRoundAPIs.cs")]
		public virtual async Task Calls_To_MathRound_API(string source) =>
			await VerifyCSharpDiagnosticAsync(source);
	}
}
