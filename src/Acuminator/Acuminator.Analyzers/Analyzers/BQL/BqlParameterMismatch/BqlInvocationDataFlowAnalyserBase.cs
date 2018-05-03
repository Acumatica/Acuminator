using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities;
using PX.Data;


namespace Acuminator.Analyzers
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

			public BqlInvocationDataFlowAnalyserBase(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, IdentifierNameSyntax identifierNode)
			{
				SyntaxContext = syntaxContext;
				PXContext = pxContext;
				Invocation = SyntaxContext.Node as InvocationExpressionSyntax;
                InvocationStatement = Invocation.GetStatementNode();
				VariableName = identifierNode.Identifier.ValueText;
			}

			protected (bool AnalysisSucceded, bool VarAlwaysAssigned) CheckCandidate(StatementSyntax assignmentStatement)
            {
                var (_, scopedAssignment, scopedInvocation) =
                    RoslynSyntaxUtils.LowestCommonAncestorSyntaxStatement(assignmentStatement, InvocationStatement);

                if (scopedAssignment == null || scopedInvocation == null)
                    return (false, false);       //If there was some kind of error during analysis we should assume the worst case - that the candidat is valid but not always assigns variable 

                DataFlowAnalysis flowAnalysisWithAssignment = null;

                try
                {
                    flowAnalysisWithAssignment = SemanticModel.AnalyzeDataFlow(scopedAssignment, scopedInvocation);
                }
                catch (Exception e)
                {
                    return (false, false);
                }

                if (flowAnalysisWithAssignment == null || !flowAnalysisWithAssignment.Succeeded)
                    return (false, false);
                else if (flowAnalysisWithAssignment.AlwaysAssigned.All(var => var.Name != VariableName))
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
                catch (Exception e)
                {
                    //If there was some kind of error during analysis we should assume the worst case - that assignment is reacheable
                    return true;
                }

                return controlFlow?.Succeeded == true
                    ? controlFlow.EndPointIsReachable
                    : true;
            }
		}	
	}
}
