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

using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Document = Microsoft.CodeAnalysis.Document;
using Shell = Microsoft.VisualStudio.Shell;
using static Microsoft.VisualStudio.Shell.VsTaskLibraryHelper;
using Microsoft.VisualStudio.ComponentModelHost;

namespace Acuminator.Vsix.DiagnosticSuppression
{
	/// <summary>
	/// Suppress Diagnostic base command.
	/// </summary>
	internal abstract class SuppressDiagnosticCommandBase : VSCommandBase
	{
		protected SuppressDiagnosticCommandBase(Shell.AsyncPackage package, Shell.OleMenuCommandService commandService, int commandID) : 
										   base(package, commandService, commandID)
		{
		}

		protected override void CommandCallback(object sender, EventArgs e) =>
			CommandCallbackAsync()
				.FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{this.GetType().Name}");

		protected virtual async Task CommandCallbackAsync()
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
			SyntaxNode nodeWithDiagnostic = syntaxRoot.FindNode(caretSpan);

			if (nodeWithDiagnostic != null)
			{
				await SuppressDiagnosticsAsync(diagnosticData, document, syntaxRoot, semanticModel, nodeWithDiagnostic);
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

		protected bool IsPlatformReferenced(SemanticModel semanticModel)
		{
			PXContext context = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);
			return context.IsPlatformReferenced;
		}

		protected async Task<List<DiagnosticData>> GetDiagnosticsAsync(Document document, TextSpan caretSpan)
		{
			await Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			IComponentModel componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>();

			if (componentModel == null)
				return new List<DiagnosticData>();

			List<DiagnosticData> diagnosticData = null;

			try
			{
				var roslynService = RoslynDiagnosticService.Create(componentModel);

				if (roslynService != null)
				{
					diagnosticData = await roslynService.GetCurrentAcuminatorDiagnosticForDocumentSpanAsync(document, caretSpan,
																											Package.DisposalToken);
				}
			}
			catch
			{
				return new List<DiagnosticData>();
			}

			return diagnosticData ?? new List<DiagnosticData>();
		}

		protected virtual Task SuppressDiagnosticsAsync(List<DiagnosticData> diagnosticData, Document document, SyntaxNode syntaxRoot, 
														SemanticModel semanticModel, SyntaxNode nodeWithDiagnostic)
		{
			switch (diagnosticData.Count)
			{
				case 0:
					MessageBox.Show(VSIXResource.DiagnosticSuppression_NoDiagnosticFound, AcuminatorVSPackage.PackageName);
					return Task.CompletedTask;
				case 1:
					return SuppressSingleDiagnosticOnNodeAsync(diagnosticData[0], document, syntaxRoot, semanticModel, nodeWithDiagnostic);
				default:
					return SupressMultipleDiagnosticOnNodeAsync(diagnosticData, document, syntaxRoot, semanticModel, nodeWithDiagnostic);
			}
		}

		protected abstract Task SuppressSingleDiagnosticOnNodeAsync(DiagnosticData diagnostic, Document document, SyntaxNode syntaxRoot,
																	SemanticModel semanticModel, SyntaxNode nodeWithDiagnostic);

		protected abstract Task SupressMultipleDiagnosticOnNodeAsync(List<DiagnosticData> diagnosticData, Document document, SyntaxNode syntaxRoot,
																	 SemanticModel semanticModel, SyntaxNode nodeWithDiagnostic);
	}
}
