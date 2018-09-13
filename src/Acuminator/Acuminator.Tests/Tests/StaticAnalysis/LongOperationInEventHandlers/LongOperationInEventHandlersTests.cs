using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.LongOperationInEventHandlers;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities.Roslyn;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationInEventHandlers
{
	public class LongOperationInEventHandlersTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(new LongOperationInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData("Invalid.cs")]
		public void TestDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual, 
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(14, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(19, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(24, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(29, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(34, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(39, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(44, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(49, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(54, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(59, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(64, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(69, 4));
		}

		[Theory]
		[EmbeddedFileData("InvalidWithExternalMethod.cs")]
		public void TestDiagnostic_WithExternalMethod(string actual)
		{
			VerifyCSharpDiagnostic(actual,
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(14, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(19, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(24, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(29, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(34, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(39, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(44, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(49, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(54, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(59, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(64, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(69, 4));
		}

		[Theory]
		[EmbeddedFileData("Valid.cs")]
		public void TestDiagnostic_EventHandlers_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}
	}
}
