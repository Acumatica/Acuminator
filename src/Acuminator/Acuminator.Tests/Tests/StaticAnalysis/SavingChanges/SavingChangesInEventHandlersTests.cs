using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ConnectionScopeInRowSelecting;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.SavingChanges
{
	public class SavingChangesInEventHandlersTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(new SavingChangesInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PressSave.cs")]
		public void TestDiagnostic_PressSave(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(14, 4));
		}

		[Theory]
		[EmbeddedFileData(@"EventHandlers\SavePress.cs")]
		public void TestDiagnostic_SavePress(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(14, 4));
		}

		[Theory]
		[EmbeddedFileData(@"EventHandlers\GraphPersist.cs")]
		public void TestDiagnostic_GraphPersist(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(14, 4));
		}

		[Theory]
		[EmbeddedFileData(@"EventHandlers\CachePersist.cs")]
		public void TestDiagnostic_CachePersist(string actual)
		{
			VerifyCSharpDiagnostic(actual, 
				Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(14, 4),
				Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(19, 4),
				Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(24, 4),
				Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(29, 4),
				Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(30, 4),
				Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(31, 4));
		}

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PressSaveInExternalMethod.cs")]
		public void TestDiagnostic_PressSaveInExternalMethod(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(14, 4));
		}

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PressSaveInsideRowPersisting.cs")]
		public void TestDiagnostic_PressSaveInsideRowPersisting(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1043_SavingChangesInRowPerstisting.CreateFor(14, 4));
		}

		[Theory]
		[EmbeddedFileData(@"EventHandlers\CachePersistInsideRowPersisting.cs")]
		public void TestDiagnostic_CachePersistInsideRowPersisting_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"EventHandlers\ArbitraryCodeInsideRowPersisting.cs")]
		public void TestDiagnostic_ArbitraryCodeInsideRowPersisting_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}
	}
}
