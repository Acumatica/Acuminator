using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace Acuminator.Utilities.Roslyn.CodeActions
{
	/// <summary>
	/// A base class for code actions with posibility to disable diplay of preview. 
	/// </summary>
	public abstract class SimpleCodeActionWithOptionalPreview : CodeAction
	{
		public sealed override string Title { get; }

		public sealed override string EquivalenceKey { get; }

		public bool DisplayPreview { get; }

		public SimpleCodeActionWithOptionalPreview(string title, string equivalenceKey, bool displayPreview)
		{
			title.ThrowOnNullOrWhiteSpace();

			Title = title;
			EquivalenceKey = equivalenceKey;
			DisplayPreview = displayPreview;
		}

		protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken) =>
			Task.FromResult<Document>(null);

		protected override Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(CancellationToken cancellationToken) =>
			DisplayPreview
				? base.ComputePreviewOperationsAsync(cancellationToken)
				: Task.FromResult<IEnumerable<CodeActionOperation>>(null);
	}
}
