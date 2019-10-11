using System;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.CodeActions
{
	/// <summary>
	/// A document change simple action. Copied from Roslyn, should be removed in the future.
	/// </summary>
	public class DocumentChangeActionWithOptionalPreview : SimpleCodeActionWithOptionalPreview
	{
		private readonly Func<CancellationToken, Task<Document>> _createChangedDocument;

		public DocumentChangeActionWithOptionalPreview(string title, Func<CancellationToken, Task<Document>> createChangedDocument, bool displayPreview, 
													   string equivalenceKey = null) :
												  base(title, equivalenceKey, displayPreview)
		{
			_createChangedDocument = createChangedDocument.CheckIfNull(nameof(createChangedDocument));
		}

		protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			return _createChangedDocument(cancellationToken);
		}
	}
}
