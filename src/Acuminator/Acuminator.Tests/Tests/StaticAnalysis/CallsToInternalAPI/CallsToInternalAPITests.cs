using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.CallsToInternalAPI;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

using Resources = Acuminator.Analyzers.Resources;

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
		[EmbeddedFileData("WithoutDescription.cs")]
		public async Task PublicClass_WithoutDescription(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(9, 15, messageArgs: nameof(Resources.PX1007Class).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(13, 23, messageArgs: nameof(Resources.PX1007Delegate).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(15, 16, messageArgs: nameof(Resources.PX1007Struct).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(17, 19, messageArgs: nameof(Resources.PX1007Interface).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(19, 14, messageArgs: nameof(Resources.PX1007Enum).GetLocalized()));		
	}
}
