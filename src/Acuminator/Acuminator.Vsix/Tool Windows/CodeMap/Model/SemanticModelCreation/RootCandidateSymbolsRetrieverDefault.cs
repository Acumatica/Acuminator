using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Default implementation for <see cref="IRootCandidateSymbolsRetriever"/> interface.
	/// </summary>
	public class RootCandidateSymbolsRetrieverDefault : IRootCandidateSymbolsRetriever
	{
		public IEnumerable<INamedTypeSymbol> GetCodeMapRootCandidates(SyntaxNode treeRoot, PXContext context, 
																CancellationToken cancellationToken = default)
		{
			treeRoot.ThrowOnNull(nameof(treeRoot));
			context.ThrowOnNull(nameof(context));

			cancellationToken.ThrowIfCancellationRequested();


		}

		protected  IEnumerable<(ITypeSymbol RootSymbol, SyntaxNode RootNode)> GetDeclaredCodeMapRoots(
																						SyntaxNode syntaxTreeRoot, SemanticModel semanticModel,
																						PXContext context, CancellationToken cancellationToken = default)
		{
			if (!context.CheckIfNull(nameof(context)).IsPlatformReferenced)
				return Enumerable.Empty<(ITypeSymbol, SyntaxNode)>();

			syntaxTreeRoot.ThrowOnNull(nameof(syntaxTreeRoot));
			semanticModel.ThrowOnNull(nameof(semanticModel));
			cancellationToken.ThrowIfCancellationRequested();

			return GetDeclaredGraphsAndExtensionsImpl();


			IEnumerable<(ITypeSymbol GraphSymbol, SyntaxNode GraphNode)> GetDeclaredGraphsAndExtensionsImpl()
			{
				var declaredClasses = syntaxTreeRoot.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>();

				foreach (ClassDeclarationSyntax classNode in declaredClasses)
				{
					ITypeSymbol classTypeSymbol = classNode.GetTypeSymbolFromClassDeclaration(semanticModel, cancellationToken);

					if (classTypeSymbol != null && classTypeSymbol.IsPXGraphOrExtension(context))
					{
						yield return (classTypeSymbol, classNode);
					}
				}
			}
		}

		protected ITypeSymbol GetTypeSymbolFromClassDeclaration(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel,
																	CancellationToken cancellationToken = default)
		{
			classDeclaration.ThrowOnNull(nameof(classDeclaration));
			semanticModel.ThrowOnNull(nameof(semanticModel));
			cancellationToken.ThrowIfCancellationRequested();

			var typeSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) as ITypeSymbol;

			if (typeSymbol != null)
				return typeSymbol;

			SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(classDeclaration, cancellationToken);
			typeSymbol = symbolInfo.Symbol as ITypeSymbol;

			if (typeSymbol == null && symbolInfo.CandidateSymbols.Length == 1)
			{
				typeSymbol = symbolInfo.CandidateSymbols[0] as ITypeSymbol;
			}

			return typeSymbol;
		}
	}
}
