﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
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
		private const string CachePersistInserted = "PersistInserted"; // PXCache.PersistInserted
		private const string CachePersistUpdated = "PersistUpdated"; // PXCache.PersistUpdated
		private const string CachePersistDeleted = "PersistDeleted"; // PXCache.PersistDeleted

		public static SaveOperationKind GetSaveOperationKind(IMethodSymbol symbol, InvocationExpressionSyntax syntaxNode, 
			SemanticModel semanticModel, PXContext pxContext)
		{
			symbol.ThrowOnNull(nameof (symbol));
			syntaxNode.ThrowOnNull(nameof (syntaxNode));
			semanticModel.ThrowOnNull(nameof (semanticModel));
			pxContext.ThrowOnNull(nameof (pxContext));

			if (symbol.ContainingType != null)
			{
				// PXGraph.Actions.PressSave or PXSave.Press
				if (String.Equals(symbol.Name, PressSaveMethodName, StringComparison.Ordinal)
				    && symbol.ContainingType.OriginalDefinition.InheritsFromOrEquals(pxContext.PXActionCollection))
				{
					return SaveOperationKind.PressSave;
				}

				if (String.Equals(symbol.Name, PressMethodName, StringComparison.Ordinal)
				    && symbol.ContainingType.OriginalDefinition.InheritsFromOrEquals(pxContext.PXActionType))
				{
					var walker = new SavePressWalker(semanticModel, pxContext);
					syntaxNode.Accept(walker);
					return walker.Found ? SaveOperationKind.PressSave : SaveOperationKind.None;
				}

				// PXGraph.Persist
				if (String.Equals(symbol.Name, GraphPersistMethodName, StringComparison.Ordinal)
				    && symbol.ContainingType.OriginalDefinition.IsPXGraph())
				{
					return SaveOperationKind.GraphPersist;
				}
					
				// PXCache.Persist / PXCache.PersistInserted / PXCache.PersistUpdated / PXCache.PersistDeleted
				if ((String.Equals(symbol.Name, CachePersistMethodName, StringComparison.Ordinal)
					|| String.Equals(symbol.Name, CachePersistInserted, StringComparison.Ordinal)
					|| String.Equals(symbol.Name, CachePersistUpdated, StringComparison.Ordinal)
					|| String.Equals(symbol.Name, CachePersistDeleted, StringComparison.Ordinal))
					&& symbol.ContainingType.OriginalDefinition.InheritsFromOrEquals(pxContext.PXCacheType))
				{
					return SaveOperationKind.CachePersist;
				}
			}

			return SaveOperationKind.None;
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
	}
}
