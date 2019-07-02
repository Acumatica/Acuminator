using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using PX.Objects.FA;
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
			VerifyCSharpDiagnosticAsync(
				source: actual, 
				expected: Descriptors.PX1043_SavingChangesInRowPerstistedNonISV.CreateFor(22, 5));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PXDatabaseInsertContainedInMethodInsideRowPersisted.cs")]
		public Task ISV_TestDiagnostic_PXDatabaseInsertContainedInMethodInsideRowPersisted(string actual) =>
			VerifyCSharpDiagnosticAsync(
				source: actual,
				expected: Descriptors.PX1043_SavingChangesInRowPerstistedNonISV.CreateFor(17, 5));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PXDatabaseInsertWithoutTranStatusOpenInsideRowPersisted.cs")]
		public Task ISV_TestDiagnostic_PXDatabaseInsertWithoutTranStatusOpenInsideRowPersisted(string actual) =>
			VerifyCSharpDiagnosticAsync(
				source: actual,
				expected: Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(18, 4));
	}
}
