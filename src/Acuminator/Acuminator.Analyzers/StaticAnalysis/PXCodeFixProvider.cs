using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis
{
	/// <summary>
	/// A base type for Acuminator code fix provider that already has some boilerplate diagnostic checking logic implemented.
	/// </summary>
	public abstract class PXCodeFixProvider : CodeFixProvider
	{
		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			var diagnostics = context.Diagnostics;

			if (diagnostics.IsDefaultOrEmpty)
				return Task.CompletedTask;
			else if (diagnostics.Length == 1)		//Hot path optimization for single diagnostic
			{
				var diagnostic = diagnostics[0];

				if (FixableDiagnosticIds.Contains(diagnostic.Id))
					return RegisterCodeFixesForDiagnosticAsync(context, diagnostic);
				else
					return Task.CompletedTask;
			}

			List<Task> allTasks = new(capacity: diagnostics.Length);

			// Some FixableDiagnosticIds return a new collection each time they are called, so we can cache it here
			var fixableDiagnosticIds = FixableDiagnosticIds;

			foreach (Diagnostic diagnostic in context.Diagnostics)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (fixableDiagnosticIds.Contains(diagnostic.Id))
				{
					var task = RegisterCodeFixesForDiagnosticAsync(context, diagnostic);
					allTasks.Add(task);
				}
			}

			switch (allTasks.Count)
			{
				case 0:
					return Task.CompletedTask;
				case 1:
					return allTasks[0];
				default:
					return Task.WhenAll(allTasks);
			}	
		}

		protected abstract Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic);
	}
}
