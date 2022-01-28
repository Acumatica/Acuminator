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
		public async Task FieldInitializers_Constructors_BaseTypes(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(20, 32),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(31, 31),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(60, 19),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(60, 49),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(61, 19),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(67, 25));

		[Theory]
		[EmbeddedFileData("GraphUsingInternalApi.cs", "InternalAPI.cs")]
		public async Task InsideGraph_Properties_Methods_Fields_Events(string source, string internalApiDeclaration) =>
			await VerifyCSharpDiagnosticAsync(
				source, internalApiDeclaration,
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(5, 79),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(15, 18),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(17, 19),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(25, 79),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(25, 121),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(27, 35),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(30, 24),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(30, 67),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(30, 103),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(32, 29),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(35, 39),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(37, 36),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(40, 15),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(41, 12),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(43, 34));
	}
}
