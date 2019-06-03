using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Analyzers.StaticAnalysis.DacKeyFieldDeclaration
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class KeyFieldDeclarationAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField
			);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(symbolContext =>
				AnalyzeDacOrDacExtensionDeclaration(symbolContext, pxContext), SymbolKind.NamedType);
		}

		private static void AnalyzeDacOrDacExtensionDeclaration(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			if (!(symbolContext.Symbol is INamedTypeSymbol dacOrDacExtSymbol) || !dacOrDacExtSymbol.IsDacOrExtension(pxContext))
				return;

			var dacPropertiesDeclarations = dacOrDacExtSymbol.GetMembers().OfType<IPropertySymbol>();

			var keyAttributes = new List<AttributeData>();

			bool isKey = false;
			bool isKeyIdentity = false;

			foreach (var attribute in dacPropertiesDeclarations.SelectMany(a => a.GetAttributes()))
			{
				bool hasKeys = attribute.NamedArguments.Any(a => a.Key.Contains(DelegateNames.IsKey) &&
																	a.Value.Value is bool isKeyValue &&
																	isKeyValue == true);

				if (hasKeys)
				{
					var identityOrKey = CheckAttributeIdentityOrKey(attribute, pxContext);

					isKey = identityOrKey.IsKey || isKey;
					isKeyIdentity = identityOrKey.IsKeyIdentity || isKeyIdentity;

					keyAttributes.Add(attribute);
				}
			}

			if (isKey && isKeyIdentity)
			{
				var locations = keyAttributes.Select(attribute => GetAttributeLocation(attribute, symbolContext.CancellationToken)).ToList();

				foreach (Location attributeLocation in locations)
				{
					var extraLocations = locations.Where(l => l != attributeLocation);

					symbolContext.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(
							Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField, attributeLocation, extraLocations),
							pxContext.CodeAnalysisSettings);
				}
			}
		}

		private static Location GetAttributeLocation(AttributeData attribute, CancellationToken cancellationToken) =>
			attribute.ApplicationSyntaxReference
					?.GetSyntax(cancellationToken)
					?.GetLocation();
		
		private static bool IsDerivedFromIdentityTypes(AttributeData attribute, PXContext pxContext)
		{
			var attributeInformation = new AttributeInformation(pxContext);

			return attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, pxContext.FieldAttributes.PXDBIdentityAttribute) ||
				   attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, pxContext.FieldAttributes.PXDBLongIdentityAttribute);
		}

		private static (bool IsKey, bool IsKeyIdentity) CheckAttributeIdentityOrKey(AttributeData attribute, PXContext pxContext)
		{
			if (!IsDerivedFromIdentityTypes(attribute, pxContext))
				return (IsKey: true, IsKeyIdentity: false);
			else
				return (IsKey: false, IsKeyIdentity: true);
		}
	}
}
