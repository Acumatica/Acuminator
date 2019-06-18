using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
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
using Shell = Microsoft.VisualStudio.Shell;
using static Microsoft.VisualStudio.Shell.VsTaskLibraryHelper;

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
		public const int SuppressDiagnosticCommandId = 0x0201;

		/// <summary>
		/// Initializes a new instance of the <see cref="SuppressDiagnosticCommand"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		private SuppressDiagnosticCommand(Shell.AsyncPackage package, Shell.OleMenuCommandService commandService) : 
									 base(package, commandService, SuppressDiagnosticCommandId)
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
		/// <param name="commandService">The command service.</param>
		public static void Initialize(Shell.AsyncPackage package, Shell.OleMenuCommandService commandService)
		{
			if (Interlocked.CompareExchange(ref _isCommandInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				Instance = new SuppressDiagnosticCommand(package, commandService);
			}
		}

		protected override void CommandCallback(object sender, EventArgs e) =>
			CommandCallbackAsync()
				.FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(SuppressDiagnosticCommand)}");

		private async Task CommandCallbackAsync()
		{		
			IWpfTextView textView = await ServiceProvider.GetWpfTextViewAsync();

			if (textView == null)
				return;

			SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
			ITextSnapshotLine caretLine = caretPosition.GetContainingLine();

			if (caretLine == null)
				return;

			Document document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
			if (document == null || !document.SupportsSyntaxTree /*|| document.SourceCodeKind ==*/)
				return;

			Task<SyntaxNode> syntaxRootTask = document.GetSyntaxRootAsync(Package.DisposalToken);
			Task<SemanticModel> semanticModelTask = document.GetSemanticModelAsync(Package.DisposalToken);
			await Task.WhenAll(syntaxRootTask, semanticModelTask);

#pragma warning disable VSTHRD002, VSTHRD103 // Avoid problematic synchronous waits - the results are already obtained
			SyntaxNode syntaxRoot = syntaxRootTask.Result;
			SemanticModel semanticModel = semanticModelTask.Result;
#pragma warning restore VSTHRD002, VSTHRD103

			if (syntaxRoot == null || semanticModel == null || !IsPlatformReferenced(semanticModel) ||
				Package.DisposalToken.IsCancellationRequested)
			{
				return;
			}

			TextSpan caretSpan = GetTextSpanFromCaret(caretPosition, caretLine);
			List<DiagnosticData> diagnosticData = await GetDiagnosticsAsync(document, caretSpan);

			SyntaxNode syntaxNode = syntaxRoot.FindNode(caretSpan);
			 
			

			switch (diagnosticData.Count)
			{
				case 0:
					MessageBox.Show(VSIXResource.DiagnosticSuppression_NoDiagnosticFound, AcuminatorVSPackage.PackageName);
					return;
				case 1:
					SuppressDiagnosticsOnNode(syntaxNode, semanticModel, diagnosticData[0]);
					return;
				default:
					MessageBox.Show(VSIXResource.DiagnosticSuppression_MultipleDiagnosticFound, AcuminatorVSPackage.PackageName);
					return;
			}	
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
			PXContext context = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);
			return context.IsPlatformReferenced;
		}

		private async Task<List<DiagnosticData>> GetDiagnosticsAsync(Document document, TextSpan caretSpan)
		{
			List<DiagnosticData> diagnosticData = null;

			try
			{
				await Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package.DisposalToken);
				diagnosticData = await Shell.ThreadHelper.JoinableTaskFactory.RunAsync(() =>
					RoslynDiagnosticService.Instance.GetCurrentAcuminatorDiagnosticForDocumentSpanAsync(document, caretSpan));
			}
			catch
			{
				return new List<DiagnosticData>();
			}

			return diagnosticData ?? new List<DiagnosticData>();
		}

		


		private void SuppressDiagnosticsOnNode(SyntaxNode syntaxNode, SemanticModel semanticModel, DiagnosticData diagnostic)
		{
			SyntaxNode targetNode = SuppressionManager.FindTargetNode(syntaxNode);

			if (targetNode == null)
				return;

			
		}
	}
}
