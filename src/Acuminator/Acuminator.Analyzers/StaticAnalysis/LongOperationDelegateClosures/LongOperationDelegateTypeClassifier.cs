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
	internal static class LongOperationDelegateTypeClassifier
	{
		public static (LongOperationDelegateType Type, IMethodSymbol Method)? GetLongOperationDelegateInfo(
																				InvocationExpressionSyntax? longOperationSetupMethodInvocationNode,
																				SemanticModel? semanticModel, PXContext pxContext, 
																				CancellationToken cancellationToken)
		{
			if (semanticModel == null)
				return null;

			cancellationToken.ThrowIfCancellationRequested();
			string? methodName;

			switch (longOperationSetupMethodInvocationNode?.Expression)
			{
				case MemberAccessExpressionSyntax memberAccessNode
				when memberAccessNode.OperatorToken.IsKind(SyntaxKind.DotToken):
					methodName = memberAccessNode.Name?.Identifier.ValueText;
					return GetLongOperationDelegateInfoFromMethodAccessNode(semanticModel, pxContext, memberAccessNode, methodName, cancellationToken);

				case MemberBindingExpressionSyntax memberBindingNode
				when memberBindingNode.OperatorToken.IsKind(SyntaxKind.DotToken):
					methodName = memberBindingNode.Name?.Identifier.ValueText;
					return GetLongOperationDelegateInfoFromMethodAccessNode(semanticModel, pxContext, memberBindingNode, methodName, cancellationToken);

				default:
					return null;
			}
		}

		private static (LongOperationDelegateType Type, IMethodSymbol Method)? GetLongOperationDelegateInfoFromMethodAccessNode(SemanticModel semanticModel, 
																									PXContext pxContext, ExpressionSyntax methodAccessNode, 
																									string? methodName, CancellationToken cancellationToken)
		{
			switch (methodName)
			{
				case DelegateNames.Processing.SetProcessDelegate:
				case DelegateNames.Processing.SetAsyncProcessDelegate:
					var setDelegateSymbol = semanticModel.GetSymbolOrBestCandidate(methodAccessNode, cancellationToken) as IMethodSymbol;

					if (setDelegateSymbol != null && setDelegateSymbol.ContainingType.ConstructedFrom.InheritsFromOrEquals(pxContext.PXProcessingBase.Type))
						return (LongOperationDelegateType.ProcessingDelegate, setDelegateSymbol);

					return null;

				case DelegateNames.Async.StartOperation:
				case DelegateNames.Async.StartAsyncOperation:
				case DelegateNames.Async.Await:
					var longRunDelegate = semanticModel.GetSymbolOrBestCandidate(methodAccessNode, cancellationToken) as IMethodSymbol;

					if (longRunDelegate == null)
						return null;

					return pxContext.AsyncOperations.AllMethodsStartingLongRun.Contains(longRunDelegate.OriginalDefinition)
						? (LongOperationDelegateType.LongRunDelegate, longRunDelegate)
						: null;

				default:
					return null;
			}
		}
	}
}