using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers
{
	public partial class RowChangesInEventHandlersAnalyzer
	{
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
			private readonly RowChangesAnalysisMode _analysisMode;
			private readonly ImmutableHashSet<ILocalSymbol> _rowVariables;
			private readonly object[] _messageArgs;

			private readonly VariableMemberAccessWalker _variableMemberAccessWalker;
			private readonly DacInstanceAccessWalker _dacInstanceAccessWalker;

			public DiagnosticWalker(SymbolAnalysisContext context, SemanticModel semanticModel, PXContext pxContext, 
				ImmutableArray<ILocalSymbol> rowVariables, // variables which were assigned with e.Row
				RowChangesAnalysisMode analysisMode,
				params object[] messageArgs)
			{
				pxContext.ThrowOnNull(nameof (pxContext));
				semanticModel.ThrowOnNull(nameof (semanticModel));

				_context = context;
				_semanticModel = semanticModel;
				_pxContext = pxContext;
				_analysisMode = analysisMode;
				_rowVariables = rowVariables.ToImmutableHashSet();
				_messageArgs = messageArgs;

				_variableMemberAccessWalker = new VariableMemberAccessWalker(_rowVariables, semanticModel);
				_dacInstanceAccessWalker = new DacInstanceAccessWalker(semanticModel);
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				_context.CancellationToken.ThrowIfCancellationRequested();

				var methodSymbol = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

				if (methodSymbol != null && IsMethodForbidden(methodSymbol))
				{
					bool found = node.ArgumentList.Arguments
						.Where(arg => arg.Expression != null)
						.Select(arg => _semanticModel.GetSymbolInfo(arg.Expression).Symbol as ILocalSymbol)
						.Any(variable => variable != null && _rowVariables.Contains(variable));

					if (!found)
					{
						var walker = new EventArgsRowWalker(_semanticModel, _pxContext);
						node.ArgumentList.Accept(walker);

						found = walker.Success;
					}

					if (found && _analysisMode == RowChangesAnalysisMode.ChangesForbiddenForRowFromEventArgs)
					{
						_context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs, 
							node.GetLocation(), _messageArgs));
					}
					else if (!found && _analysisMode == RowChangesAnalysisMode.ChangesAllowedOnlyForRowFromEventArgs)
					{
						_context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1048_RowChangesInEventHandlersAllowedForArgsOnly, 
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
						_variableMemberAccessWalker.Reset();
						node.Left.Accept(_variableMemberAccessWalker);
						found = _variableMemberAccessWalker.Success;
					}
					
					if (found && _analysisMode == RowChangesAnalysisMode.ChangesForbiddenForRowFromEventArgs)
					{
						_context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs, 
							node.GetLocation(), _messageArgs));
					}
					else if (!found && _analysisMode == RowChangesAnalysisMode.ChangesAllowedOnlyForRowFromEventArgs)
					{
						_dacInstanceAccessWalker.Reset();
						node.Left.Accept(_dacInstanceAccessWalker);

						if (_dacInstanceAccessWalker.Success)
						{
							_context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1048_RowChangesInEventHandlersAllowedForArgsOnly,
								node.GetLocation(), _messageArgs));
						}
					}
				}
			}


			private bool IsMethodForbidden(IMethodSymbol symbol)
			{
				return symbol.ContainingType?.OriginalDefinition != null
				       && symbol.ContainingType.OriginalDefinition.InheritsFromOrEquals(_pxContext.PXCache.Type)
				       && MethodNames.Contains(symbol.Name);
			}
		}

	}
}
