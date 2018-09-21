using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DatabaseQueries;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DatabaseQueries
{
	public class DatabaseQueriesInRowSelectedTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new EventHandlerAnalyzer(CodeAnalysisSettings.Default
					.WithRecursiveAnalysisEnabled()
					.WithIsvSpecificAnalyzersEnabled(),
				new DatabaseQueriesInRowSelectedAnalyzer());

		[Theory]
		[EmbeddedFileData(@"RowSelected\BQLSelect.cs")]
		public void BQLSelect(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1049_DatabaseQueriesInRowSelected.CreateFor(19, 9));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelected\GenericEventHandlerSignature.cs")]
		public void GenericEventHandlerSignature(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1049_DatabaseQueriesInRowSelected.CreateFor(19, 9));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelected\DataView.cs")]
		public void DataView(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1049_DatabaseQueriesInRowSelected.CreateFor(32, 9));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelected\PXView.cs")]
		public void PXView(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1049_DatabaseQueriesInRowSelected.CreateFor(32, 9));
		}
		
		[Theory]
		[EmbeddedFileData(@"RowSelected\BQLSearch.cs")]
		public void BQLSearch(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1049_DatabaseQueriesInRowSelected.CreateFor(30, 9));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelected\PXSelector.cs")]
		public void PXSelector(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1049_DatabaseQueriesInRowSelected.CreateFor(20, 6));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelected\PXDatabase.cs")]
		public void PXDatabase(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1049_DatabaseQueriesInRowSelected.CreateFor(19, 9));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelected\ExternalMethod.cs")]
		public void ExternalMethod(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1049_DatabaseQueriesInRowSelected.CreateFor(18, 23));
		}

		// TODO: Enable this test after migration to Roslyn v2
		[Theory(Skip = "IOperation feature (Operation Actions) is experimental in Roslyn v1")]
		[EmbeddedFileData(@"RowSelected\Lambda.cs")]
		public void Lambda(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1049_DatabaseQueriesInRowSelected.CreateFor(14, 66));
		}

		// TODO: Enable this test after migration to Roslyn v2
		[Theory(Skip = "IOperation feature (Operation Actions) is experimental in Roslyn v1")]
		[EmbeddedFileData(@"RowSelected\LambdaWithBody.cs")]
		public void LambdaWithBody(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1049_DatabaseQueriesInRowSelected.CreateFor(18, 6));
		}
	}
}
