using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.ChangesClassification;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A class for Code Map specific document changes classification. Contains optimizations for code map.
	/// </summary>
	internal class CodeMapDocChangesClassifier : DocumentChangesClassifier
	{
		public async Task<CodeMapRefreshMode> ShouldRefreshCodeMapAsync(Document oldDocument, SyntaxNode newRoot, Document newDocument,
																		CancellationToken cancellationToken = default)
		{
			ChangeLocation changeLocation = await GetChangesLocationAsync(oldDocument, newRoot, newDocument, cancellationToken);

			if (changeLocation == ChangeLocation.Class || changeLocation == ChangeLocation.Namespace)
			{
				return newRoot.ContainsDiagnostics
					? CodeMapRefreshMode.Clear
					: CodeMapRefreshMode.Recalculate;
			}
			else
			{
				return CodeMapRefreshMode.NoRefresh;
			}
		}

		protected override ChangeLocation GetChangesLocationImplAsync(Document oldDocument, SyntaxNode newRoot, Document newDocument,
																	  IEnumerable<TextChange> textChanges, CancellationToken cancellationToken = default)
		{
			ChangeLocation accumulatedChangeLocation = ChangeLocation.None;

			foreach (TextChange change in textChanges)
			{
				ChangeLocation changeLocation = GetTextChangeLocation(change, newRoot);

				//Early exit if we found a change which require the refresh of code map 
				if (changeLocation.ContainsLocation(ChangeLocation.Class) || changeLocation.ContainsLocation(ChangeLocation.Namespace))
					return changeLocation;

				accumulatedChangeLocation = accumulatedChangeLocation | changeLocation;
				cancellationToken.ThrowIfCancellationRequested();
			}

			return accumulatedChangeLocation;
		}
	}
}
