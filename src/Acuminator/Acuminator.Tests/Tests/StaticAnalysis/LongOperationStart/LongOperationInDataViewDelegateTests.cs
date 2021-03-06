﻿using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using System.Threading.Tasks;
using Acuminator.Utilities;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationStart
{
    public class LongOperationInDataViewDelegateTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(
                CodeAnalysisSettings.Default
                .WithRecursiveAnalysisEnabled(),
                new LongOperationInDataViewDelegateAnalyzer());

        [Theory]
        [EmbeddedFileData(@"PXGraph\DataViewFromGraphStartsLongOperation.cs")]
        public async Task DataViewDelegateFromGraph_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(17, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\DataViewFromGraphStartsLongOperationViaMethod.cs")]
        public async Task DataViewDelegateWithMethodFromGraph_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(17, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\DataViewFromGraphExtensionStartsLongOperation.cs")]
        public async Task DataViewDelegateFromGraphExtension_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(15, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\DataViewWithParameterStartsLongOperation.cs")]
        public async Task DataViewDelegateWithParameter_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(19, 17));

        [Theory]
        [EmbeddedFileData(@"PXGraph\StaticDataViewStartsLongOperation.cs")]
        public async Task StaticDataViewDelegate_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(15, 13));
    }
}
