using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using PX.SM;

namespace Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers
{
	public class RowChangesInEventHandlersAnalyzer : IEventHandlerAnalyzer
	{
		private static readonly ISet<EventType> AnalyzedEventTypes = new HashSet<EventType>()
		{
			EventType.FieldDefaulting,
			EventType.FieldVerifying,
			EventType.RowSelected,
		};

		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create(Descriptors.PX1047_RowChangesInEventHandlers);

		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (AnalyzedEventTypes.Contains(eventType))
			{
				var methodSymbol = (IMethodSymbol) context.Symbol;
				var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as MethodDeclarationSyntax;

				if (methodSyntax != null)
				{
					var semanticModel = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree, true);
					var variablesWalker = new VariablesWalker(methodSyntax, semanticModel, pxContext,
						context.CancellationToken);
					methodSyntax.Accept(variablesWalker);

					var diagnosticWalker = new DiagnosticWalker(context, semanticModel, pxContext, variablesWalker.Result, eventType);
					methodSyntax.Accept(diagnosticWalker);
				}
			}
		}

		private class VariablesWalker : CSharpSyntaxWalker
		{
			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;
			private CancellationToken _cancellationToken;
			private readonly ImmutableHashSet<ILocalSymbol> _variables;

			private readonly ISet<ILocalSymbol> _result = new HashSet<ILocalSymbol>();
			public ImmutableArray<ILocalSymbol> Result => _result.ToImmutableArray();

			public VariablesWalker(MethodDeclarationSyntax methodSyntax, SemanticModel semanticModel, PXContext pxContext,
				CancellationToken cancellationToken)
			{
				methodSyntax.ThrowOnNull(nameof (methodSyntax));
				semanticModel.ThrowOnNull(nameof (semanticModel));
				pxContext.ThrowOnNull(nameof (pxContext));

				_semanticModel = semanticModel;
				_pxContext = pxContext;
				_cancellationToken = cancellationToken;

				if (methodSyntax.Body != null || methodSyntax.ExpressionBody?.Expression != null)
				{
					var dataFlow = methodSyntax.Body != null
						? semanticModel.AnalyzeDataFlow(methodSyntax.Body)
						: semanticModel.AnalyzeDataFlow(methodSyntax.ExpressionBody.Expression);

					if (dataFlow.Succeeded)
					{
						_variables = dataFlow.WrittenInside
							.Intersect(dataFlow.VariablesDeclared)
							.OfType<ILocalSymbol>()
							.ToImmutableHashSet();
					}
				}
				
			}

			public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
			{
				_cancellationToken.ThrowIfCancellationRequested();

				if (node.Left is IdentifierNameSyntax variableNode && node.Right != null)
				{
					var variableSymbol = _semanticModel.GetSymbolInfo(variableNode).Symbol as ILocalSymbol;

					if (variableSymbol != null && _variables.Contains(variableSymbol))
					{
						var walker = new EventArgsRowWalker(_semanticModel, _pxContext);
						node.Right.Accept(walker);

						if (walker.Success)
							_result.Add(variableSymbol);
					}
				}
			}

			public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
			{
				foreach (var variableDeclarator in node.Variables
					.Where(v => v.Initializer?.Value != null))
				{
					var variableSymbol = _semanticModel.GetDeclaredSymbol(variableDeclarator) as ILocalSymbol;

					if (variableSymbol != null)
					{
						var walker = new EventArgsRowWalker(_semanticModel, _pxContext);
						variableDeclarator.Initializer.Value.Accept(walker);

						if (walker.Success)
							_result.Add(variableSymbol);
					}
				}
			}
		}

		private class EventArgsRowWalker : CSharpSyntaxWalker
		{
			private static readonly string RowPropertyName = "Row";

			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;

			public bool Success { get; private set; }

			public EventArgsRowWalker(SemanticModel semanticModel, PXContext pxContext)
			{
				_semanticModel = semanticModel;
				_pxContext = pxContext;
			}

			public override void Visit(SyntaxNode node)
			{
				if (!Success)
					base.Visit(node);
			}

			public override void VisitIdentifierName(IdentifierNameSyntax node)
			{
				if (node.Identifier.Text == RowPropertyName)
				{
					var propertySymbol = _semanticModel.GetSymbolInfo(node).Symbol as IPropertySymbol;
					var containingType = propertySymbol?.ContainingType?.OriginalDefinition;

					if (containingType != null && _pxContext.Events.EventTypeMap.ContainsKey(containingType))
					{
						Success = true;
					}
				}
			}
		}

		private class VariableAccessWalker : CSharpSyntaxWalker
		{
			private readonly ImmutableHashSet<ILocalSymbol> _variables;
			private readonly SemanticModel _semanticModel;

			public bool Success { get; private set; }

			public VariableAccessWalker(ImmutableHashSet<ILocalSymbol> variables, SemanticModel semanticModel)
			{
				_variables = variables;
				_semanticModel = semanticModel;
			}

			public override void Visit(SyntaxNode node)
			{
				if (!Success)
					base.Visit(node);
			}

			public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
			{
				if (IsVariable(node.Expression))
					Success = true;
			}

			public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
			{
				if (IsVariable(node.Expression))
					Success = true;
			}

			private bool IsVariable(ExpressionSyntax node)
			{
				return node != null
				       && _semanticModel.GetSymbolInfo(node).Symbol is ILocalSymbol variable
				       && _variables.Contains(variable);
			}
		}

		private class DiagnosticWalker : CSharpSyntaxWalker
		{
			private static readonly ISet<string> MethodNames = new HashSet<string>(StringComparer.Ordinal)
			{
				"SetValue" ,
				"SetValueExt",
				"SetDefaultExt",
			};

			private SymbolAnalysisContext _context;
			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;
			private readonly ImmutableHashSet<ILocalSymbol> _rowVariables;
			private readonly object[] _messageArgs;

			public DiagnosticWalker(SymbolAnalysisContext context, SemanticModel semanticModel, PXContext pxContext, 
				ImmutableArray<ILocalSymbol> rowVariables, // variables which were assigned with e.Row
				params object[] messageArgs)
			{
				pxContext.ThrowOnNull(nameof (pxContext));

				_context = context;
				_semanticModel = semanticModel;
				_pxContext = pxContext;
				_rowVariables = rowVariables.ToImmutableHashSet();
				_messageArgs = messageArgs;
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				_context.CancellationToken.ThrowIfCancellationRequested();

				var methodSymbol = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

				if (methodSymbol != null && IsMethodForbidden(methodSymbol))
				{
					bool found = false;

					found = node.ArgumentList.Arguments
						.Where(arg => arg.Expression != null)
						.Select(arg => _semanticModel.GetSymbolInfo(arg.Expression).Symbol as ILocalSymbol)
						.Any(variable => variable != null && _rowVariables.Contains(variable));

					if (!found)
					{
						var walker = new EventArgsRowWalker(_semanticModel, _pxContext);
						node.ArgumentList.Accept(walker);

						found = walker.Success;
					}

					if (found)
					{
						_context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1047_RowChangesInEventHandlers, 
							node.GetLocation(), _messageArgs));
					}
				}
			}

			public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
			{
				if (node.Left != null)
				{
					var walker = new EventArgsRowWalker(_semanticModel, _pxContext);
					node.Left.Accept(walker);
					bool found = walker.Success;

					if (!found)
					{
						var varWalker = new VariableAccessWalker(_rowVariables, _semanticModel);
						node.Left.Accept(varWalker);
						found = varWalker.Success;
					}
					
					if (found)
					{
						_context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1047_RowChangesInEventHandlers, 
							node.GetLocation(), _messageArgs));
					}
				}
			}


			private bool IsMethodForbidden(IMethodSymbol symbol)
			{
				return symbol.ContainingType?.OriginalDefinition != null
				       && symbol.ContainingType.OriginalDefinition.InheritsFromOrEquals(_pxContext.PXCacheType)
				       && MethodNames.Contains(symbol.Name);
			}
		}

	}
}
