using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;


using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Document = Microsoft.CodeAnalysis.Document;
using DTE = EnvDTE.DTE;



namespace Acuminator.Vsix.Utils.Navigation
{
	public static class VSDocumentNavigation
	{
		public static (bool IsSuccess, CaretPosition CaretPosition) OpenCodeFileAndNavigateToPosition(this IServiceProvider serviceProvider,
																									  Solution solution, string filePath, 
																									  int? caretPosition = null)
		{
			serviceProvider.ThrowOnNull(nameof(serviceProvider));

			if (caretPosition.HasValue && caretPosition < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(caretPosition));
			}

			IWpfTextView wpfTextView = OpenCodeWindow(serviceProvider, solution, filePath);

			if (wpfTextView == null)
				return default;

			try
			{
				CaretPosition caret = caretPosition.HasValue
					? wpfTextView.MoveCaretTo(caretPosition.Value)
					: wpfTextView.Caret.Position;

				return (true, caret);
			}
			catch
			{
				return default;
			}
		}

		public static IWpfTextView OpenCodeWindow(this IServiceProvider serviceProvider, Solution solution, string filePath)
		{
			serviceProvider.ThrowOnNull(nameof(serviceProvider));
			solution.ThrowOnNull(nameof(solution));

			if (!ThreadHelper.CheckAccess() || !File.Exists(filePath))
				return null;

			ImmutableArray<DocumentId> documentIDs = solution.GetDocumentIdsWithFilePath(filePath);

			if (documentIDs.Length != 1)
				return null;

			DocumentId documentId = documentIDs[0];
			bool wasAlreadyOpened = solution.IsFileOpen(documentId);

			try
			{
				solution.Workspace.OpenDocument(documentId);
				return wasAlreadyOpened
					? serviceProvider.GetWpfTextViewByFilePath(filePath)
					: serviceProvider.GetWpfTextView(); 			
			}
			catch
			{
				return null;
			}
		}

		private static bool IsFileOpen(this Solution solution, DocumentId documentID) => 
			solution.Workspace.GetOpenDocumentIds()
							  .Contains(documentID);
	}
}
