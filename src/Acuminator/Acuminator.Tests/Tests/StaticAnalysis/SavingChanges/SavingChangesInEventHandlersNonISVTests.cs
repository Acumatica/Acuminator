using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.SavingChanges
{
	public class SavingChangesInEventHandlersNonISVTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(CodeAnalysisSettings.Default
					.WithRecursiveAnalysisEnabled()
					.WithIsvSpecificAnalyzersDisabled(),
				new SavingChangesInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PXDatabaseInsertInsideRowPersisted.cs")]
		public Task ISV_TestDiagnostic_PXDatabaseInsertInsideRowPersisting(string actual) => 
			VerifyCSharpDiagnosticAsync(source: actual);

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PXDatabaseInsertContainedInMethodInsideRowPersisted.cs")]
		public Task ISV_TestDiagnostic_PXDatabaseInsertContainedInMethodInsideRowPersisted(string actual) =>
			VerifyCSharpDiagnosticAsync(source: actual);

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PXDatabaseInsertWithoutTranStatusOpenInsideRowPersisted.cs")]
		public Task ISV_TestDiagnostic_PXDatabaseInsertWithoutTranStatusOpenInsideRowPersisted(string actual) =>
			VerifyCSharpDiagnosticAsync(
				source: actual,
				expected: Descriptors.PX1043_SavingChangesInRowPerstistedNonISV.CreateFor(16, 4));
	}
}
