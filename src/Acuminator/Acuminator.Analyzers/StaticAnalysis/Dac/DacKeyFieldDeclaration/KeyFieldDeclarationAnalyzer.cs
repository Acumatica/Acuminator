using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;

namespace Acuminator.Analyzers.StaticAnalysis.DacKeyFieldDeclaration
{
	public class KeyFieldDeclarationAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var keyAttributes = new List<AttributeInfo>(capacity: 2);
			bool containsIdentityKeys = false;

			foreach (DacPropertyInfo property in dac.DeclaredDacProperties.Where(p => p.IsKey))
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				IEnumerable<AttributeInfo> propertyKeyAttributes = property.Attributes.Where(a => a.IsKey);
				containsIdentityKeys = containsIdentityKeys || property.IsIdentity;

				keyAttributes.AddRange(propertyKeyAttributes);
			}

			if (keyAttributes.Count > 1 && containsIdentityKeys)
			{
				var locations = keyAttributes.Select(attribute => GetAttributeLocation(attribute.AttributeData, context.CancellationToken)).ToList();

				foreach (Location attributeLocation in locations)
				{
					var extraLocations = locations.Where(l => l != attributeLocation);

					context.ReportDiagnosticWithSuppressionCheck(
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
	}
}
