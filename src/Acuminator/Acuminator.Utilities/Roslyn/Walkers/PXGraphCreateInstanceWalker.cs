#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Utilities.Roslyn.Walkers
{
	/// <summary>
	/// A recursive walker that reports graph creation.
	/// </summary>
	public class PXGraphCreateInstanceWalker : NestedInvocationWalker
	{
		private readonly SymbolAnalysisContext _context;
		private readonly PXContext _pxContext;
		private readonly DiagnosticDescriptor _descriptor;

		public PXGraphCreateInstanceWalker(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor descriptor)
			: base(context.Compilation, context.CancellationToken, pxContext.CodeAnalysisSettings)
		{
			_context = context;
			_pxContext = pxContext.CheckIfNull(nameof(pxContext));
			_descriptor = descriptor.CheckIfNull(nameof(descriptor));
		}

		public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
		{
			_context.CancellationToken.ThrowIfCancellationRequested();

			IMethodSymbol symbol = GetSymbol<IMethodSymbol>(node);

			if (symbol != null && _pxContext.PXGraph.CreateInstance.Contains(symbol.ConstructedFrom))
			{
				ReportDiagnostic(_context.ReportDiagnostic, _descriptor, node);
			}
			else
			{
				base.VisitMemberAccessExpression(node);
			}
		}

		/// <summary>
		/// Called when the visitor visits a ObjectCreationExpressionSyntax node (a constructor call via new).
		/// We need to check that graphs are not created via "<see langword="new"/> PXGraph()" constructor call.
		/// </summary>
		/// <param name="node">The node.</param>
		public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
		{
			_context.CancellationToken.ThrowIfCancellationRequested();

			ITypeSymbol createdObjectType = GetSymbol<ITypeSymbol>(node.Type);

			if (createdObjectType != null && createdObjectType.IsPXGraph(_pxContext))
			{
				ReportDiagnostic(_context.ReportDiagnostic, _descriptor, node);
			}
			else
			{
				base.VisitObjectCreationExpression(node);
			}
		}
	}
}
