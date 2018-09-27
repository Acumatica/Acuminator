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
				Descriptors.PX1055_DacKeyFieldBound
			);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(async symbolContext =>
				await AnalyzeDacOrDacExtensionDeclarationAsync(symbolContext, pxContext), SymbolKind.NamedType);
		}

		private static async Task AnalyzeDacOrDacExtensionDeclarationAsync(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			if (!(symbolContext.Symbol is INamedTypeSymbol dacOrDacExtSymbol) || !dacOrDacExtSymbol.IsDacOrExtension(pxContext))
				return;
			
			var dacPropertiesDeclarations  = dacOrDacExtSymbol.GetMembers().OfType<IPropertySymbol>();

			List<AttributeData> keyAttributes = new List<AttributeData>();
			
			bool flagIsKey = false;
			bool flagIsKeyIdentity = false;
			
			foreach (var property in dacPropertiesDeclarations)
			{
				foreach (var attribute in property.GetAttributes())
				{
					if (attribute.NamedArguments.Where(a => a.Key.Contains(IsKey) &&
															 a.Value.Value is bool boolValue &&
															 boolValue == true)
												.Any())
					{
						CheckAttributeIdentityOrKey(ref flagIsKey,ref flagIsKeyIdentity, attribute, pxContext);
						keyAttributes.Add(attribute);
					}
				}
			}

			if(flagIsKey && flagIsKeyIdentity)
			{
				List<Location> locations = new List<Location>();

				foreach (var attribute in keyAttributes)
				{
					locations.Add(await GetAttributeLocationAsync(attribute, symbolContext.CancellationToken).ConfigureAwait(false));
				}

				foreach (var attribute in keyAttributes)
				{
					Location attributeLocation = await GetAttributeLocationAsync(attribute, symbolContext.CancellationToken).ConfigureAwait(false);

					symbolContext.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.PX1055_DacKeyFieldBound, attributeLocation, locations));
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
			AttributeInformation attributeInformation = new AttributeInformation(pxContext);

			INamedTypeSymbol identityAttributeType = pxContext.FieldAttributes.PXDBIdentityAttribute;
			INamedTypeSymbol longIdentityAttributeType = pxContext.FieldAttributes.PXDBLongIdentityAttribute;

			return attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, identityAttributeType) ||
				   attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, longIdentityAttributeType);
		}

		private static void CheckAttributeIdentityOrKey(ref bool flagIsKey,ref bool flagIsKeyIdentity, AttributeData attribute, PXContext pxContext)
		{
			if (!IsDerivedFromIdentityTypes(attribute, pxContext))
				flagIsKey = true;
			else
				flagIsKeyIdentity = true;
		}
	}
}
