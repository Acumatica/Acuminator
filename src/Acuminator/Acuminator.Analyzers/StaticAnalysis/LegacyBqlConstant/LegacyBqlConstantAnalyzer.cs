using System;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Analyzers.StaticAnalysis.LegacyBqlField;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LegacyBqlConstant
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class LegacyBqlConstantAnalyzer : PXDiagnosticAnalyzer
	{
		public const string CorrespondingType = "CorrespondingType";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.PX1061_LegacyBqlConstant);

	    protected override bool ShouldAnalyze(PXContext pxContext) => pxContext.IsAcumatica2019R1;

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(
				c => Analyze(c, pxContext),
				SymbolKind.NamedType);
		}

		private async void Analyze(SymbolAnalysisContext context, PXContext pxContext)
		{
			INamedTypeSymbol constant = context.Symbol as INamedTypeSymbol;
			if (!IsConsant(constant, pxContext, out string constantType) || LegacyBqlFieldAnalyzer.AlreadyStronglyTyped(constant, pxContext)) return;

			var properties = ImmutableDictionary.CreateBuilder<string, string>();
			properties.Add(CorrespondingType, constantType);
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1061_LegacyBqlConstant, constant.Locations.First(), properties.ToImmutable(), constant.Name));
		}

		private static bool IsConsant(ITypeSymbol constantDef, PXContext pxContext, out string constantType)
		{
			constantType = null;
			if (constantDef == null || constantDef.TypeKind != TypeKind.Class)
				return false;

			var constantUnderlyingType = constantDef
				.GetBaseTypes()
				.FirstOrDefault(t => t.IsGenericType && t.InheritsFromOrEqualsGeneric(pxContext.BqlConstantType))?
				.TypeArguments[0];
			if (constantUnderlyingType == null)
				return false;

			if (LegacyBqlFieldFix.PropertyTypeToFieldType.ContainsKey(constantUnderlyingType.Name))
			{
				constantType = constantUnderlyingType.Name;
				return true;
			}

			return false;
		}
	}
}