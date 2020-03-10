using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.AutoNumberAttribute
{
	public class DacAutoNumberAttributeAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1019_AutoNumberOnDacPropertyWithNonStringType, 
				Descriptors.PX1020_InsufficientStringLengthForDacPropertyWithAutoNumbering 
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExt)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			AttributeInformation attributeInformation = new AttributeInformation(pxContext);
	
			foreach (DacPropertyInfo dacProperty in dacOrDacExt.DeclaredDacProperties)
			{
				CheckDacProperty(context, pxContext, dacProperty, attributeInformation);
			}		
		}

		private void CheckDacProperty(SymbolAnalysisContext context, PXContext pxContext, DacPropertyInfo dacProperty, 
									  AttributeInformation attributeInformation)
		{
			var diagnostic = Diagnostic.Create(Descriptors.PX1094_DacShouldHaveUiAttribute,
											   dac.Node.Identifier.GetLocation());

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}
	}
}
