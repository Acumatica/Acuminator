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
	internal sealed class SuppressDiagnosticCommand : SuppressDiagnosticCommandBase
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
		protected override void SuppressSingleDiagnosticOnNode(DiagnosticData diagnostic, Document document, SyntaxNode syntaxRoot,
															   SemanticModel semanticModel, SyntaxNode nodeWithDiagnostic)
		{
			SyntaxNode targetNode = SuppressionManager.FindTargetNode(nodeWithDiagnostic);

			if (targetNode == null)
				return;
		}

		protected override void SupressMultipleDiagnosticOnNode(List<DiagnosticData> diagnosticData, Document document, SyntaxNode syntaxRoot,
																SemanticModel semanticModel, SyntaxNode nodeWithDiagnostic)
		{
			MessageBox.Show(VSIXResource.DiagnosticSuppression_MultipleDiagnosticFound, AcuminatorVSPackage.PackageName);
		}
	}
}
