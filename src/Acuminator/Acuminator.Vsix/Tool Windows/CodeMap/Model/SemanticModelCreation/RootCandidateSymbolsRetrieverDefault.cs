using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Default implementation for <see cref="IRootCandidateSymbolsRetriever"/> interface.
	/// </summary>
	public class RootCandidateSymbolsRetrieverDefault : IRootCandidateSymbolsRetriever
	{
		/// <summary>
		/// Get the code map root candidate symbol + node pairs.
		/// </summary>
		/// <param name="treeRoot">The tree root.</param>
		/// <param name="context">The context.</param>
		/// <param name="semanticModel">The semantic model.</param>
		/// <param name="cancellationToken">(Optional) A token that allows processing to be cancelled.</param>
		/// <returns/>
		public IEnumerable<(INamedTypeSymbol RootSymbol, SyntaxNode RootNode)> GetCodeMapRootCandidates(SyntaxNode treeRoot, PXContext context,
																										SemanticModel semanticModel,
																										CancellationToken cancellationToken = default)
		{
			treeRoot.ThrowOnNull(nameof(treeRoot));
			context.ThrowOnNull(nameof(context));
			semanticModel.ThrowOnNull(nameof(semanticModel));

			cancellationToken.ThrowIfCancellationRequested();
			return GetDeclaredCodeMapRoots(treeRoot, context, semanticModel, cancellationToken);
		}

		protected virtual IEnumerable<(INamedTypeSymbol RootSymbol, SyntaxNode RootNode)> GetDeclaredCodeMapRoots(SyntaxNode treeRoot, PXContext context,
																												  SemanticModel semanticModel,
																												  CancellationToken cancellationToken)
		{
			var declaredClasses = treeRoot.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>();

			foreach (ClassDeclarationSyntax classNode in declaredClasses)
			{
				cancellationToken.ThrowIfCancellationRequested();
				INamedTypeSymbol classTypeSymbol = GetTypeSymbolFromClassDeclaration(classNode, semanticModel, cancellationToken);

				if (classTypeSymbol != null && IsRootCandidate(classTypeSymbol, context))
				{
					yield return (classTypeSymbol, classNode);
				}
			}
		}

		protected virtual bool IsRootCandidate(INamedTypeSymbol classTypeSymbol, PXContext context) =>
			classTypeSymbol.IsPXGraphOrExtension(context) ||
			classTypeSymbol.IsDacOrExtension(context);

		protected INamedTypeSymbol GetTypeSymbolFromClassDeclaration(ClassDeclarationSyntax classNode, SemanticModel semanticModel,
																	 CancellationToken cancellationToken)
		{
			var typeSymbol = semanticModel.GetDeclaredSymbol(classNode, cancellationToken) as INamedTypeSymbol;

			if (typeSymbol != null)
				return typeSymbol;

			SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(classNode, cancellationToken);
			typeSymbol = symbolInfo.Symbol as INamedTypeSymbol;

			if (typeSymbol == null && symbolInfo.CandidateSymbols.Length == 1)
			{
				typeSymbol = symbolInfo.CandidateSymbols[0] as INamedTypeSymbol;
			}

			return typeSymbol;
		}
	}
}
