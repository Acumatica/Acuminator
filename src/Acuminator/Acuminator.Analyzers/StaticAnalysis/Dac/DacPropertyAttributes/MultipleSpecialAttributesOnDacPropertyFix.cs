using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;


namespace Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public sealed class MultipleSpecialAttributesOnDacPropertyFix : MultipleAttributesOnDacPropertyFixBase
	{
		public override ImmutableArray<string> FixableDiagnosticIds => 
			ImmutableArray.Create
			(
				Descriptors.PX1023_MultipleSpecialTypeAttributesOnProperty.Id
			);

		protected override string GetCodeActionName() => nameof(Resources.PX1023SpecialAttributesFix).GetLocalized().ToString();

		protected override Func<FieldTypeAttributeInfo, bool> GetRemoveAttributeByAttributeInfoPredicate() => 
			attributeInfo => attributeInfo.IsSpecial;
		
	}
}