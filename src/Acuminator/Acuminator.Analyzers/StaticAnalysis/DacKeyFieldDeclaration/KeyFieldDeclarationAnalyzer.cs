#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

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

			var keyAttributes = new List<DacFieldAttributeInfo>(capacity: 2);
			var declaredInDacKeyAttributes = new List<DacFieldAttributeInfo>(capacity: 2);
			bool containsIdentityKeys = false;

			foreach (DacPropertyInfo property in dac.DacProperties.Where(p => p.IsKey))
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				IEnumerable<DacFieldAttributeInfo> propertyKeyAttributes = property.Attributes.Where(a => a.IsKey);
				containsIdentityKeys = containsIdentityKeys || property.IsIdentity;

				keyAttributes.AddRange(propertyKeyAttributes);

				if (dac.Symbol.Equals(property.Symbol.ContainingType))
				{
					declaredInDacKeyAttributes.AddRange(propertyKeyAttributes);
				}
			}

			if (keyAttributes.Count > 1 && containsIdentityKeys && declaredInDacKeyAttributes.Count > 0)
			{		
				List<Location> locations = declaredInDacKeyAttributes
											.Select(attribute => GetAttributeLocation(attribute.AttributeData, context.CancellationToken))
											.Where(location => location != null)
											.ToList()!;

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

		private static Location? GetAttributeLocation(AttributeData attribute, CancellationToken cancellationToken) =>
			attribute.ApplicationSyntaxReference
					?.GetSyntax(cancellationToken)
					?.GetLocation();
	}
}
