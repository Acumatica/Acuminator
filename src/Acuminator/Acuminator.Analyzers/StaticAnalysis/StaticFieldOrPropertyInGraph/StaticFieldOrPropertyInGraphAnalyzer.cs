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
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
													   member is IPropertySymbol or IFieldSymbol
												 select member;

			foreach (ISymbol staticFieldOrProperty in graphStaticFieldsAndProperties)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();
				AnalyseStaticFieldOrProperty(symbolContext, pxContext, staticFieldOrProperty, graphOrExtension);
			}
		}

		private void AnalyseStaticFieldOrProperty(SymbolAnalysisContext symbolContext, PXContext pxContext, ISymbol staticFieldOrProperty,
												  PXGraphSemanticModel graphOrExtension)
		{
			bool isView = graphOrExtension.ViewsByNames.ContainsKey(staticFieldOrProperty.Name);
			bool isAction = graphOrExtension.ActionsByNames.ContainsKey(staticFieldOrProperty.Name);

			if (!isView && !isAction && staticFieldOrProperty.IsReadOnly())
				return;

			Location? location = GetStaticFieldOrPropertyLocation(staticFieldOrProperty, symbolContext.CancellationToken);

			if (location == null)
				return;

			var diagnosticInfo = GetDiagnosticFormatArgsAndProperties(staticFieldOrProperty, isView, isAction);

			if (diagnosticInfo == null)
				return;

			var (formatArg, properties) = diagnosticInfo.Value;

			symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1062_StaticFieldOrPropertyInGraph, location, properties,  formatArg),
					pxContext.CodeAnalysisSettings);
		}

		private Location? GetStaticFieldOrPropertyLocation(ISymbol staticFieldOrProperty, CancellationToken cancellation)
		{
			var fieldOrPropertyNode = staticFieldOrProperty.GetSyntax(cancellation);
			var fieldOrPropertyDeclaration = fieldOrPropertyNode?.ParentOrSelf<MemberDeclarationSyntax>();
			var staticModifier = fieldOrPropertyDeclaration?.GetModifiers()
															.FirstOrDefault(modifier => modifier.IsKind(SyntaxKind.StaticKeyword));	
			if (staticModifier != null)
				return staticModifier.Value.GetLocation();

			return !staticFieldOrProperty.Locations.IsDefaultOrEmpty
				? staticFieldOrProperty.Locations[0]
				: null;
		}

		private (string FormatArg, ImmutableDictionary<string, string>? Properties)? GetDiagnosticFormatArgsAndProperties(ISymbol staticFieldOrProperty,
																														  bool isView, bool isAction)
		{
			if (isView)
			{
				return (Resources.PX1062MessageFormatArg_Views,
							CreateDiagnosticProperties(isViewOrAction: true, isProperty: staticFieldOrProperty is IPropertySymbol,
													   Resources.PX1062FixFormatArg_View));
			}
			else if (isAction)
			{
				return (Resources.PX1062MessageFormatArg_Actions, 
							CreateDiagnosticProperties(isViewOrAction: true, isProperty: staticFieldOrProperty is IPropertySymbol, 
													   Resources.PX1062FixFormatArg_Action));
			}

			switch (staticFieldOrProperty)
			{
				case IFieldSymbol:
					return (Resources.PX1062MessageFormatArg_Fields, 
							CreateDiagnosticProperties(isViewOrAction: false, isProperty: false, Resources.PX1062FixFormatArg_Field));
				case IPropertySymbol:
					return (Resources.PX1062MessageFormatArg_Properties, 
							CreateDiagnosticProperties(isViewOrAction: false, isProperty: true, Resources.PX1062FixFormatArg_Property));
				default:
					return null;
			}
		}

		private ImmutableDictionary<string, string> CreateDiagnosticProperties(bool isViewOrAction, bool isProperty, string codeFixFormatArg)
		{
			var properties = ImmutableDictionary.CreateBuilder<string, string>();
			properties.Add(StaticFieldOrPropertyInGraphDiagnosticProperties.CodeFixFormatArg, codeFixFormatArg);

			if (isViewOrAction)
				properties.Add(StaticFieldOrPropertyInGraphDiagnosticProperties.IsViewOrAction, bool.TrueString);

			if (isProperty)
				properties.Add(StaticFieldOrPropertyInGraphDiagnosticProperties.IsProperty, bool.TrueString);

			return properties.ToImmutable();
		}
	}
}