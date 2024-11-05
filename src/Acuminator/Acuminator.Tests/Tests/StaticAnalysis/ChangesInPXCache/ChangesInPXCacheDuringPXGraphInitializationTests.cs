using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ChangesInPXCache
{
	public class ChangesInPXCacheDuringPXGraphInitializationTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
				.WithRecursiveAnalysisEnabled(),
				new ChangesInPXCacheDuringPXGraphInitializationAnalyzer());

		[Theory]
		[EmbeddedFileData(@"PXGraph\PXGraphChangesPXCacheInInstanceConstructor.cs")]
		public void GraphInstanceConstructor_ReportsDiagnostic(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(16, 17),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(17, 17),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(18, 17),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(20, 17),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(21, 17),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(22, 17));
		}

		[Theory]
		[EmbeddedFileData(@"PXGraph\PXGraphChangesPXCacheInInstanceConstructorViaMethod.cs")]
		public void GraphInstanceConstructorViaMethod_ReportsDiagnostic(string source)
		{
			VerifyCSharpDiagnostic(source, Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(16, 17));
		}

		[Theory]
		[EmbeddedFileData(@"PXGraph\PXGraphExtensionChangesPXCacheInInitializationMethod.cs")]
		public void GraphExtensionInitializationMethod_ReportsDiagnostic(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(21, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(22, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(23, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(25, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(26, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(27, 5));
		}

		[Theory]
		[EmbeddedFileData(@"PXGraph\PXGraphDoesntChangePXCacheInInstanceConstructor.cs")]
		public void GraphInstanceConstructor_DoesntReportsDiagnostic(string source)
		{
			VerifyCSharpDiagnostic(source);
		}
	}
}
