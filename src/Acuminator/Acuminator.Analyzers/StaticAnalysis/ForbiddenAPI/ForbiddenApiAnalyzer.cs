using System;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ForbiddenApi
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ForbiddenApiAnalyzer : PXDiagnosticAnalyzer
	{
		public const string CorrespondingType = "CorrespondingType";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.PX1061_LegacyBqlConstant);

		protected override bool ShouldAnalyze(PXContext pxContext) =>
			base.ShouldAnalyze(pxContext) && pxContext.IsAcumatica2019R1_OrGreater && pxContext.BqlConstantType != null;

		public ForbiddenApiAnalyzer() : this(null)
		{ }

		public ForbiddenApiAnalyzer(CodeAnalysisSettings codeAnalysisSettings) : base(codeAnalysisSettings)
		{ }

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(
				c => Analyze(c, pxContext),
				SymbolKind.NamedType);
		}

		private void Analyze(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			
		}
	}
}
