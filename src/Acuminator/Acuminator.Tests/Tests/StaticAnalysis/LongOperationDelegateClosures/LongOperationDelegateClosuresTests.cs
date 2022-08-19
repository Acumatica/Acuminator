using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

using AnalyzerResources = Acuminator.Analyzers.Resources;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationDelegateClosures
{
	public class LongOperationDelegateClosuresTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new LongOperationDelegateClosuresAnalyzer(CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
																				  .WithSuppressionMechanismDisabled()
																				  .WithRecursiveAnalysisEnabled());
		[Theory]
		[EmbeddedFileData("ClosuresInNonGraph.cs")]
		public Task SetProcessDelegate_ReportOnlyCapturedPassedParameters_NonGraphHelper(string actual)
		{
			string[] formatArgsGraph = new[] { AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_ProcessingDelegateFormatArg };
			string[] formatArgsAdapter = new[] { AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, AnalyzerResources.PX1008Title_ProcessingDelegateFormatArg };
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 24, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 25, column: 4, formatArgsAdapter));
		}

		[Theory]
		[EmbeddedFileData("HelperCallsToAnotherFile.cs", "ClosuresInNonGraph.cs")]
		public Task RecursiveAnalysis_CallsTo_NonGraphHelper_FromOtherFile(string actual, string helper)
		{
			string[] formatArgsGraphAndProcDelegate = 
				new[] { AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_ProcessingDelegateFormatArg };
			string[] formatArgsGraphAndLongRunDelegate =
				new[] { AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg };
			string[] formatArgsAdapterAndProcDelegate =
				new[] { AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, AnalyzerResources.PX1008Title_ProcessingDelegateFormatArg };
			return VerifyCSharpDiagnosticAsync(actual, helper,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 19, column: 4, formatArgsGraphAndProcDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 20, column: 4, formatArgsAdapterAndProcDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 22, column: 4, formatArgsGraphAndLongRunDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 24, column: 4, formatArgsGraphAndLongRunDelegate));
		}

		[Theory]
		[EmbeddedFileData("SetProcessDelegateClosures.cs")]
		public Task SetProcessDelegates_GraphCapturedInClosures(string actual)
		{
			string[] formatArgs = new[] { AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_ProcessingDelegateFormatArg };
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 25, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 36, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 39, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 46, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 47, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 51, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 52, column: 4, formatArgs));
		}

		[Theory]
		[EmbeddedFileData("LongRunDelegateClosures_NormalCases.cs")]
		public Task LongRunDelegates_GraphAndAdapterCaptured_NormalCases(string actual)
		{
			string[] formatArgsGraph = new[] { AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg };
			string[] formatArgsAdapter = new[] { AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg };
			return VerifyCSharpDiagnosticAsync(actual,

				// graph capture
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 37, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 40, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 41, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 52, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 59, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 60, column: 4, formatArgsGraph),

				//recursive analysis graph capture
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 65, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 70, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 71, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 72, column: 4, formatArgsGraph),

				// Test capturing graph via member access
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 80, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 86, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 89, column: 4, formatArgsGraph),

				// adapter capture
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 125, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 128, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 129, column: 4, formatArgsAdapter),

				//recursive analysis adapter capture
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 141, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 143, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 144, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 145, column: 4, formatArgsAdapter));
		}

		[Theory]
		[EmbeddedFileData("LongRunDelegateClosures_ComplexMapping.cs")]
		public Task LongRunDelegates_GraphCaptured_ComplexMappingOfArgumentsToParameters(string actual)
		{
			string[] formatArgs = new[] { AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg };
			return VerifyCSharpDiagnosticAsync(actual,
				//Params check
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 27, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 34, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 35, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 36, column: 4, formatArgs),

				// Named parameters check - names in position
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 40, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 42, column: 4, formatArgs),

				// Named parameters check - names out of position
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 47, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 48, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 49, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 51, column: 4, formatArgs),

				// Named parameters check - optional parameters
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 54, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 59, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 61, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 62, column: 4, formatArgs));
		}

		[Theory]
		[EmbeddedFileData("CustomView.cs")]
		public Task CustomView_LongRunCapture_GraphAndAdapter(string actual)
		{
			string[] formatArgsGraph = new[] { AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg };
			string[] formatArgsAdapter = new[] { AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg };
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 42, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 43, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 49, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 50, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 52, column: 4, formatArgsAdapter));
		}

		[Theory]
		[EmbeddedFileData("CustomAttribute.cs")]
		public Task CustomAttribute_LongRunCapture_Adapter(string actual)
		{
			string[] formatArgsAdapter = new[] { AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg };
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 33, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 35, column: 4, formatArgsAdapter));
		}

		[Theory]
		[EmbeddedFileData("LongRunDelegateClosures_Reassign.cs")]
		public Task Adapter_Reassigned_InLongRun(string actual)
		{
			string[] formatArgs = new[] { AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg };
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 39, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 40, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 41, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 42, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 43, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 47, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 50, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 51, column: 4, formatArgs));
		}

		[Theory]
		[EmbeddedFileData("LongRunDelegateClosures_Local.cs")]
		public Task AdapterCaptured_InLocalFunctions_InLongRun(string actual)
		{
			string[] formatArgs = new[] { AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg };
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 25, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 26, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 27, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 28, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 35, column: 5, formatArgs));
		}
	}
}
