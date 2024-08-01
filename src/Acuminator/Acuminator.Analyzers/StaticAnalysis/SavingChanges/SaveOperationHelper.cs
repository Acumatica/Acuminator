#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;
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
			symbol.ThrowOnNull();
			syntaxNode.ThrowOnNull();
			semanticModel.ThrowOnNull();
			pxContext.ThrowOnNull();

			var containingType = symbol.ContainingType?.OriginalDefinition;

			if (containingType != null)
			{
				switch (symbol.Name)
				{
					// PXGraph.Actions.PressSave
					case PressSaveMethodName when containingType.InheritsFromOrEquals(pxContext.PXActionCollection):
						return SaveOperationKind.PressSave;
					// PXSave.press
					case PressMethodName when containingType.InheritsFromOrEquals(pxContext.PXAction.Type!):
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
						if (containingType.InheritsFromOrEquals(pxContext.PXCache.Type!))
							return SaveOperationKind.CachePersist;
						break;
				}
			}

			return SaveOperationKind.None;
		}

		public static PXDBOperationKind GetPXDatabaseSaveOperationKind(IMethodSymbol symbol, PXContext pxContext)
		{
			symbol.ThrowOnNull();
			pxContext.ThrowOnNull();

			var containingType = symbol.ContainingType?.OriginalDefinition;

			if (containingType != null && 
			    containingType.InheritsFromOrEquals(pxContext.PXDatabase.Type!))
			{
				return symbol.Name switch
				{
					DelegateNames.Insert => PXDBOperationKind.Insert,
					DelegateNames.Delete => PXDBOperationKind.Delete,
					DelegateNames.Update => PXDBOperationKind.Update,
					DelegateNames.Ensure => PXDBOperationKind.Ensure,
					_					 => PXDBOperationKind.None
				};
			}

			return PXDBOperationKind.None;
		}

		private class SavePressWalker : CSharpSyntaxWalker
		{
			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;

			public SavePressWalker(SemanticModel semanticModel, PXContext pxContext)
			{
				_semanticModel = semanticModel.CheckIfNull();
				_pxContext	   = pxContext.CheckIfNull();
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
	}
}
