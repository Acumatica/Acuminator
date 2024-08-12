#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.ForbiddenApi.Storage;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ForbiddenApi
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ForbiddenApiAnalyzer : PXDiagnosticAnalyzer
	{
		private const string _bannedApiFileRelativePath = @"StaticAnalysis\ForbiddenAPI\Data\BannedApis.txt";
		private const string _bannedApiAssemblyResourceName = @"StaticAnalysis.ForbiddenAPI.Data.BannedApis.txt";

		private const string _whiteListFileRelativePath = @"StaticAnalysis\ForbiddenAPI\Data\WhiteList.txt";
		private const string _whiteListAssemblyResourceName = @"StaticAnalysis.ForbiddenAPI.Data.WhiteList.txt";

		public static ApiStorage BannedApi { get; } = new ApiStorage(_bannedApiFileRelativePath, _bannedApiAssemblyResourceName);

		public static ApiStorage WhiteList { get; } = new ApiStorage(_whiteListFileRelativePath, _whiteListAssemblyResourceName);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = 
			ImmutableArray.Create
			(
				Descriptors.PX1065_ForbiddenApiUsage_NoDetails,
				Descriptors.PX1065_ForbiddenApiUsage_WithDetails
			);

		protected override bool ShouldAnalyze(PXContext pxContext) =>
			base.ShouldAnalyze(pxContext) && pxContext.IsAcumatica2019R1_OrGreater && pxContext.BqlConstantType != null;

		public ForbiddenApiAnalyzer() : this(null)
		{ }

		public ForbiddenApiAnalyzer(CodeAnalysisSettings? codeAnalysisSettings) : base(codeAnalysisSettings)
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
