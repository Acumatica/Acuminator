using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ConstructorInDac
{
	/// <summary>
	/// An analyzer for constructors in DAC.
	/// </summary>
	public class ConstructorInDacAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1028_ConstructorInDacDeclaration);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExtenstion)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var dacConstructors = dacOrDacExtenstion.DacNode.Members.OfType<ConstructorDeclarationSyntax>();

			foreach (var constructor in dacConstructors)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				Location location = constructor.Identifier.GetLocation();

				if (location != null)
				{
					context.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1028_ConstructorInDacDeclaration, location),
						pxContext.CodeAnalysisSettings);
				}
			}
		}
	}
}