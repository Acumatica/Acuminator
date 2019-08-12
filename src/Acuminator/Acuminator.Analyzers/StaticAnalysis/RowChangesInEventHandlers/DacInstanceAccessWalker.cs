using System.Collections.Immutable;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers
{
	public partial class RowChangesInEventHandlersAnalyzer
	{
		/// <summary>
		/// Searches for a member access (conditional and unconditional) on a DAC instance
		/// </summary>
		private class DacInstanceAccessWalker : AccessWalkerBase
		{
			public DacInstanceAccessWalker(SemanticModel semanticModel)
				: base(semanticModel)
			{
			}

			protected override bool Predicate(ExpressionSyntax node)
			{
				if (node != null)
				{
					var type = SemanticModel.GetTypeInfo(node).ConvertedType;
					return type?.IsDAC() == true;
				}

				return false;
			}
		}

	}
}
