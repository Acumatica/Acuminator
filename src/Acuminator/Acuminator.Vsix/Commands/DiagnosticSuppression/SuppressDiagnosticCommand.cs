using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
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
			if (document == null || !document.SupportsSyntaxTree /*|| document.SourceCodeKind ==*/)
				return;

			(SyntaxNode syntaxRoot, SemanticModel semanticModel) = ThreadHelper.JoinableTaskFactory.Run(
				async () => (await document.GetSyntaxRootAsync(), await document.GetSemanticModelAsync()));

			if (syntaxRoot == null || semanticModel == null || !IsPlatformReferenced(semanticModel))
				return;


			TextSpan caretSpan = GetTextSpanFromCaret(caretPosition, caretLine);
			SyntaxNode syntaxNode = syntaxRoot.FindNode(caretSpan);
			SyntaxNode targetNode = SuppressionManager.FindTargetNode(syntaxNode);

			var componentModel = ServiceProvider.GetService(typeof(Microsoft.VisualStudio.ComponentModelHost.SComponentModel)) as Microsoft.VisualStudio.ComponentModelHost.IComponentModel;

			if (componentModel == null)
				return;

			var refAssemblyTypes = AppDomain.CurrentDomain.GetAssemblies()
														  .Where(a => a.FullName.StartsWith("Microsoft.CodeAnalysis"))
														  .SelectMany(a => a.GetTypes())
														  .Where(type => type.IsInterface && type.Name == "IDiagnosticAnalyzerService")
														  .ToArray();

			var diagnosticAnalyzerServiceType = refAssemblyTypes.FirstOrDefault();
			var diagnosticDataType = typeof(DocumentId).Assembly.GetTypes()																												
																.Where(type => type.Name == "DiagnosticData")
																.FirstOrDefault();	
			
			if (diagnosticAnalyzerServiceType == null || diagnosticDataType == null)
				return;

			var componentModelType = componentModel.GetType();
			var methodInfo = componentModelType.GetMethod(nameof(componentModel.GetService))
											  ?.MakeGenericMethod(diagnosticAnalyzerServiceType);
			
			if (methodInfo == null)
				return;

			try
			{
				var service = methodInfo.Invoke(componentModel, null);

				if (service == null)
					return;

				MethodInfo getDiagnosticMethod = diagnosticAnalyzerServiceType.GetMethod("GetDiagnosticsForSpanAsync");

				if (getDiagnosticMethod == null)
					return;

				dynamic dataTask = getDiagnosticMethod.Invoke(service, new object[] { document, caretSpan, null, false, CancellationToken.None });

				if (!(dataTask is System.Threading.Tasks.Task task))
					return;

				task.Wait();


				Type genericIEnumerableType = typeof(IEnumerable<>).MakeGenericType(diagnosticDataType);

				if (genericIEnumerableType == null)
					return;

				Type genericTask = typeof(System.Threading.Tasks.Task<>).MakeGenericType(genericIEnumerableType);
				var resultPropertyInfo = genericTask?.GetProperty(nameof(System.Threading.Tasks.Task<object>.Result));

				if (resultPropertyInfo == null)
					return;

				object diagnosticsCollectionRaw = resultPropertyInfo.GetValue(dataTask);

				if (!(diagnosticsCollectionRaw is IEnumerable<object> diagnostics) || diagnostics.IsNullOrEmpty())
					return;

				foreach (dynamic diagnostic in diagnostics)
				{
					var id = diagnostic.Id;
				}
			}
			catch (Exception exc)
			{

			
			}
			

			//SuppressDiagnosticsOnTargetNode(targetNode, semanticModelFromVS);	
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

		
	
		private List<IVsTaskItem> GetErrorList()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var errorService = ServiceProvider.GetService<SVsErrorList, IVsTaskList>();

			if (errorService == null)
				return null;

			int result = VSConstants.S_OK;
			List<IVsTaskItem> taskItemsList = new List<IVsTaskItem>(8);

			try
			{
				ErrorHandler.ThrowOnFailure(errorService.EnumTaskItems(out IVsEnumTaskItems errorItems));

				if (errorItems == null)
				{
					return null;
				}

				// Retrieve the task item text and check whether it is equal with one that supposed to be thrown.

				uint[] fetched = new uint[1];

				do
				{
					IVsTaskItem[] taskItems = new IVsTaskItem[1];

					result = errorItems.Next(1, taskItems, fetched);

					if (fetched[0] == 1 && taskItems[0] is IVsTaskItem2 taskItem)
					{
						taskItemsList.Add(taskItem);
					}

				}
				while (result == VSConstants.S_OK && fetched[0] == 1);

			}
			catch (System.Runtime.InteropServices.COMException e)
			{		
				result = e.ErrorCode;
			}

			if (result != VSConstants.S_OK)
			{
				return null;
			}

			return taskItemsList;
		}

		//private void SuppressDiagnosticsOnTargetNode(SyntaxNode targetNode, SemanticModel semanticModel)
		//{
		//	if (targetNode == null)
		//		return;

		//	bool x = targetNode.DescendantNodesAndTokens().Any(n => n.ContainsDiagnostics);

		//	var diagnostics = semanticModel.GetDiagnostics(targetNode.Span)
		//								   .Where(diagnostic => diagnostic.IsAcuminatorDiagnostic());

		//	if (!diagnostics.IsNullOrEmpty())
		//	{
		//		MessageBox.Show(VSIXResource.DiagnosticSuppression_NoDiagnosticFound, AcuminatorVSPackage.PackageName);
		//		return;
		//	}

		//	//return true;
		//}
	}
}
