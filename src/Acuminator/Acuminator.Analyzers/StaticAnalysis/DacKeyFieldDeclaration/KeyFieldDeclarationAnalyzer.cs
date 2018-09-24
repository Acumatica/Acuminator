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
		private const string IsKey = "IsKey";

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
			if (!(symbolContext.Symbol is INamedTypeSymbol dacOrDacExtSymbol) || !dacOrDacExtSymbol.IsDacOrExtension(pxContext) || 
				symbolContext.CancellationToken.IsCancellationRequested)
				return;
			
			var dacPropertiesDeclarations  = dacOrDacExtSymbol.GetMembers().OfType<IPropertySymbol>();

			AttributeInformation attributeInformation = new AttributeInformation(pxContext);

			List<AttributeData> keyAttributes = new List<AttributeData>();
			var identityAttributeType = pxContext.FieldAttributes.PXDBIdentityAttribute;

			bool flagIsKey = false;
			bool flagIsKeyIdentity = false;
			
			foreach (var property in dacPropertiesDeclarations)
			{
				foreach (var attribute in property.GetAttributes())
				{
					if (!attribute.NamedArguments.IsEmpty &&
						attribute.NamedArguments.Where(a => a.Key.Contains(IsKey) &&
															!a.Value.IsNull &&
															a.Value.Value is bool boolValue &&
															boolValue == true)
												.Any() && 
												!attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, identityAttributeType))
					{
						flagIsKey = true;
						keyAttributes.Add(attribute);
					}

					if (!attribute.NamedArguments.IsEmpty &&
						attribute.NamedArguments.Where(a => a.Key.Contains(IsKey) &&
															!a.Value.IsNull &&
															a.Value.Value is bool boolValue &&
															boolValue == true)
												.Any() &&
												attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, identityAttributeType))
					{
						flagIsKeyIdentity = true;
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


		public static async Task<Location> GetAttributeLocationAsync(AttributeData attribute, CancellationToken cancellationToken)
		{
			SyntaxNode attributeSyntaxNode = null;

			attributeSyntaxNode = await attribute.ApplicationSyntaxReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);

			return attributeSyntaxNode?.GetLocation();
		}
	}
}
