using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public sealed class MultipleFieldTypeAttributesOnDacPropertyFix : MultipleAttributesOnDacPropertyFixBase
	{
		public override ImmutableArray<string> FixableDiagnosticIds => 
			ImmutableArray.Create
			(
				Descriptors.PX1023_MultipleTypeAttributesOnProperty.Id
			);

		protected override string GetCodeActionName() => nameof(Resources.PX1023TypeAttributesFix).GetLocalized().ToString();	
	}
}