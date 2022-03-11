using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationDelegateClosures
{
    public class LongOperationDelegateClosuresTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new LongOperationDelegateClosuresAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled());

		[Theory]
        [EmbeddedFileData("SetProcessDelegateClosures.cs")]
        public Task SetProcessDelegates_GraphCapturedInClosures(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 25, column: 4),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 36, column: 4),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 39, column: 4),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 46, column: 4),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 47, column: 4),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 51, column: 4),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 52, column: 4));

		[Theory]
		[EmbeddedFileData("LongRunDelegateClosures.cs")]
		public Task LongRunDelegates_GraphCapturedInClosures(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 36, column: 4),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 39, column: 4),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 40, column: 4),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 51, column: 4),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 58, column: 4),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 59, column: 4));
	}
}
