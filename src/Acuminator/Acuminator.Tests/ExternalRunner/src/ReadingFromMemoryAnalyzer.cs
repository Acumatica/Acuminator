#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.BannedApi;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

namespace ExternalRunner
{
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

		protected override void ReadAcuminatorSettingsFromSharedMemory()
		{
			base.ReadAcuminatorSettingsFromSharedMemory();

		}

		protected override bool ShouldRegisterAnalysisActions() => false;

		protected override bool ShouldAnalyze(PXContext pxContext) => true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisPerformance", "RS1012:Start action has no registered actions",
														 Justification = "Analyzer exists just for tests")]
		protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{ }
	}
}
