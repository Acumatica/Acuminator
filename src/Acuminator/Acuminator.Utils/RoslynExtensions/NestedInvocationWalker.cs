using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities;
using CommonServiceLocator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utils.RoslynExtensions
{
	/// <summary>
	/// Syntax walker that follows method invocations, property getters, etc.,
	/// and analyzes corresponding symbols in a recursive manner.
	/// </summary>
	/// <remarks>
	/// Please note that it doesn't analyze symbols which don't have any source code available.
	/// </remarks>
	/// <example>
	///	<code title="Example">
	/// string descr = SomeHelper.GetDescription(); // this code is being analyzed
	/// ...
	/// // In some other file or even in a different assembly
	/// public static class SomeHelper
	/// {
	///		public static void GetDescription()
	///		{
	///			var graph = new PXGraph(); // this code will be analyzed too
	///		}
	/// }
	///	</code>
	/// </example>
	// ReSharper disable once InheritdocConsiderUsage
	public class NestedInvocationWalker : CSharpSyntaxWalker
	{
		private const int MaxDepth = 100; // to avoid circular dependencies

		private readonly SemanticModel _semanticModel;
		private CancellationToken _cancellationToken;

		private SyntaxNode _currentNode;
		private SyntaxNode _originalNode;

		private readonly CodeAnalysisSettings _settings;

		/// <summary>
		/// Syntax node in the original tree that is being analyzed.
		/// Typically it is the node on which a diagnostic should be reported.
		/// </summary>
		protected SyntaxNode OriginalNode => _originalNode ?? _currentNode;

		private readonly Stack<SyntaxNode> _nodesStack = new Stack<SyntaxNode>();

		public NestedInvocationWalker(SemanticModel semanticModel, CancellationToken cancellationToken)
		{
			semanticModel.ThrowOnNull(nameof (semanticModel));

			_semanticModel = semanticModel;
			_cancellationToken = cancellationToken;

			try
			{
				if (ServiceLocator.IsLocationProviderSet)
					_settings = ServiceLocator.Current.GetInstance<CodeAnalysisSettings>();
			}
			catch
			{
				// TODO: log the exception
			}

			if (_settings == null)
				_settings = CodeAnalysisSettings.Default;
		}

		protected void ThrowIfCancellationRequested()
		{
			_cancellationToken.ThrowIfCancellationRequested();
		}

		/// <summary>
		/// Returns a symbol for an invocation expression, or, 
		/// if the exact symbol cannot be found, returns the first candidate.
		/// </summary>
		protected IMethodSymbol GetMethodSymbol(InvocationExpressionSyntax node)
		{
			var symbolInfo = _semanticModel.GetSymbolInfo(node, _cancellationToken);

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

		private void Push(SyntaxNode node)
		{
			if (_nodesStack.Count == 0)
				_originalNode = node;

			_nodesStack.Push(node);
		}

		private void Pop()
		{
			_nodesStack.Pop();

			if (_nodesStack.Count == 0)
				_originalNode = null;
		}

		private bool RecursiveAnalysisEnabled()
		{
			return _settings.RecursiveAnalysisEnabled && _nodesStack.Count <= MaxDepth;
		}

		#region Visit

		public override void DefaultVisit(SyntaxNode node)
		{
			_currentNode = node;
			base.DefaultVisit(node);
			_currentNode = null;
		}

		public override void VisitInvocationExpression(InvocationExpressionSyntax node)
		{
			ThrowIfCancellationRequested();

			if (RecursiveAnalysisEnabled())
			{
				var methodSymbol = GetMethodSymbol(node);

				var externalMethodNode = methodSymbol?.GetSyntax(_cancellationToken) as MethodDeclarationSyntax;
				if (externalMethodNode != null)
				{
					Push(node);
					externalMethodNode.Accept(this);
					Pop();
				}
			}

			base.VisitInvocationExpression(node);
		}

		#endregion
	}
}
