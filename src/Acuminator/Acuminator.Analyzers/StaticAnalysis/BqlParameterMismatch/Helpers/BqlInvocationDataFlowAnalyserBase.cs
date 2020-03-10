using System;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.BqlParameterMismatch
{
	public partial class BqlParameterMismatchAnalyzer : PXDiagnosticAnalyzer
	{
		protected abstract class BqlInvocationDataFlowAnalyserBase
		{
			protected SyntaxNodeAnalysisContext SyntaxContext { get; }

			protected PXContext PXContext { get; }

			protected InvocationExpressionSyntax Invocation { get; }

			protected StatementSyntax InvocationStatement { get; }

			protected string VariableName { get; }

			protected SemanticModel SemanticModel => SyntaxContext.SemanticModel;

			protected CancellationToken CancellationToken => SyntaxContext.CancellationToken;

			public BqlInvocationDataFlowAnalyserBase(SyntaxNodeAnalysisContext syntaxContext, InvocationExpressionSyntax invocation,
													 PXContext pxContext, IdentifierNameSyntax identifierNode)
			{
				SyntaxContext = syntaxContext;
				PXContext = pxContext;
				Invocation = invocation;
				InvocationStatement = Invocation.GetStatementNode();
				VariableName = identifierNode.Identifier.ValueText;
			}

			protected (bool AnalysisSucceded, bool VarAlwaysAssigned) CheckCandidate(StatementSyntax assignmentStatement)
			{
				var lcaResult = LowestCommonAncestor.GetCommonAncestorForSyntaxStatements(assignmentStatement, InvocationStatement);
				var scopedAssignment = lcaResult.ScopedX;
				var scopedInvocation = lcaResult.ScopedY;

				if (scopedAssignment == null || scopedInvocation == null)
					return (false, false);       //If there was some kind of error during analysis we should assume the worst case - that the candidat is valid but not always assigns variable 

				switch (scopedAssignment)
				{
					case SwitchStatementSyntax _:
					case IfStatementSyntax _:
						return (true, false);
				}

				DataFlowAnalysis flowAnalysisWithAssignment = null;

				try
				{
					flowAnalysisWithAssignment = SemanticModel.AnalyzeDataFlow(scopedAssignment, scopedInvocation);
				}
				catch
				{
					return (false, false);
				}

				if (flowAnalysisWithAssignment == null || !flowAnalysisWithAssignment.Succeeded)
					return (false, false);
				if (flowAnalysisWithAssignment.AlwaysAssigned.All(var => var.Name != VariableName))
					return (true, false);

				return (true, true);
			}

			protected bool IsReacheableByControlFlow(StatementSyntax statement)
			{
				ControlFlowAnalysis controlFlow = null;

				try
				{
					controlFlow = SemanticModel.AnalyzeControlFlow(statement);
				}
				catch
				{
					//If there was some kind of error during analysis we should assume the worst case - that assignment is reacheable
					return true;
				}

				return controlFlow?.Succeeded != true || controlFlow.EndPointIsReachable;
			}
		}
	}
}
