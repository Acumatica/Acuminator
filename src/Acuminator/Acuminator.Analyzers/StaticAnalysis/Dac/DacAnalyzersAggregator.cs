using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Analyzers.StaticAnalysis.ConstructorInDac;
using Acuminator.Analyzers.StaticAnalysis.DacExtensionDefaultAttribute;
using Acuminator.Analyzers.StaticAnalysis.DacKeyFieldDeclaration;
using Acuminator.Analyzers.StaticAnalysis.DacNonAbstractFieldType;
using Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes;
using Acuminator.Analyzers.StaticAnalysis.DacUiAttributes;
using Acuminator.Analyzers.StaticAnalysis.ForbiddenFieldsInDac;
using Acuminator.Analyzers.StaticAnalysis.InheritanceFromPXCacheExtension;
using Acuminator.Analyzers.StaticAnalysis.LegacyBqlField;
using Acuminator.Analyzers.StaticAnalysis.MethodsUsageInDac;
using Acuminator.Analyzers.StaticAnalysis.MissingTypeListAttribute;
using Acuminator.Analyzers.StaticAnalysis.NonNullableTypeForBqlField;
using Acuminator.Analyzers.StaticAnalysis.PXGraphUsageInDac;
using Acuminator.Analyzers.StaticAnalysis.UnderscoresInDac;
using Acuminator.Analyzers.StaticAnalysis.AutoNumberAttribute;
using Acuminator.Analyzers.StaticAnalysis.NonPublicExtensions;
using Acuminator.Analyzers.StaticAnalysis.NoIsActiveMethodForExtension;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.Dac
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DacAnalyzersAggregator : SymbolAnalyzersAggregator<IDacAnalyzer>
    {
        protected override SymbolKind SymbolKind => SymbolKind.NamedType;

        public DacAnalyzersAggregator() : this(null,
			new DacPropertyAttributesAnalyzer(),
			new DacAutoNumberAttributeAnalyzer(),
			new DacNonAbstractFieldTypeAnalyzer(),
			new ConstructorInDacAnalyzer(),
			new UnderscoresInDacAnalyzer(),
			new NonPublicGraphAndDacExtensionAnalyzer(),
			new ForbiddenFieldsInDacAnalyzer(),
			new DacUiAttributesAnalyzer(),
			new InheritanceFromPXCacheExtensionAnalyzer(),
			new LegacyBqlFieldAnalyzer(),
			new MethodsUsageInDacAnalyzer(),
			new KeyFieldDeclarationAnalyzer(),
			new DacExtensionDefaultAttributeAnalyzer(),
			new NonNullableTypeForBqlFieldAnalyzer(),
			new MissingTypeListAttributeAnalyzer(),
			new PXGraphUsageInDacAnalyzer(),
			new NoIsActiveMethodForDacExtensionAnalyzer())
		{
        }

        /// <summary>
        /// Constructor for the unit tests.
        /// </summary>
        public DacAnalyzersAggregator(CodeAnalysisSettings settings, params IDacAnalyzer[] innerAnalyzers) : base(settings, innerAnalyzers)
        {
        }

		protected override void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!(context.Symbol is INamedTypeSymbol type))
				return;

			var inferredDacModel = DacSemanticModel.InferModel(pxContext, type, context.CancellationToken);

			if (inferredDacModel == null)
				return;

			ParallelOptions parallelOptions = new ParallelOptions
			{
				CancellationToken = context.CancellationToken
			};

			Parallel.ForEach(_innerAnalyzers, parallelOptions, innerAnalyzer =>
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (innerAnalyzer.ShouldAnalyze(pxContext, inferredDacModel))
				{
					innerAnalyzer.Analyze(context, pxContext, inferredDacModel);
				}
			});
		}
    }
}
