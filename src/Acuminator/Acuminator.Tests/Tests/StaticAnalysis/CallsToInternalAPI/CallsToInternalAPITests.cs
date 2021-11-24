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
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(15, 27),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(17, 19),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(25, 88),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(25, 130),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(27, 47),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(30, 33),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(30, 76),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(30, 112),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(32, 41),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(35, 48),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(37, 48),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(40, 24),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(41, 24),
				Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV.CreateFor(43, 43));
	}
}
