using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.PublicExtensionAnalyzer
{
	/// <summary>
	/// An analyzer that checks that DAC and graph extensions are public.
	/// </summary>
	public class PublicGraphAndDacExtensionAnalyzer : IDacAnalyzer, IPXGraphAnalyzer
	{
		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1022_NonPublicDacExtension,
								  Descriptors.PX1022_NonPublicGraphExtension);

		bool IDacAnalyzer.ShouldAnalyze(PXContext pxContext, DacSemanticModel dac) =>
			dac?.DacType == DacType.DacExtension;

		bool IPXGraphAnalyzer.ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) =>
			graph?.Type == GraphType.PXGraphExtension;

		void IDacAnalyzer.Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacExtension) =>
			CheckSymbolIsPublic(context, pxContext, dacExtension);

		void IPXGraphAnalyzer.Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel graphExtension) =>
			CheckSymbolIsPublic(context, pxContext, graphExtension);

		private void CheckSymbolIsPublic(SymbolAnalysisContext context, PXContext pxContext, ISemanticModel extension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (IsExtensionPublic(extension.Symbol))
				return;

			string extensionType;
			DiagnosticDescriptor descriptor; 
			
			if (extension is DacSemanticModel)
			{
				extensionType = ExtensionType.DAC;
				descriptor = Descriptors.PX1022_NonPublicDacExtension;
			}
			else
			{
				extensionType = ExtensionType.Graph;
				descriptor = Descriptors.PX1022_NonPublicGraphExtension;
			}
			
			var locations = GetDiagnosticLocations(extension.Symbol, context.CancellationToken);
			var diagnosticProperties = new Dictionary<string, string>
			{
				{ nameof(ExtensionType),  extensionType}
			}
			.ToImmutableDictionary();

			foreach (Location location in locations)
			{
				context.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(descriptor, location, diagnosticProperties),
															 pxContext.CodeAnalysisSettings);
			}		
		}

		private bool IsExtensionPublic(INamedTypeSymbol extensionSymbol) =>
			extensionSymbol.GetContainingTypesAndThis()
						   .All(type => type.DeclaredAccessibility == Accessibility.Public);

		private IEnumerable<Location> GetDiagnosticLocations(INamedTypeSymbol extensionSymbol, CancellationToken cancellationToken) =>
			extensionSymbol.DeclaringSyntaxReferences
						   .Select(reference => reference.GetSyntax(cancellationToken))
						   .OfType<ClassDeclarationSyntax>()
						   .Select(classNode => GetLocationFromClassNode(classNode));
		
		private Location GetLocationFromClassNode(ClassDeclarationSyntax classNode)
		{
			if (classNode.Modifiers.Count == 0)
				return classNode.Identifier.GetLocation();

			int lastModifierIndex = classNode.Modifiers.Count - 1;

			for (int i = 0; i < classNode.Modifiers.Count; i++)
			{
				SyntaxToken modifier = classNode.Modifiers[i];

				switch (modifier.Kind())
				{
					case SyntaxKind.PublicKeyword:
						return classNode.Identifier.GetLocation();	//Case when nested public class is declared inside non-public type

					case SyntaxKind.PrivateKeyword:
						return GetLocationForComplexAccessModifier(modifier, i, secondPartKind: SyntaxKind.ProtectedKeyword);

					case SyntaxKind.ProtectedKeyword:
						return GetLocationForComplexAccessModifier(modifier, i, secondPartKind: SyntaxKind.InternalKeyword);

					case SyntaxKind.InternalKeyword:
						return modifier.GetLocation();
				}
			}

			return classNode.Identifier.GetLocation();

			//------------------------------------------------Local Function------------------------------------------------------------
			Location GetLocationForComplexAccessModifier(in SyntaxToken modifier, int modifierIndex, SyntaxKind secondPartKind)
			{
				if (modifierIndex == lastModifierIndex)
					return modifier.GetLocation();

				SyntaxToken nextModifier = classNode.Modifiers[modifierIndex + 1];
				return nextModifier.IsKind(secondPartKind)
					? Location.Create(modifier.SyntaxTree, TextSpan.FromBounds(modifier.SpanStart, nextModifier.Span.End))
					: modifier.GetLocation();
			}
		}
	}
}