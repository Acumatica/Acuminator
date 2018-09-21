using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreateInstance
{
	public class PXGraphCreateInstanceInEventHandlersTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(CodeAnalysisSettings.Default.WithRecursiveAnalysisEnabled(),
				new PXGraphCreateInstanceInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\CreateInstance.cs")]
		public void TestDiagnostic_CreateInstance(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers.CreateFor(16, 21));
		}

		[Theory]
		[EmbeddedFileData(@"EventHandlers\Constructor.cs")]
		public void TestDiagnostic_Constructor(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers.CreateFor(16, 21));
		}

		[Theory]
		[EmbeddedFileData(@"EventHandlers\ConstructorForNonSpecificPXGraph.cs")]
		public void TestDiagnostic_ConstructorForNonSpecificPXGraph(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers.CreateFor(16, 16));
		}
	}
}
