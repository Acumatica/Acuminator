#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn
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
	public abstract class NestedInvocationWalker : CSharpSyntaxWalker
	{
		private const int MaxDepth = 100; // to avoid circular dependencies

		/// <summary>
		/// Acumatica specific context with compilation, settings and Acumatica-specific symbol collections.
		/// </summary>
		protected PXContext PxContext { get; }

		private Compilation Compilation => PxContext.Compilation;

		protected CodeAnalysisSettings Settings => PxContext.CodeAnalysisSettings;

		private readonly Dictionary<SyntaxTree, SemanticModel> _semanticModels = new Dictionary<SyntaxTree, SemanticModel>();

		private readonly ISet<(SyntaxNode, DiagnosticDescriptor)> _reportedDiagnostics = new HashSet<(SyntaxNode, DiagnosticDescriptor)>();

        /// <summary>
        /// Cancellation token
        /// </summary>
        protected CancellationToken CancellationToken { get; }

        /// <summary>
        /// Syntax node in the original tree that is being analyzed.
        /// Typically it is the node on which a diagnostic should be reported.
        /// </summary>
        protected SyntaxNode? OriginalNode { get; private set; }

		private readonly Stack<SyntaxNode> _nodesStack = new Stack<SyntaxNode>();
        private readonly HashSet<IMethodSymbol> _methodsInStack = new HashSet<IMethodSymbol>();

		private readonly Lazy<HashSet<INamedTypeSymbol>> _typesToBypass;
		private readonly Func<IMethodSymbol, bool>? _extraBypassCheck;

		/// <summary>
		/// Constructor of the class.
		/// </summary>
		/// <param name="pxContext">Acumatica specific context with compilation, settings and Acumatica-specific symbol collections.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <param name="bypassMethod">(Optional) An optional delegate to control if it is needed to bypass analysis of an invocation of a method and do not step into it. </param>
		protected NestedInvocationWalker(PXContext pxContext, CancellationToken cancellationToken, Func<IMethodSymbol, bool>? extraBypassCheck = null)
		{
			pxContext.ThrowOnNull(nameof (pxContext));

			PxContext = pxContext;
            CancellationToken = cancellationToken;
			_extraBypassCheck = extraBypassCheck;

			//Use lazy to avoid calling virtual methods inside the constructor
			_typesToBypass = new Lazy<HashSet<INamedTypeSymbol>>(valueFactory: GetTypesToBypass, isThreadSafe: false);
		}

		/// <summary>
		/// Gets types to bypass. The alker won't go into their type members.
		/// </summary>
		/// <returns>
		/// The types to bypass.
		/// </returns>
		/// <remarks>
		/// some core types from PX.Data namespace
		/// </remarks>
		protected virtual HashSet<INamedTypeSymbol> GetTypesToBypass() =>
			new HashSet<INamedTypeSymbol>()
			{
				PxContext.PXGraph.Type,
				PxContext.PXView.Type,
				PxContext.PXCache.Type,
				PxContext.PXCache.GenericType,
				PxContext.PXAction.Type,
				PxContext.PXSelectBaseGeneric.Type,
				PxContext.PXAdapterType,
				PxContext.PXDatabase.Type
			};

		protected void ThrowIfCancellationRequested() => CancellationToken.ThrowIfCancellationRequested();

		/// <summary>
		/// Returns a symbol for an invocation expression, or, 
		/// if the exact symbol cannot be found, returns the first candidate.
		/// </summary>
		protected virtual T? GetSymbol<T>(ExpressionSyntax node)
			where T : class, ISymbol
		{
			var semanticModel = GetSemanticModel(node.SyntaxTree);

			if (semanticModel != null)
			{
				var symbolInfo = semanticModel.GetSymbolInfo(node, CancellationToken);

				if (symbolInfo.Symbol is T symbol)
				{
					return symbol;
				}

				if (!symbolInfo.CandidateSymbols.IsEmpty)
				{
					return symbolInfo.CandidateSymbols.OfType<T>().FirstOrDefault();
				}
			}

			return null;
		}

		protected virtual SemanticModel? GetSemanticModel(SyntaxTree syntaxTree)
		{
			if (!Compilation.ContainsSyntaxTree(syntaxTree))
				return null;

			if (_semanticModels.TryGetValue(syntaxTree, out var semanticModel))
				return semanticModel;

			semanticModel = Compilation.GetSemanticModel(syntaxTree);
			_semanticModels[syntaxTree] = semanticModel;
			return semanticModel;
		}

		/// <summary>
		/// Reports a diagnostic for the provided descriptor on the original syntax node from which recursive analysis started.
		/// This method must be used for all diagnostic reporting within the walker
		/// because it does diagnostic deduplication and determine the right syntax node to perform diagnostic reporting.
		/// </summary>
		/// <param name="reportDiagnostic">Action that reports a diagnostic in the current context (e.g., <code>SymbolAnalysisContext.ReportDiagnostic</code>)</param>
		/// <param name="diagnosticDescriptor">Diagnostic descriptor</param>
		/// <param name="node">Current syntax node that is being analyzed. Diagnostic will be reported on the original node.</param>
		/// <param name="messageArgs">Arguments to the message of the diagnostic</param>
		/// <remarks>This method takes a report diagnostic method as a parameter because it is different for each analyzer type 
		/// (<code>SymbolAnalysisContext.ReportDiagnostic</code>, <code>SyntaxNodeAnalysisContext.ReportDiagnostic</code>, etc.)</remarks>
		protected virtual void ReportDiagnostic(Action<Diagnostic> reportDiagnostic, DiagnosticDescriptor diagnosticDescriptor,
												SyntaxNode node, params object[] messageArgs)
		{
			var nodeToReport = OriginalNode ?? node;
			var diagnosticKey = (nodeToReport, diagnosticDescriptor);

			if (!_reportedDiagnostics.Contains(diagnosticKey))
			{
				var diagnostic = Diagnostic.Create(diagnosticDescriptor, nodeToReport.GetLocation(), messageArgs);
				var semanticModel = GetSemanticModel(nodeToReport.SyntaxTree);

				SuppressionManager.ReportDiagnosticWithSuppressionCheck(semanticModel, reportDiagnostic, diagnostic, Settings, CancellationToken);
				_reportedDiagnostics.Add(diagnosticKey);
			}
		}

		private bool RecursiveAnalysisEnabled() => Settings.RecursiveAnalysisEnabled && _nodesStack.Count <= MaxDepth;

		#region Visit

		public override void VisitInvocationExpression(InvocationExpressionSyntax node)
		{
			ThrowIfCancellationRequested();

			if (RecursiveAnalysisEnabled() && node.Parent?.Kind() != SyntaxKind.ConditionalAccessExpression)
			{
				var methodSymbol = GetSymbol<IMethodSymbol>(node);
				VisitCalledMethod(methodSymbol, node);
			}

			base.VisitInvocationExpression(node);
		}

		public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
		{
			VisitPropertyOrIndexerAccessExpression(node);
			base.VisitMemberAccessExpression(node);
		}

		public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
		{
			VisitPropertyOrIndexerAccessExpression(node);
			base.VisitElementAccessExpression(node);
		}

		private void VisitPropertyOrIndexerAccessExpression(ExpressionSyntax node)
		{
			ThrowIfCancellationRequested();

			if (RecursiveAnalysisEnabled() && node.Parent != null
				&& !node.Parent.IsKind(SyntaxKind.ObjectInitializerExpression)
				&& !(node.Parent is AssignmentExpressionSyntax))
			{
				var propertySymbol = GetSymbol<IPropertySymbol>(node);
				VisitCalledMethod(propertySymbol?.GetMethod, node);
			}
		}

		public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
		{
			ThrowIfCancellationRequested();

			if (RecursiveAnalysisEnabled())
			{
				var propertySymbol = GetSymbol<IPropertySymbol>(node.Left);
				VisitCalledMethod(propertySymbol?.SetMethod, node);
			}

			base.VisitAssignmentExpression(node);
		}

		public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
		{
			ThrowIfCancellationRequested();

			if (RecursiveAnalysisEnabled())
			{
				var methodSymbol = GetSymbol<IMethodSymbol>(node);
				VisitCalledMethod(methodSymbol, node);
			}

			base.VisitObjectCreationExpression(node);
		}

		public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
		{
			ThrowIfCancellationRequested();

			if (RecursiveAnalysisEnabled() && node.WhenNotNull != null)
			{
				var propertySymbol = GetSymbol<IPropertySymbol>(node.WhenNotNull);
				var methodSymbol = propertySymbol != null 
					? propertySymbol.GetMethod 
					: GetSymbol<IMethodSymbol>(node.WhenNotNull);

				VisitCalledMethod(methodSymbol, node);
			}

			base.VisitConditionalAccessExpression(node);
		}

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
        }

		public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
		{
		}

		private void VisitCalledMethod(IMethodSymbol? calledMethod, SyntaxNode callSite)
		{
			if (calledMethod == null || IsMethodInStack(calledMethod) || !(calledMethod.GetSyntax(CancellationToken) is CSharpSyntaxNode calledMethodNode))
				return;

			bool wasVisited = false;
			BeforeBypassCheck(calledMethod, calledMethodNode, callSite);

			if (!BypassMethod(calledMethod, calledMethodNode, callSite))
			{
				BeforeRecursiveVisit(calledMethod, calledMethodNode, callSite);

				Push(callSite, calledMethod);
				calledMethodNode.Accept(this);
				Pop(calledMethod);

				wasVisited = true;
			}

			AfterRecursiveVisit(calledMethod, calledMethodNode, callSite, wasVisited);
		}

        private bool IsMethodInStack(IMethodSymbol calledMethod) =>
			_methodsInStack.Contains(calledMethod);

		/// <summary>
		/// Extensibility point that allows to add some logic executed before <paramref name="calledMethod"/> is checked by the bypass check <see cref="BypassMethod(IMethodSymbol)"/>.
		/// </summary>
		/// <param name="calledMethod">The called method symbol.</param>
		/// <param name="calledMethodNode">The called method node.</param>
		/// <param name="callSite">Syntax node representing the call site into which the walker steps in.</param>
		protected virtual void BeforeBypassCheck(IMethodSymbol calledMethod, CSharpSyntaxNode calledMethodNode, SyntaxNode callSite)
		{
		}

		/// <summary>
		/// Extensibility point that allows to add some logic executed after the bypass check on a <paramref name="calledMethod"/> that passed it and 
		/// just before the visit of the <paramref name="calledMethodNode"/>.
		/// </summary>
		/// <param name="calledMethod">The called method symbol.</param>
		/// <param name="calledMethodNode">The called method node.</param>
		/// <param name="callSite">Syntax node representing the call site into which the walker steps in.</param>
		protected virtual void BeforeRecursiveVisit(IMethodSymbol calledMethod, CSharpSyntaxNode calledMethodNode, SyntaxNode callSite)
		{
		}

		/// <summary>
		/// Extensibility point that allows to add some logic executed after the recursive visit of a <paramref name="calledMethod"/>.
		/// The logic is executed both for methods that passed bypass check and were visited and for methods that didn't pass it. <br/>
		/// The flag <paramref name="wasVisited"/> allows to distinguish between the methods that were really visited and those that were bypassed.
		/// </summary>
		/// <param name="calledMethod">The called method symbol.</param>
		/// <param name="calledMethodNode">The called method node.</param>
		/// <param name="callSite">Syntax node representing the call site into which the walker steps in.</param>
		/// <param name="wasVisited">True if the <paramref name="calledMethod"/> was really visited.</param>
		protected virtual void AfterRecursiveVisit(IMethodSymbol calledMethod, CSharpSyntaxNode calledMethodNode, SyntaxNode callSite, bool wasVisited)
		{
		}

		/// <summary>
		/// An analysis that checks if walker should skip going into <paramref name="calledMethod"/>.
		/// </summary>
		/// <remarks>
		/// The default implementation will check <paramref name="calledMethod"/>.ContainingType and bypass all types obtained from the extendable <see cref="GetTypesToBypass"/> method.<br/>
		/// If the method containing type is one of bypassed types then the code will immediately return <see langword="true"/> and <paramref name="calledMethod"/> will be bypassed. If custom
		/// extraBypassCheck delegate was specified in the <see cref=" NestedInvocationWalker"/> constructor then it will be called after the check for bypassed types.<br/>
		/// The method be overriden for custom skip logic.
		/// </remarks>
		/// <param name="calledMethod">The called method symbol to check.</param>
		/// <param name="calledMethodNode">The called method node.</param>
		/// <param name="callSite">Syntax node representing the call site into which the walker steps in.</param>
		/// <returns>
		/// <see langword="true"/> if the <paramref name="calledMethod"/> should be skipped, <see langword="false"/> if the walker should go into it.
		/// </returns>
		protected virtual bool BypassMethod(IMethodSymbol calledMethod, CSharpSyntaxNode calledMethodNode, SyntaxNode callSite)
		{
			var typesToBypass = _typesToBypass.Value;

			if (typesToBypass != null && typesToBypass.Contains(calledMethod.ContainingType))
				return true;

			return _extraBypassCheck?.Invoke(calledMethod) ?? false;
		}

		private void Push(SyntaxNode callSite, IMethodSymbol calledMethod)
		{
			if (_nodesStack.Count == 0)
				OriginalNode = callSite;

			_nodesStack.Push(callSite);
			_methodsInStack.Add(calledMethod);
		}

		private void Pop(IMethodSymbol calledMethod)
		{
			_nodesStack.Pop();
			_methodsInStack.Remove(calledMethod);

			if (_nodesStack.Count == 0)
				OriginalNode = null;
		}
		#endregion
	}
}
