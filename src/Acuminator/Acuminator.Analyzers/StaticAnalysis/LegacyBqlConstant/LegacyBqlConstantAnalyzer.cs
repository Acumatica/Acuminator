using System;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Analyzers.StaticAnalysis.LegacyBqlField;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.DiagnosticSuppression;

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
			if (context.Symbol is INamedTypeSymbol constant)
			{
				if (!IsConstant(constant, pxContext, out string constantType) || LegacyBqlFieldAnalyzer.AlreadyStronglyTyped(constant, pxContext))
					return;

				context.CancellationToken.ThrowIfCancellationRequested();

				Location location = constant.Locations.FirstOrDefault();
				if (location != null)
				{
					var properties = ImmutableDictionary.CreateBuilder<string, string>();
					properties.Add(CorrespondingType, constantType);

					context.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(Descriptors.PX1061_LegacyBqlConstant, location, properties.ToImmutable(), constant.Name)); 
				}
			}
		}

		private static bool IsConstant(ITypeSymbol constantDef, PXContext pxContext, out string constantType)
		{
			constantType = null;

			if (constantDef.TypeKind != TypeKind.Class)
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