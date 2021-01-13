using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.CallsToInternalAPI;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.CallsToInternalAPI
{
	public class CallsToInternalAPITests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new CallsToInternalAPIAnalyzer(
					CodeAnalysisSettings.Default
										.WithStaticAnalysisEnabled()
										.WithSuppressionMechanismDisabled()
										.WithIsvSpecificAnalyzersEnabled());

		[Theory]
		[EmbeddedFileData("InternalAPI.cs")]
		public async Task Field_WithInitializer(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(39, 10),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(39, 40));

		[Theory]
		[EmbeddedFileData("SOShipmentExt.cs", "InternalAPI.cs")]
		public async Task CallsToInternal_Properties_Methods_Fields(string source, string internalApiDeclaration) =>
			await VerifyCSharpDiagnosticAsync(
				source, internalApiDeclaration,
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(13, 27),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(23, 88),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(23, 130),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(25, 47),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(28, 33),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(28, 76),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(28, 112),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(30, 41),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(33, 43));
	}
}
