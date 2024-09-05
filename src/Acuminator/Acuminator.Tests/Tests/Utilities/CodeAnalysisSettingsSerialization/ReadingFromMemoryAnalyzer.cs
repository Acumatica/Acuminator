#nullable enable

using Acuminator.Analyzers.StaticAnalysis.BannedApi;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.Utilities.CodeAnalysisSettingsSerialization;

/// <summary>
/// An analyzer for Acuminator tests that just reads settings from memory and compares them with expected settings.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ReadingFromMemoryAnalyzer : BannedApiAnalyzer
{
	public CodeAnalysisSettings ExpectedCodeAnalysisSettings { get; }

	public BannedApiSettings ExpectedBannedApiSettings { get; }

	public ReadingFromMemoryAnalyzer(CodeAnalysisSettings expectedAnalysisSettings, BannedApiSettings expectedBannedApiSettings)
	{
		ExpectedCodeAnalysisSettings = expectedAnalysisSettings.CheckIfNull();
		ExpectedBannedApiSettings = expectedBannedApiSettings.CheckIfNull();
	}

	protected override string? SharedMemorySlotName => SharedVsSettings.SharedMemoryNameForTests;

	protected override void ReadAcuminatorSettingsFromSharedMemory()
	{
		base.ReadAcuminatorSettingsFromSharedMemory();

		Assert.Equal(CodeAnalysisSettings, ExpectedCodeAnalysisSettings);
		Assert.Equal(BannedApiSettings, ExpectedBannedApiSettings);
	}

	protected override bool ShouldRegisterAnalysisActions() => false;

	protected override bool ShouldAnalyze(PXContext pxContext) => true;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisPerformance", "RS1012:Start action has no registered actions",
													 Justification = "Analyzer exists just for tests")]
	protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
	{ }
}