#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	internal class LongOperationDelegateTypeClassifier
	{
		public LongOperationDelegateType? GetLongOperationDelegateType(InvocationExpressionSyntax? longOperationSetupMethodInvocationNode,
																	   SemanticModel? semanticModel, PXContext pxContext, 
																	   CancellationToken cancellationToken)
		{
			if (semanticModel == null)
				return null;

			cancellationToken.ThrowIfCancellationRequested();
			string? methodName = null;

			switch (longOperationSetupMethodInvocationNode?.Expression)
			{
				case MemberAccessExpressionSyntax memberAccessNode
				when memberAccessNode.OperatorToken.IsKind(SyntaxKind.DotToken):
					methodName = memberAccessNode.Name?.ToString();
					return GetLongOperationDelegateTypeFromMethodAccessNode(semanticModel, pxContext, memberAccessNode, methodName, cancellationToken);

				case MemberBindingExpressionSyntax memberBindingNode
				when memberBindingNode.OperatorToken.IsKind(SyntaxKind.DotToken):
					methodName = memberBindingNode.Name?.ToString();
					return GetLongOperationDelegateTypeFromMethodAccessNode(semanticModel, pxContext, memberBindingNode, methodName, cancellationToken);

				default:
					return null;
			}
		}

		private static LongOperationDelegateType? GetLongOperationDelegateTypeFromMethodAccessNode(SemanticModel semanticModel, PXContext pxContext,
																								   ExpressionSyntax methodAccessNode, string? methodName,
																								   CancellationToken cancellationToken)
		{
			switch (methodName)
			{
				case DelegateNames.SetProcess:
					var setDelegateSymbol = semanticModel.GetSymbolInfo(methodAccessNode, cancellationToken).Symbol as IMethodSymbol;

					if (setDelegateSymbol != null && setDelegateSymbol.ContainingType.ConstructedFrom.InheritsFromOrEquals(pxContext.PXProcessingBase.Type))
						return LongOperationDelegateType.ProcessingDelegate;

					return null;

				case DelegateNames.StartOperation:
					var longRunDelegate = semanticModel.GetSymbolInfo(methodAccessNode, cancellationToken).Symbol as IMethodSymbol;

					if (longRunDelegate != null && longRunDelegate.IsStatic && longRunDelegate.DeclaredAccessibility == Accessibility.Public &&
						pxContext.PXLongOperation.Equals(longRunDelegate.ContainingType))
					{
						return LongOperationDelegateType.LongRunDelegate;
					}

					return null;

				default:
					return null;
			}

		}
	}
}