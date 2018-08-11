using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ConnectionScopeInRowSelectingAnalyzer : PXDiagnosticAnalyzer
	{
		private class Walker : CSharpSyntaxWalker
		{
			private class PXConnectionScopeVisitor : CSharpSyntaxVisitor<bool>
			{
				private SymbolAnalysisContext _context;
				private readonly PXContext _pxContext;
				private readonly SemanticModel _semanticModel;

				public PXConnectionScopeVisitor(SymbolAnalysisContext context, PXContext pxContext, SemanticModel semanticModel)
				{
					pxContext.ThrowOnNull(nameof(pxContext));
					semanticModel.ThrowOnNull(nameof(semanticModel));

					_context = context;
					_pxContext = pxContext;
					_semanticModel = semanticModel;
				}

				public override bool VisitUsingStatement(UsingStatementSyntax node)
				{
					return (node.Declaration?.Accept(this) ?? false) || (node.Expression?.Accept(this) ?? false);
				}

				public override bool VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
				{
					var symbolInfo = _semanticModel.GetSymbolInfo(node.Type);
					return symbolInfo.Symbol?.OriginalDefinition != null 
						&& symbolInfo.Symbol.OriginalDefinition.Equals(_pxContext.PXConnectionScope);
				}
			}

			private static readonly IEnumerable<string> MethodPrefixes = new[] { "Select", "Search" };
			private const int MaxDepth = 5;

			private SymbolAnalysisContext _context;
			private readonly PXContext _pxContext;
			private readonly SemanticModel _semanticModel;
			private bool _insideConnectionScope;
			private readonly Stack<bool> _externalCall = new Stack<bool>();
			private bool _found;
			private readonly PXConnectionScopeVisitor _connectionScopeVisitor;

			public Walker(SymbolAnalysisContext context, PXContext pxContext, SemanticModel semanticModel)
			{
				pxContext.ThrowOnNull(nameof (pxContext));
				semanticModel.ThrowOnNull(nameof (semanticModel));

				_context = context;
				_pxContext = pxContext;
				_semanticModel = semanticModel;

				_connectionScopeVisitor = new PXConnectionScopeVisitor(context, pxContext, semanticModel);
			}

			public override void VisitUsingStatement(UsingStatementSyntax node)
			{
				_context.CancellationToken.ThrowIfCancellationRequested();

				if (_insideConnectionScope)
				{
					base.VisitUsingStatement(node);
				}
				else
				{
					_insideConnectionScope = node.Accept(_connectionScopeVisitor);
					base.VisitUsingStatement(node);
					_insideConnectionScope = false;
				}
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				_context.CancellationToken.ThrowIfCancellationRequested();

				if (_insideConnectionScope || _found || _externalCall.Count >= MaxDepth)
					return;

				var symbolInfo = _semanticModel.GetSymbolInfo(node);
				var methodSymbol = GetMethodSymbol(symbolInfo);

				if (methodSymbol != null)
				{
					if (IsDatabaseCall(methodSymbol))
					{
						if (_externalCall.Count > 0)
							_found = true; // diagnostic will be reported on the original node in RowSelecting
						else
							ReportDiagnostic(node);
					}
					else
					{
						// Check calls to other methods
						var externalMethodNode = methodSymbol.GetSyntax(_context.CancellationToken) as MethodDeclarationSyntax;
						_externalCall.Push(true);
						externalMethodNode?.Accept(this);
						_externalCall.Pop();

						if (_found)
						{
							if (_externalCall.Count > 0) return;

							ReportDiagnostic(node);
							_found = false;
						}
					}
				}

				base.VisitInvocationExpression(node);
			}

			private IMethodSymbol GetMethodSymbol(SymbolInfo symbolInfo)
			{
				if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
				{
					return methodSymbol;
				}
				if (!symbolInfo.CandidateSymbols.IsEmpty)
				{
					return symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
				}

				return null;
			}

			private bool IsDatabaseCall(IMethodSymbol candidate)
			{
				var containingType = candidate.ContainingType?.OriginalDefinition;
				return MethodPrefixes.Any(p => candidate.Name.StartsWith(p, StringComparison.Ordinal))
				       && containingType != null
				       && (containingType.InheritsFromOrEqualsGeneric(_pxContext.PXSelectBaseType)
				           || containingType.InheritsFromOrEquals(_pxContext.PXViewType)
				           || containingType.InheritsFromOrEquals(_pxContext.PXSelectorAttribute)
				           || containingType.Equals(_pxContext.PXDatabase));
			}

			private void ReportDiagnostic(SyntaxNode node)
			{
				_context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1042_ConnectionScopeInRowSelecting,
					node.GetLocation()));
			}
		}

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1042_ConnectionScopeInRowSelecting);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzeMethod(c, pxContext), SymbolKind.Method);
		}

		private void AnalyzeMethod(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol) context.Symbol;
			
			if (methodSymbol != null && IsRowSelectingMethod(methodSymbol, pxContext))
			{
				var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as MethodDeclarationSyntax;
				if (methodSyntax != null)
				{
					var semanticModel = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);
					methodSyntax.Accept(new Walker(context, pxContext, semanticModel));
				}
			}
		}

		private bool IsRowSelectingMethod(IMethodSymbol symbol, PXContext pxContext)
		{
			if (symbol.ReturnsVoid && symbol.TypeParameters.IsEmpty && !symbol.Parameters.IsEmpty)
			{
				// Loosely check method signature because sometimes business logic from event handler calls is extracted to a separate method

				// New generic event syntax
				if (symbol.Parameters[0].Type.OriginalDefinition.Equals(pxContext.Events.RowSelecting))
					return true;

				// Old syntax
				if (symbol.Parameters.Length >= 2
				    && symbol.Parameters[0].Type.OriginalDefinition.InheritsFromOrEquals(pxContext.PXCacheType)
				    && symbol.Parameters[1].Type.OriginalDefinition.InheritsFromOrEquals(pxContext.Events.PXRowSelectingEventArgs))
					return true;
			}

			
			return false;
		}
	}
}
