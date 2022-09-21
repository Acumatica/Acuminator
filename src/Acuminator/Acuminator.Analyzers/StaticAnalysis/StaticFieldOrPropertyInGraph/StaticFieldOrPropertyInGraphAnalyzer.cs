#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.StaticFieldOrPropertyInGraph
{
	public class StaticFieldOrPropertyInGraphAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1062_StaticFieldOrPropertyInGraph);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) =>
			base.ShouldAnalyze(pxContext, graph) && graph.Type != GraphType.None; 

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, PXGraphSemanticModel graphOrExtension)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var graphStaticFieldsAndProperties = from member in graphOrExtension.Symbol.GetMembers()
												 where member.IsStatic && member.CanBeReferencedByName && !member.IsImplicitlyDeclared &&
													   graphOrExtension.Symbol.Equals(member.ContainingType) &&
													   member is (IPropertySymbol or IFieldSymbol)
												 select member;

			foreach (ISymbol staticFieldOrProperty in graphStaticFieldsAndProperties)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();
				ReportStaticFieldOrProperty(symbolContext, pxContext, staticFieldOrProperty, graphOrExtension);
			}
		}

		private void ReportStaticFieldOrProperty(SymbolAnalysisContext symbolContext, PXContext pxContext, ISymbol staticFieldOrProperty,
												PXGraphSemanticModel graphOrExtension)
		{
			Location? location = GetStaticFieldOrPropertyLocation(staticFieldOrProperty, symbolContext.CancellationToken);

			if (location == null)
				return;

			var diagnosticInfo = GetDiagnosticFormatArgsAndProperties(staticFieldOrProperty, graphOrExtension);

			if (diagnosticInfo == null)
				return;

			var (formatArg, properties) = diagnosticInfo.Value;

			symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1062_StaticFieldOrPropertyInGraph, location, properties,  formatArg),
					pxContext.CodeAnalysisSettings);
		}

		private Location? GetStaticFieldOrPropertyLocation(ISymbol staticFieldOrProperty, CancellationToken cancellation)
		{
			var fieldOrPropertyDeclaration = staticFieldOrProperty.GetSyntax(cancellation);
			var staticModifier = fieldOrPropertyDeclaration?.GetModifiers()
															.FirstOrDefault(modifier => modifier.IsKind(SyntaxKind.StaticKeyword));	
			if (staticModifier != null)
				return staticModifier.Value.GetLocation();

			return !staticFieldOrProperty.Locations.IsDefaultOrEmpty
				? staticFieldOrProperty.Locations[0]
				: null;
		}

		private (string FormatArg, ImmutableDictionary<string, string>? Properties)? GetDiagnosticFormatArgsAndProperties(ISymbol staticFieldOrProperty,
																														  PXGraphSemanticModel graphOrExtension)
		{
			bool isView = graphOrExtension.ViewsByNames.ContainsKey(staticFieldOrProperty.Name);

			if (isView)
				return (Resources.PX1062MessageFormatArg_Views, CreatePropertiesWithIsViewOrAction());

			bool isAction = graphOrExtension.ActionsByNames.ContainsKey(staticFieldOrProperty.Name);

			if (isAction)
				return (Resources.PX1062MessageFormatArg_Actions, CreatePropertiesWithIsViewOrAction());

			switch (staticFieldOrProperty)
			{
				case IFieldSymbol:
					return (Resources.PX1062MessageFormatArg_Fields , null);
				case IPropertySymbol:
					return (Resources.PX1062MessageFormatArg_Properties, null);
				default:
					return null;
			}
		}

		private ImmutableDictionary<string, string> CreatePropertiesWithIsViewOrAction()
		{
			var properties = ImmutableDictionary.CreateBuilder<string, string>();
			properties.Add(StaticFieldOrPropertyInGraphDiagnosticProperties.IsViewOrAction, bool.TrueString);
			return properties.ToImmutable();
		}
	}
}