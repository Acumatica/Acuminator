#nullable enable

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.GraphCreation
{
	public class PXGraphCreationInDacIsActiveTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default
					.WithStaticAnalysisEnabled()
					.WithSuppressionMechanismDisabled()
					.WithRecursiveAnalysisEnabled(),
				new PXGraphCreationInGraphInWrongPlacesDacAnalyzer());

		[Theory]
		[EmbeddedFileData("DacExtensionWithGraphCreationInIsActiveMethods.cs")]
		public async Task GraphExtension_IsActiveAndIsActiveForGraph(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1056_PXGraphCreationInIsActiveMethod.CreateFor(15, 41),
				Descriptors.PX1056_PXGraphCreationInIsActiveMethod.CreateFor(22, 11),
				Descriptors.PX1056_PXGraphCreationInIsActiveMethod.CreateFor(23, 8));
	}
}