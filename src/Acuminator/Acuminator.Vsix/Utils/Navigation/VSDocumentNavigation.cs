using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
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
																									  string filePath, int caretPosition)
		{
			serviceProvider.ThrowOnNull(nameof(serviceProvider));

			if (caretPosition < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(caretPosition));
			}

			var window = OpenCodeWindow(serviceProvider, filePath);

			if (window == null || !(serviceProvider.GetWpfTextView() is IWpfTextView activeTextView))
				return default;

			try
			{
				CaretPosition caret = activeTextView.MoveCaretTo(caretPosition);
				return (true, caret);
			}
			catch
			{
				return default;
			}
		}

#pragma warning disable VSTHRD010
		public static EnvDTE.Window OpenCodeWindow(this IServiceProvider serviceProvider, string filePath)
		{
			serviceProvider.ThrowOnNull(nameof(serviceProvider));		

			if (!ThreadHelper.CheckAccess() || !File.Exists(filePath) || !(serviceProvider.GetService(typeof(DTE)) is DTE dte))
				return null;

			try
			{
				return dte.ItemOperations.OpenFile(filePath, EnvDTE.Constants.vsViewKindTextView);
			}
			catch
			{
				return default;
			}
		}

		private static TextDocument GetTextDocumentFromWindow(this EnvDTE.Window window)
		{
			const string TextDocumentPropertyName = "TextDocument";

			try
			{
				return window.Document?.Object(TextDocumentPropertyName) as TextDocument;
			}
			catch
			{
				return default;
			}
		}
#pragma warning restore VSTHRD010
	}
}
