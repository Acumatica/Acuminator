﻿using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ChangesInPXCache
{
    public class PXGraphChangesInPXCacheDuringInitializationTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(new PXGraphChangesInPXCacheDuringInitializationAnalyzer());

        [Theory]
        [EmbeddedFileData(@"PXGraph\PXGraphChangesPXCacheInInstanceConstructor.cs")]
        public void GraphInstanceConstructor_ReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source,
                Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization.CreateFor(16, 17),
                Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization.CreateFor(17, 17),
                Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization.CreateFor(18, 17),
                Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization.CreateFor(20, 17),
                Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization.CreateFor(21, 17),
                Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization.CreateFor(22, 17));
        }

        [Theory]
        [EmbeddedFileData(@"PXGraph\PXGraphChangesPXCacheInInstanceConstructorViaMethod.cs")]
        public void GraphInstanceConstructorViaMethod_ReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization.CreateFor(16, 17));
        }

        [Theory]
        [EmbeddedFileData(@"PXGraph\PXGraphExtensionChangesPXCacheInInitializationMethod.cs")]
        public void GraphExtensionInitializationMethod_ReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source,
                Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization.CreateFor(14, 17),
                Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization.CreateFor(15, 17),
                Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization.CreateFor(16, 17),
                Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization.CreateFor(18, 17),
                Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization.CreateFor(19, 17),
                Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization.CreateFor(20, 17));
        }

        [Theory]
        [EmbeddedFileData(@"PXGraph\PXGraphDoesntChangePXCacheInInstanceConstructor.cs")]
        public void GraphInstanceConstructor_DoesntReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source);
        }
    }
}
