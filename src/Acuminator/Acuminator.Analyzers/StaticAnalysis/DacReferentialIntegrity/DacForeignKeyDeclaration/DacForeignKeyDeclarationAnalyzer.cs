using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Symbols;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	public class DacForeignKeyDeclarationAnalyzer : DacKeyDeclarationAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1034_MissingDacForeignKeyDeclaration,
				Descriptors.PX1036_WrongDacForeignKeyName
			);

		protected override bool IsKeySymbolDefined(PXContext context) =>
			context.ReferentialIntegritySymbols.IForeignKey != null || 
			context.ReferentialIntegritySymbols.KeysRelation != null;

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();
			
			var allForeignKeyDeclarationsByContainingType = GetForeignKeyDeclarations(context, dac, symbolContext.CancellationToken)
															 .ToLookup(type => type.ContainingType);

			if (allForeignKeyDeclarationsByContainingType.Count == 0)
			{
				ReportNoForeignKeyDeclarationsInDac(symbolContext, context, dac);
				return;
			}
			
			//TODO extend logic to check foreign keys declarations
		}

		private IEnumerable<INamedTypeSymbol> GetForeignKeyDeclarations(PXContext context, DacSemanticModel dac, CancellationToken cancellationToken)
		{
			var allNestedTypes = dac.Symbol.GetFlattenedNestedTypes(cancellationToken);

			if (context.ReferentialIntegritySymbols.IForeignKey != null)
			{
				return allNestedTypes.Where(type => type.ImplementsInterface(context.ReferentialIntegritySymbols.IForeignKey));
			}

			return from nestedType in allNestedTypes
				   where nestedType.InheritsFromOrEqualsGeneric(context.ReferentialIntegritySymbols.KeysRelation) &&
						 nestedType.GetBaseTypesAndThis().Any(IsForeignKey)
				   select nestedType;
		}

		private bool IsForeignKey(ITypeSymbol type)
		{
			ITypeSymbol currentType = type.ContainingType;

			while (currentType != null)
			{
				if (PXReferentialIntegritySymbols.ForeignKeyContainerNames.Contains(currentType.Name))
					return true;

				currentType = currentType.ContainingType;
			}

			return false;
		}

		private void ReportNoForeignKeyDeclarationsInDac(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac)
		{
			Location location = dac.Node.Identifier.GetLocation() ?? dac.Node.GetLocation();

			if (location != null)
			{
				symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1034_MissingDacForeignKeyDeclaration, location),
					context.CodeAnalysisSettings);
			} 
		}	
	}
}