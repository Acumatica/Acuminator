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



namespace Acuminator.Vsix.ChangesClassification
{
	/// <summary>
	/// A base class for document changes classification.
	/// </summary>
	public class DocumentChangesClassifier
	{
		public async Task<ChangeLocation> GetChangesLocationAsync(Document oldDocument, SyntaxNode oldRoot, Document newDocument, 
																  CancellationToken cancellationToken = default)
		{
			oldDocument.ThrowOnNull(nameof(oldDocument));
			oldRoot.ThrowOnNull(nameof(oldRoot));
			newDocument.ThrowOnNull(nameof(newDocument));

			IEnumerable<TextChange> textChanges = await newDocument.GetTextChangesAsync(oldDocument, cancellationToken)
																   .ConfigureAwait(false);
			if (textChanges.IsNullOrEmpty())
				return ChangeLocation.None;

			return GetChangesLocationImplAsync(oldDocument, oldRoot, newDocument, textChanges, cancellationToken);
		}

		protected virtual ChangeLocation GetChangesLocationImplAsync(Document oldDocument, SyntaxNode oldRoot, Document newDocument,
																	 IEnumerable<TextChange> textChanges, 
																	 CancellationToken cancellationToken = default)
		{
			ChangeLocation accumulatedChangeLocation = ChangeLocation.None;

			foreach (TextChange change in textChanges)
			{
				ChangeLocation changeLocation = GetTextChangeLocation(change, oldRoot);
				accumulatedChangeLocation = accumulatedChangeLocation | changeLocation;

				cancellationToken.ThrowIfCancellationRequested();
			}

			return accumulatedChangeLocation;
		}

		protected ChangeLocation GetTextChangeLocation(TextChange textChange, SyntaxNode oldRoot)
		{
			var containingNode = oldRoot.FindNode(textChange.Span);

			if (containingNode == null)
				return ChangeLocation.None;

			while (!containingNode.IsKind(SyntaxKind.CompilationUnit))
			{
				

				switch (containingNode)
				{
					case MethodDeclarationSyntax methodNode:

				}
			}

			return ChangeLocation.Namespace;
		}

		protected ChangeLocation? AnalyzeMethodNode(MethodDeclarationSyntax methodNode, TextChange textChange)
		{

		}
	}
}
