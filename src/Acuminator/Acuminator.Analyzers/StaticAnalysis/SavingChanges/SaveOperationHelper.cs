using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.SavingChanges
{
	internal static class SaveOperationHelper
	{
		private const string GraphPersistMethodName = "Persist"; // PXGraph.Persist
		private const string PressSaveMethodName = "PressSave"; // PXActionCollection.PressSave
		private const string PressMethodName = "Press"; // PXAction.Press
		private const string CachePersistMethodName = "Persist"; // PXCache.Persist
		private const string CachePersistInsertedMethodName = "PersistInserted"; // PXCache.PersistInserted
		private const string CachePersistUpdatedMethodName = "PersistUpdated"; // PXCache.PersistUpdated
		private const string CachePersistDeletedMethodName = "PersistDeleted"; // PXCache.PersistDeleted

		public static SaveOperationKind GetSaveOperationKind(IMethodSymbol symbol, InvocationExpressionSyntax syntaxNode, 
			SemanticModel semanticModel, PXContext pxContext)
		{
			symbol.ThrowOnNull(nameof (symbol));
			syntaxNode.ThrowOnNull(nameof (syntaxNode));
			semanticModel.ThrowOnNull(nameof (semanticModel));
			pxContext.ThrowOnNull(nameof (pxContext));

			var containingType = symbol.ContainingType?.OriginalDefinition;

			if (containingType != null)
			{
				switch (symbol.Name)
				{
					// PXGraph.Actions.PressSave
					case PressSaveMethodName when containingType.InheritsFromOrEquals(pxContext.PXActionCollection):
						return SaveOperationKind.PressSave;
					// PXSave.press
					case PressMethodName when containingType.InheritsFromOrEquals(pxContext.PXAction.Type):
						var walker = new SavePressWalker(semanticModel, pxContext);
						syntaxNode.Accept(walker);
						return walker.Found ? SaveOperationKind.PressSave : SaveOperationKind.None;
					// PXGraph.Persist
					case GraphPersistMethodName when containingType.IsPXGraph():
						return SaveOperationKind.GraphPersist;
					// PXCache.Persist / PXCache.PersistInserted / PXCache.PersistUpdated / PXCache.PersistDeleted
					case CachePersistMethodName:
					case CachePersistInsertedMethodName:
					case CachePersistUpdatedMethodName:
					case CachePersistDeletedMethodName:
						if (containingType.InheritsFromOrEquals(pxContext.PXCache.Type))
							return SaveOperationKind.CachePersist;
						break;
				}
			}

			return SaveOperationKind.None;
		}

		public static PXDatabaseKind GetPXDatabaseSaveOperationKind(IMethodSymbol symbol, PXContext pxContext)
		{
			symbol.ThrowOnNull(nameof(symbol));
			pxContext.ThrowOnNull(nameof(pxContext));

			var containingType = symbol.ContainingType?.OriginalDefinition;

			if (containingType != null && 
			    containingType.InheritsFromOrEquals(pxContext.PXDatabase.Type))
			{
				if (string.Equals(symbol.Name, DelegateNames.Insert))
					return PXDatabaseKind.Insert;
				else if (string.Equals(symbol.Name, DelegateNames.Delete))
					return PXDatabaseKind.Delete;
				else if (string.Equals(symbol.Name, DelegateNames.Update))
					return PXDatabaseKind.Update;
			}
			return PXDatabaseKind.None;
		}

		private class SavePressWalker : CSharpSyntaxWalker
		{
			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;

			public SavePressWalker(SemanticModel semanticModel, PXContext pxContext)
			{
				semanticModel.ThrowOnNull(nameof (semanticModel));
				pxContext.ThrowOnNull(nameof (pxContext));

				_semanticModel = semanticModel;
				_pxContext = pxContext;
			}

			public bool Found { get; private set; }

			public override void VisitIdentifierName(IdentifierNameSyntax node)
			{
				var typeInfo = _semanticModel.GetTypeInfo(node);
				if (typeInfo.Type?.OriginalDefinition != null && typeInfo.Type.OriginalDefinition.TypeKind != TypeKind.Error
				    && typeInfo.Type.OriginalDefinition.InheritsFromOrEquals(_pxContext.PXSystemActions.PXSave))
				{
					Found = true;
				}
			}
		}
		
		private class TransactionOpenWalker : CSharpSyntaxWalker
		{
			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;

			public TransactionOpenWalker(SemanticModel semanticModel, PXContext pxContext)
			{
				semanticModel.ThrowOnNull(nameof(semanticModel));
				pxContext.ThrowOnNull(nameof(pxContext));

				_semanticModel = semanticModel;
				_pxContext = pxContext;
			}

			public override void Visit(SyntaxNode node) => base.Visit(node);

			public bool TransactionOpened { get; private set; }

			public override void VisitIfStatement(IfStatementSyntax node)
			{
				TransactionOpened = true;
				base.VisitIfStatement(node);
			}
		}
	}
}
