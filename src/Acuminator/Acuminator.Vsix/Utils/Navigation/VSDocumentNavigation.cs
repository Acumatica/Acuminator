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
		private const string TextDocumentPropertyName = "TextDocument";


		public static bool OpenCodeFile(this IServiceProvider serviceProvider, string filePath, bool navigateToCodeFile)
		{
			serviceProvider.ThrowOnNull(nameof(serviceProvider));

			if (!ThreadHelper.CheckAccess() || !File.Exists(filePath) || !(serviceProvider.GetService(typeof(DTE)) is DTE dte))
				return false;

			var (window, wasOpened) = OpenOrGetCodeWindow(dte, filePath);

			if (window?.Document == null)
				return false;
			else if (wasOpened && !navigateToCodeFile)
				return true;

			



			IWpfTextView textView = serviceProvider.GetWpfTextView();

			if (textView == null)
				return false;

			return true;
		}

#pragma warning disable VSTHRD010
		private static (EnvDTE.Window Window, bool WasOpened) OpenOrGetCodeWindow(DTE dte, string filePath)
		{
			try
			{
				bool isFileOpened = dte.ItemOperations.IsFileOpen(filePath);
				var window = dte.ItemOperations.OpenFile(filePath, EnvDTE.Constants.vsViewKindTextView);
				return (window, isFileOpened);
			}
			catch
			{
				return default;
			}
		}
#pragma warning restore VSTHRD010
	}
}
