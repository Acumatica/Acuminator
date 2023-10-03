#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.NullableDiagnosticHider
{
	/// <summary>Customized version of CS8524 diagnostic.</summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class NullableDiagnosticHider : DiagnosticSuppressor
	{
		private const string PXGraph = "PX.Data.PXGraph";
		private const string PXGraphExtension = "PX.Data.PXGraphExtension";
		private const string PXSelectBase = "PX.Data.PXSelectBase";
		private const string PXAction = "PX.Data.PXAction";

		public static readonly SuppressionDescriptor NonNullableUninitializedAfterConstructorDiagnostic =
			new SuppressionDescriptor("PXS001", "CS8618", "Graph views and actions are always initialized by Acumatica platform");

		public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(NonNullableUninitializedAfterConstructorDiagnostic);

		public override void ReportSuppressions(SuppressionAnalysisContext context)
		{
			if (context.ReportedDiagnostics.IsDefaultOrEmpty)
				return;

			foreach (Diagnostic diagnostic in context.ReportedDiagnostics)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				SuppressNullableDiagnostic(context, diagnostic);
			}
		}

		private static void SuppressNullableDiagnostic(SuppressionAnalysisContext context, Diagnostic diagnostic)
		{
			if (diagnostic.IsSuppressed || diagnostic.Location.SourceTree is not { } syntaxTree)
				return;

			SyntaxNode? root = syntaxTree.GetRoot(context.CancellationToken);
			var diagnosticNode = root?.FindNode(diagnostic.Location.SourceSpan);

			if (diagnosticNode is not (ConstructorDeclarationSyntax or VariableDeclaratorSyntax or PropertyDeclarationSyntax) ||
				context.GetSemanticModel(syntaxTree) is not SemanticModel semanticModel)
			{
				return;
			}

			if (semanticModel.GetDeclaredSymbol(diagnosticNode, context.CancellationToken) is not { } memberSymbol ||
				memberSymbol.ContainingType == null || !IsDerivedFromPXGraphOrExtension(memberSymbol.ContainingType))
			{
				return;
			}
			
			switch (memberSymbol)
			{
				case IFieldSymbol field:
					AnalyzeFieldSymbol(field, context, diagnostic);
					break;

				case IPropertySymbol property:
					AnalyzePropertySymbol(property, context, diagnostic);
					break;

				case IMethodSymbol constructor
				when constructor.MethodKind == MethodKind.Constructor:
					AnalyzeDiagnosticOnConstructor(context, diagnostic);
					break;
			}
		}

		private static void AnalyzeFieldSymbol(IFieldSymbol field, SuppressionAnalysisContext context, Diagnostic diagnostic)
		{
			if (field.DeclaredAccessibility == Accessibility.Public &&
				field.Type is INamedTypeSymbol fieldType && IsGraphViewOrAction(fieldType))
			{
				var suppression = Suppression.Create(NonNullableUninitializedAfterConstructorDiagnostic, diagnostic);
				context.ReportSuppression(suppression);
			}	
		}

		private static void AnalyzePropertySymbol(IPropertySymbol property, SuppressionAnalysisContext context, Diagnostic diagnostic)
		{
			if (property.DeclaredAccessibility == Accessibility.Public &&
				property.Type is INamedTypeSymbol propertyType && IsGraphViewOrAction(propertyType))
			{
				var suppression = Suppression.Create(NonNullableUninitializedAfterConstructorDiagnostic, diagnostic);
				context.ReportSuppression(suppression);
			}
		}

		private static void AnalyzeDiagnosticOnConstructor(SuppressionAnalysisContext context, Diagnostic diagnostic)
		{
			if (diagnostic.AdditionalLocations.Count != 1 || diagnostic.AdditionalLocations[0] is not Location uninitializedMemberLocation ||
				uninitializedMemberLocation.SourceTree == null)
			{
				return;
			}

			var locationRoot = uninitializedMemberLocation.SourceTree.GetRoot(context.CancellationToken);
			var uninitializedMemberNode = locationRoot?.FindNode(uninitializedMemberLocation.SourceSpan);

			if (uninitializedMemberNode is not (VariableDeclaratorSyntax or PropertyDeclarationSyntax) ||
				context.GetSemanticModel(uninitializedMemberLocation.SourceTree) is not SemanticModel semanticModel)
			{
				return;
			}

			if (semanticModel.GetDeclaredSymbol(uninitializedMemberNode, context.CancellationToken) is not { } memberSymbol ||
				memberSymbol.ContainingType == null)
			{
				return;
			}

			switch (memberSymbol)
			{
				case IFieldSymbol field:
					AnalyzeFieldSymbol(field, context, diagnostic);
					break;

				case IPropertySymbol property:
					AnalyzePropertySymbol(property, context, diagnostic);
					break;
			}
		}

		private static bool IsDerivedFromPXGraphOrExtension(ITypeSymbol type) =>
			IsDerivedFromOneOfTwoTypes(type, PXGraph, PXGraphExtension);
		private static bool IsGraphViewOrAction(ITypeSymbol type) =>
			IsDerivedFromOneOfTwoTypes(type, PXSelectBase, PXAction);

		private static bool IsDerivedFromOneOfTwoTypes(ITypeSymbol type, string type1Name, string type2Name)
		{
			IEnumerable<ITypeSymbol> baseTypes = type.GetBaseTypesUtil(includeThis: false);

			foreach (ITypeSymbol baseType in baseTypes)
			{
				string fullName = baseType.ToString();

				if (type1Name.Equals(fullName, StringComparison.Ordinal) || type2Name.Equals(fullName, StringComparison.Ordinal))
					return true;
			}

			return false;
		}
	}


	internal static class Utils
	{
		public static IEnumerable<ITypeSymbol> GetBaseTypesUtil(this ITypeSymbol typeToUse, bool includeThis)
		{
			var current = includeThis ? typeToUse : typeToUse.BaseType;

			while (current != null)
			{
				yield return current;
				current = current.BaseType;
			}
		}
	}
}
