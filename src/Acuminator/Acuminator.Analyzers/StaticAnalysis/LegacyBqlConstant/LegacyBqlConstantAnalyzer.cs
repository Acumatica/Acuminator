using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LegacyBqlConstant
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class LegacyBqlConstantAnalyzer : PXDiagnosticAnalyzer
	{
		public const string CorrespondingType = "CorrespondingType";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.PX1061_LegacyBqlConstant);

		protected override bool ShouldAnalyze(PXContext pxContext) =>
			base.ShouldAnalyze(pxContext) && pxContext.IsAcumatica2019R1_OrGreater && pxContext.BqlConstantType != null;

		public LegacyBqlConstantAnalyzer() : this(null)
		{ }

		public LegacyBqlConstantAnalyzer(CodeAnalysisSettings? codeAnalysisSettings) : base(codeAnalysisSettings)
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

			if (context.Symbol is INamedTypeSymbol constant)
			{
				if (!IsConstant(constant, pxContext, out string? constantType) || constant.IsStronglyTypedBqlFieldOrBqlConstant(pxContext))
					return;

				Location? location = constant.Locations.FirstOrDefault();
				if (location != null)
				{
					var properties = ImmutableDictionary.CreateBuilder<string, string?>();
					properties.Add(CorrespondingType, constantType);

					context.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1061_LegacyBqlConstant, location, properties.ToImmutable(), constant.Name),
						pxContext.CodeAnalysisSettings); 
				}
			}
		}

		private static bool IsConstant(ITypeSymbol constantDef, PXContext pxContext, [NotNullWhen(returnValue: true)] out string? constantType)
		{
			constantType = null;

			if (constantDef.TypeKind != TypeKind.Class)
				return false;

			var constantUnderlyingType = constantDef
				.GetBaseTypes()
				.OfType<INamedTypeSymbol>()
				.FirstOrDefault(t => t.IsGenericType && t.InheritsFromOrEqualsGeneric(pxContext.BqlConstantType!))?
				.TypeArguments[0];

			if (constantUnderlyingType == null || constantUnderlyingType.Name.IsNullOrWhiteSpace())
				return false;

			if (PropertyTypeToBqlFieldTypeMapping.ContainsPropertyType(constantUnderlyingType.Name))
			{
				constantType = constantUnderlyingType.Name;
				return true;
			}

			return false;
		}
	}
}
