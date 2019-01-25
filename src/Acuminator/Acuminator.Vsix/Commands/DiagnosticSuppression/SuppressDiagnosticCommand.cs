using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;

using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Document = Microsoft.CodeAnalysis.Document;


namespace Acuminator.Vsix.DiagnosticSuppression
{
	/// <summary>
	/// Suppress Diagnostic Command.
	/// </summary>
	internal sealed class SuppressDiagnosticCommand : VSCommandBase
	{
		private static int _isCommandInitialized = NOT_INITIALIZED;

		/// <summary>
		/// Suppress Diagnostic command ID.
		/// </summary>
		public const int SuppressDiagnosticCommandId = 0x0104;

		protected override bool CanModifyDocument => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="SuppressDiagnosticCommand"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		private SuppressDiagnosticCommand(Package package) : base(package, SuppressDiagnosticCommandId)
		{
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static SuppressDiagnosticCommand Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static void Initialize(Package package)
		{
			if (Interlocked.CompareExchange(ref _isCommandInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				Instance = new SuppressDiagnosticCommand(package);
			}
		}

		protected override void CommandCallback(object sender, EventArgs e)
		{
			IWpfTextView textView = ServiceProvider.GetWpfTextView();

			if (textView == null)
				return;

			SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
			ITextSnapshotLine caretLine = caretPosition.GetContainingLine();

			if (caretLine == null)
				return;

			Document document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
			if (document == null)
				return;

			(SyntaxNode syntaxRoot, SemanticModel semanticModel) = ThreadHelper.JoinableTaskFactory.Run(
				async () => (await document.GetSyntaxRootAsync(), await document.GetSemanticModelAsync()));

			if (syntaxRoot == null || semanticModel == null || !IsPlatformReferenced(semanticModel))
				return;

			TextSpan caretSpan = GetTextSpanFromCaret(caretPosition, caretLine);
			SyntaxNode syntaxNode = syntaxRoot.FindNode(caretSpan);
			SyntaxNode targetNode = SuppressionManager.FindTargetNode(syntaxNode);

			SuppressDiagnosticsOnTargetNode(targetNode, semanticModel);	
		}

		private TextSpan GetTextSpanFromCaret(SnapshotPoint caretPosition, ITextSnapshotLine caretLine)
		{
			if (caretLine.Length == 0)
			{
				return TextSpan.FromBounds(caretLine.Start.Position, caretLine.End.Position);
			}
			else if (caretPosition.Position < caretLine.End.Position)
			{
				var nextPoint = caretPosition.Add(1);
				return TextSpan.FromBounds(caretPosition.Position, nextPoint.Position);
			}
			else
			{
				var prevPoint = caretPosition.Add(-1);
				return TextSpan.FromBounds(prevPoint.Position, caretPosition.Position);
			}
		}

		private bool IsPlatformReferenced(SemanticModel semanticModel)
		{
			PXContext context = new PXContext(semanticModel.Compilation);
			return context.IsPlatformReferenced;
		}

		private void SuppressDiagnosticsOnTargetNode(SyntaxNode targetNode, SemanticModel semanticModel)
		{
			if (targetNode == null)
				return;




			//if (!targetNode.ContainsDiagnostics)
			//{
			//	MessageBox.Show(VSIXResource.DiagnosticSuppression_NoDiagnosticFound, AcuminatorVSPackage.PackageName);
			//	return false;
			//}

			//return true;
		}
	}
}
