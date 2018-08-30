using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using PX.Data;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;


namespace Acuminator.Utilities.PrimaryDAC
{
	/// <summary>
	/// A rule for views with PXViewNameAttribute attribute.
	/// </summary>
	public class PXViewNameAttributeViewRule : ViewRuleBase
	{
		private readonly INamedTypeSymbol pxViewNameAttribute;

		public sealed override bool IsAbsolute => false;

		public PXViewNameAttributeViewRule(PXContext context, double? weight = null) : base(weight)
		{
			context.ThrowOnNull(nameof(context));

			pxViewNameAttribute = context.Compilation.GetTypeByMetadataName(typeof(PXViewNameAttribute).FullName);
		}

		public override bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol view, INamedTypeSymbol viewType)
		{
			if (view == null || dacFinder == null || dacFinder.CancellationToken.IsCancellationRequested)
				return false;

			ImmutableArray<AttributeData> attributes = view.GetAttributes();

			if (attributes.Length == 0)
				return false;

			return attributes.SelectMany(a => a.AttributeClass.GetBaseTypesAndThis())
							 .Any(baseType => baseType.Equals(pxViewNameAttribute));
		}
	}
}