using Acuminator.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers
{
	public partial class RowChangesInEventHandlersAnalyzer
	{
		/// <summary>
		/// Searches for <code>e.Row</code> mentions
		/// </summary>
		private class EventArgsRowWalker : CSharpSyntaxWalker
		{
			private static readonly string RowPropertyName = "Row";

			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;

			public bool Success { get; private set; }

			public EventArgsRowWalker(SemanticModel semanticModel, PXContext pxContext)
			{
				_semanticModel = semanticModel;
				_pxContext = pxContext;
			}

			public override void Visit(SyntaxNode node)
			{
				if (!Success)
					base.Visit(node);
			}

			public override void VisitIdentifierName(IdentifierNameSyntax node)
			{
				if (node.Identifier.Text == RowPropertyName)
				{
					var propertySymbol = _semanticModel.GetSymbolInfo(node).Symbol as IPropertySymbol;
					var containingType = propertySymbol?.ContainingType?.OriginalDefinition;

					if (containingType != null && _pxContext.Events.EventTypeMap.ContainsKey(containingType))
					{
						Success = true;
					}
				}
			}
		}

	}
}
