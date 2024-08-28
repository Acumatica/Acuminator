using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.TypoInViewAndActionHandlerName
{
	public class TypoInViewAndActionHandlerNameAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public const string ViewOrActionNameProperty = nameof(ViewOrActionNameProperty);

		private const int MaximumDistance = 2;

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1005_TypoInViewDelegateName,
				Descriptors.PX1005_TypoInActionDelegateName
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphModel)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var declaredNonOverriddenMethods = graphModel.Symbol.GetMethods()
																.Where(method => method.IsExplicitlyDeclared() && !method.IsOverride)
																.ToList();
			if (declaredNonOverriddenMethods.Count > 0)
			{
				AnalyzeViewDelegates(context, pxContext, graphModel, declaredNonOverriddenMethods);

				context.CancellationToken.ThrowIfCancellationRequested();

				AnalyzeActionHandlers(context, pxContext, graphModel, declaredNonOverriddenMethods);
			}
		}

		private void AnalyzeViewDelegates(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphModel,
										  List<IMethodSymbol> declaredNonOverriddenMethods)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var declaredViewDelegatesByName = graphModel.DeclaredViewDelegates
												.ToDictionary(delegateInfo => delegateInfo.Name, StringComparer.OrdinalIgnoreCase);
			var viewsWithoutDelegatesNames = GetNamesOfViewsOrActionsWithoutDelegate(graphModel.Views, declaredViewDelegatesByName);

			if (viewsWithoutDelegatesNames.Count > 0)
			{
				var viewDelegateCandidates = GetViewDelegateCandidates(declaredNonOverriddenMethods, declaredViewDelegatesByName, pxContext);

				AnalyzeCandidatesForViewOrActionDelegates(context, Descriptors.PX1005_TypoInViewDelegateName,
														  viewDelegateCandidates, viewsWithoutDelegatesNames, pxContext);
			}
		}

		private void AnalyzeActionHandlers(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphModel,
										   List<IMethodSymbol> declaredNonOverriddenMethods)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var declaredActionHandlersByName = graphModel.DeclaredActionHandlers
												 .ToDictionary(handlerInfo => handlerInfo.Name, StringComparer.OrdinalIgnoreCase);
			var actionsWithoutDelegatesNames = GetNamesOfViewsOrActionsWithoutDelegate(graphModel.Actions, declaredActionHandlersByName);

			if (actionsWithoutDelegatesNames.Count > 0)
			{
				var actionHandlerCandidates = GetActionHandlerCandidates(declaredNonOverriddenMethods, declaredActionHandlersByName, pxContext);

				AnalyzeCandidatesForViewOrActionDelegates(context, Descriptors.PX1005_TypoInActionDelegateName,
														  actionHandlerCandidates, actionsWithoutDelegatesNames, pxContext);
			}
		}

		/// <summary>
		/// Gets names of views or actions without delegate.
		/// </summary>
		/// <typeparam name="TActionOrViewInfo">Type of the action or view information.</typeparam>
		/// <typeparam name="TDelegateOrHandlerInfo">Type of the delegate or handler information.</typeparam>
		/// <param name="viewsOrActions">Views or actions from graph/graph extension and their base (in both C# and Acumatica sense) types.</param>
		/// <param name="declaredViewOrActionDelegates">View or action delegates declared in the graph / graph extension.</param>
		/// <returns>
		/// The names of views or actions without delegate.
		/// </returns>
		/// <remarks>
		/// This method returns names of views/actions that may correspond to some view delegate / action handler with typo.<br/>
		/// Views or actions are taken from graph/ graph extension and their base (in both C# and Acumatica sense) types.<br/>
		/// They are filtered by the requirement that they should not have a corresponding view delegate / action handler in the graph / graph extension.<br/>
		/// It's okay to have a view delegate / action handler in the base type though, since there is a scenario of override of a base view delegate / action handler.<br/>
		/// </remarks>
		private HashSet<string> GetNamesOfViewsOrActionsWithoutDelegate<TActionOrViewInfo, TDelegateOrHandlerInfo>(
																	IEnumerable<TActionOrViewInfo> viewsOrActions,
																	Dictionary<string, TDelegateOrHandlerInfo> declaredViewOrActionDelegates)
		where TActionOrViewInfo : SymbolItem<ISymbol>
		where TDelegateOrHandlerInfo : SymbolItem<IMethodSymbol>
		{
			if (declaredViewOrActionDelegates.Count > 0)
			{
				var viewsOrActionsWithoutDelegatesNames = viewsOrActions.Where(viewOrAction => !declaredViewOrActionDelegates.ContainsKey(viewOrAction.Name))
																		.Select(viewOrAction => viewOrAction.Name)
																		.ToHashSet(StringComparer.OrdinalIgnoreCase);
				return viewsOrActionsWithoutDelegatesNames;
			}
			else
			{
				return viewsOrActions.Select(viewOrAction => viewOrAction.Name)
									 .ToHashSet(StringComparer.OrdinalIgnoreCase);
			}
		}

		private IEnumerable<IMethodSymbol> GetViewDelegateCandidates(List<IMethodSymbol> declaredNonOverriddenMethods,
																	 Dictionary<string, DataViewDelegateInfo> declaredViewDelegatesByName, PXContext pxContext)
		{
			var candidates = GetActionHandlerOrViewDelegateCandidates(declaredNonOverriddenMethods, declaredViewDelegatesByName, pxContext)
													.Where(method => method.IsValidViewDelegate(pxContext) && !method.IsValidActionHandler(pxContext));
			return candidates;
		}

		private IEnumerable<IMethodSymbol> GetActionHandlerCandidates(List<IMethodSymbol> declaredNonOverriddenMethods,
																	  Dictionary<string, ActionHandlerInfo> declaredActionHandlersByName, PXContext pxContext)
		{
			var candidates = GetActionHandlerOrViewDelegateCandidates(declaredNonOverriddenMethods, declaredActionHandlersByName, pxContext)
													.Where(method => method.IsValidActionHandler(pxContext) && !method.IsValidViewDelegate(pxContext));
			return candidates;
		}

		private IEnumerable<IMethodSymbol> GetActionHandlerOrViewDelegateCandidates<TDelegateOrHandlerInfo>(List<IMethodSymbol> declaredNonOverriddenMethods,
																				Dictionary<string, TDelegateOrHandlerInfo> declaredViewOrActionDelegatesByName,
																				PXContext pxContext)
		where TDelegateOrHandlerInfo : SymbolItem<IMethodSymbol>
		{
			if (declaredViewOrActionDelegatesByName.Count == 0)
				return declaredNonOverriddenMethods;

			var candidates = from method in declaredNonOverriddenMethods
							 where !declaredViewOrActionDelegatesByName.TryGetValue(method.Name, out TDelegateOrHandlerInfo? actionOrViewDelegateInfo) ||
								   !method.Equals(actionOrViewDelegateInfo.Symbol, SymbolEqualityComparer.Default)
							 select method;
			return candidates;
		}

		private void AnalyzeCandidatesForViewOrActionDelegates(SymbolAnalysisContext context, DiagnosticDescriptor diagnosticDescriptor,
																IEnumerable<IMethodSymbol> viewOrActionDelegateCandidates,
																HashSet<string> namesOfViewsOrActionsWithoutDelegates, PXContext pxContext)
		{
			foreach (IMethodSymbol candidateMethod in viewOrActionDelegateCandidates)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (namesOfViewsOrActionsWithoutDelegates.Contains(candidateMethod.Name))
					continue;

				string? nearestViewOrActionName = FindNearestViewOrAction(namesOfViewsOrActionsWithoutDelegates, candidateMethod);

				if (nearestViewOrActionName != null && !candidateMethod.Locations.IsEmpty)
				{
					var properties = ImmutableDictionary.CreateBuilder<string, string?>();
					properties.Add(ViewOrActionNameProperty, nearestViewOrActionName);

					var location = candidateMethod.Locations.FirstOrDefault();

					if (location == null)
						continue;

					context.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(diagnosticDescriptor, location, properties.ToImmutable(), nearestViewOrActionName),
						pxContext.CodeAnalysisSettings);
				}
			}
		}

		private string? FindNearestViewOrAction(IEnumerable<string> viewCandidatesNames, IMethodSymbol method)
		{
			string methodName = method.Name.ToLowerInvariant();
			int minDistance = int.MaxValue;
			string? nearestViewOrActionName = null;

			foreach (var viewOrActionName in viewCandidatesNames)
			{
				int distance = StringExtensions.LevenshteinDistance(methodName, viewOrActionName.ToLowerInvariant());

				if (distance <= MaximumDistance && distance < minDistance)
				{
					minDistance = distance;
					nearestViewOrActionName = viewOrActionName;
				}
			}

			return nearestViewOrActionName;
		}
	}
}
