using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.UiPresentationLogic;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.UiPresentationLogic
{
    public class UiPresentationLogicInActionHandlersTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(
                CodeAnalysisSettings.Default
                .WithRecursiveAnalysisEnabled(),
                new UiPresentationLogicInActionHandlersAnalyzer());

        [Theory]
        [EmbeddedFileData(@"ActionHandlers\Handlers_Bad.cs")]
        public async Task Actions_ReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source,
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(19, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(20, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(21, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(22, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(23, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(24, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(25, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(26, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(27, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(28, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(29, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(33, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(34, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(35, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(36, 13),

                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(43, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(44, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(45, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(46, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(47, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(48, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(49, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(50, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(51, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(52, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(53, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(57, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(58, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(59, 13),
                Descriptors.PX1089_UiPresentationLogicInActionDelegates.CreateFor(60, 13));

        [Theory]
        [EmbeddedFileData(@"ActionHandlers\Handlers_Good.cs")]
        public async Task Actions_WithoutUiPresentationLogic_DontReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source);
    }
}
