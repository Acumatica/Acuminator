using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;
using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public static class CodeMapSyntaxUtils
	{
		public static IEnumerable<(ITypeSymbol RootSymbol, SyntaxNode RootNode)> GetDeclaredCodeMapRoots(
																						this SyntaxNode syntaxTreeRoot, SemanticModel semanticModel,
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

		public static ITypeSymbol GetTypeSymbolFromClassDeclaration(this ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel,
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
