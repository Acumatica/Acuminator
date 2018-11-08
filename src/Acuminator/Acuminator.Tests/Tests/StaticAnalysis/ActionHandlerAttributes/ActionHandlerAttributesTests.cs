using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ActionHandlerAttributes;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ActionHandlerAttributes
{
    public class ActionHandlerAttributesTests : CodeFixVerifier
    {
        protected override CodeFixProvider GetCSharpCodeFixProvider() => new ActionHandlerAttributesFix();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(CodeAnalysisSettings.Default, new ActionHandlerAttributesAnalyzer());

        [Theory]
        [EmbeddedFileData("Handler_Bad.cs")]
        public async Task Handler_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1092_MissingAttributesOnActionHandler.CreateFor(1, 1));
    }
}
