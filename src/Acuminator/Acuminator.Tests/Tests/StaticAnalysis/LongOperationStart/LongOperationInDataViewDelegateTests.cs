using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationStart
{
    public class LongOperationInDataViewDelegateTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(new LongOperationInDataViewDelegateAnalyzer());

        [Theory]
        [EmbeddedFileData(@"PXGraph\DataViewFromGraphStartsLongOperation.cs")]
        public void GraphInstanceConstructor_ReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(17, 13));
        }

        [Theory]
        [EmbeddedFileData(@"PXGraph\DataViewFromGraphStartsLongOperationViaMethod.cs")]
        public void GraphInstanceConstructorViaMethod_ReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(17, 13));
        }

        [Theory]
        [EmbeddedFileData(@"PXGraph\DataViewFromGraphExtensionStartsLongOperation.cs")]
        public void GraphExtensionInitializationMethod_ReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(15, 13));
        }
    }
}
