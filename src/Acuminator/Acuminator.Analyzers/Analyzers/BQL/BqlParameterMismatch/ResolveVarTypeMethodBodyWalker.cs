using System;
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
		/// <summary>
		/// The BQL parameters counting syntax walker.
		/// </summary>
		protected class ResolveVarTypeMethodBodyWalker : CSharpSyntaxWalker
		{
			private readonly SyntaxNodeAnalysisContext syntaxContext;			
			private readonly PXContext pxContext;

			private readonly MethodDeclarationSyntax methodDeclaration;
			private readonly InvocationExpressionSyntax invocation;
			private readonly string variable;

			private SemanticModel SemanticModel => syntaxContext.SemanticModel;

			private CancellationToken CancellationToken => syntaxContext.CancellationToken;
			private bool shouldStop;

			public ITypeSymbol VariableResolvedType
			{
				get;
				private set;
			}

			public bool IsValid
			{
				get;
				private set;
			}

			public ResolveVarTypeMethodBodyWalker(SyntaxNodeAnalysisContext aSyntaxContext, PXContext aPxContext, MethodDeclarationSyntax methodNode,
												  InvocationExpressionSyntax invocationNode, string variableName)
			{
				syntaxContext = aSyntaxContext;
				methodDeclaration = methodNode;
				invocation = invocationNode;
				variable = variableName;
				
				IsValid = true;
			}

			public override void Visit(SyntaxNode node)
			{
				if (!shouldStop)
					base.Visit(node);
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				if (node.IsEquivalentTo(invocation))
				{
					shouldStop = true;
					return;
				}

				base.VisitInvocationExpression(node);
			}


			public override void VisitGenericName(GenericNameSyntax genericNode)
			{
				if (cancellationToken.IsCancellationRequested)
					return;

				SymbolInfo symbolInfo = syntaxContext.SemanticModel.GetSymbolInfo(genericNode, cancellationToken);

				if (cancellationToken.IsCancellationRequested || !(symbolInfo.Symbol is ITypeSymbol typeSymbol))
				{
					if (!cancellationToken.IsCancellationRequested)
						base.VisitGenericName(genericNode);

					return;
				}

				if (genericNode.IsUnboundGenericName)
					typeSymbol = typeSymbol.OriginalDefinition;

				if (!ParametersCounter.CountParametersInTypeSymbol(typeSymbol, cancellationToken))
					return;

				if (!cancellationToken.IsCancellationRequested)
					base.VisitGenericName(genericNode);
			}
		}
	}
}
