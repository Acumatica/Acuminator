using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using PX.Data;
using Acuminator.Analyzers;

namespace Acuminator.Utilities
{
	/// <summary>
	/// Information about the Acumatica field attributes.
	/// </summary>
	public class FieldAttributesInfo
	{
		private readonly PXContext context;

		public ImmutableDictionary<INamedTypeSymbol, INamedTypeSymbol> CorrespondingSimpleTypes { get; }

		public FieldAttributesInfo(PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			context = pxContext;
			CorrespondingSimpleTypes = new Dictionary<INamedTypeSymbol, INamedTypeSymbol>
			{
				{ context.FieldAttributes.PXStringAttribute,  context.String },
				{ context.FieldAttributes.PXDBStringAttribute, context.String },
				{ context.FieldAttributes.PXDBIntAttribute, context.Int32 },
				{ context.FieldAttributes.PXIntAttribute, context.Int32 }
			}.ToImmutableDictionary();
		}
	}
}
