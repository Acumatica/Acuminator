using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.DacKeyFieldDeclaration
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class KeyFieldDeclarationAnalyzer : PXDiagnosticAnalyzer
	{
		private const string IsKey = nameof(PX.Data.PXDBFieldAttribute.IsKey);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField
			);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(async symbolContext =>
				await AnalyzeDacOrDacExtensionDeclarationAsync(symbolContext, pxContext).ConfigureAwait(false), SymbolKind.NamedType);
		}

		private static async Task AnalyzeDacOrDacExtensionDeclarationAsync(SymbolAnalysisContext symbolContext, PXContext pxContext)
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
				bool hasKeys = attribute.NamedArguments.Any(a => a.Key.Contains(IsKey) &&
																	a.Value.Value is bool isKeyValue &&
																	isKeyValue == true);

				if (hasKeys)
				{
					var identityOrKey = CheckAttributeIdentityOrKey(attribute, pxContext);

					isKey = identityOrKey.IsKey? true : isKey;
					isKeyIdentity = identityOrKey.IsKeyIdentity ? true : isKeyIdentity;

					keyAttributes.Add(attribute);
				}
			}

			if (isKey && isKeyIdentity)
			{
				var locations = new List<Location>();

				foreach (var attribute in keyAttributes)
				{
					locations.Add(await GetAttributeLocationAsync(attribute, symbolContext.CancellationToken).ConfigureAwait(false));
				}

				var extraLocations = new HashSet<Location>(locations);

				foreach (var attributeLocation in locations)
				{
					extraLocations.Remove(attributeLocation);

					symbolContext.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField, attributeLocation, extraLocations));

					extraLocations.Add(attributeLocation);
				}
			}
		}

		private static async Task<Location> GetAttributeLocationAsync(AttributeData attribute, CancellationToken cancellationToken)
		{
			if (attribute.ApplicationSyntaxReference == null)
				return null;

			SyntaxNode attributeSyntaxNode = await attribute.ApplicationSyntaxReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
			
			return attributeSyntaxNode?.GetLocation();
		}

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
