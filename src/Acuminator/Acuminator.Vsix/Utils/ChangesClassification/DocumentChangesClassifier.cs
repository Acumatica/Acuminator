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
		/// <summary>
		/// Values that represent span length changes for a <see cref="TextSpan"/>.
		/// </summary>
		protected enum SpanLengthChange : byte
		{
			NotChanged,
			Increased,
			Decreased
		} 

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

		protected virtual ChangeLocation GetTextChangeLocation(TextChange textChange, SyntaxNode oldRoot)
		{
			var containingNode = oldRoot.FindNode(textChange.Span);

			if (containingNode == null)
				return ChangeLocation.None;

			while (!containingNode.IsKind(SyntaxKind.CompilationUnit))
			{
				SpanLengthChange lengthChange = GetTextLengthChange(textChange, containingNode.Span);
				ChangeLocation? changesLocation = null;

				switch (containingNode)
				{
					case StatementSyntax statementNode:
						changesLocation = GetChangeLocationFromStatementNode(statementNode, textChange, lengthChange);
						break;
					case BaseMethodDeclarationSyntax baseMethodNode:
						changesLocation = GetChangeLocationFromMethodBaseSyntaxNode(baseMethodNode, textChange, lengthChange);
						break;
					case BasePropertyDeclarationSyntax basePropertyNode:
						changesLocation = GetChangeLocationFromPropertyBaseSyntaxNode(basePropertyNode, textChange, lengthChange);
						break;
				}
				
				if (changesLocation.HasValue)
					return changesLocation.Value;

				containingNode = containingNode.Parent;
			}

			return ChangeLocation.Namespace;
		}

		protected virtual ChangeLocation? GetChangeLocationFromStatementNode(StatementSyntax statementNode, TextChange textChange,
																			 SpanLengthChange lengthChange) =>
			GetDefaultChangeLocationFromLengthChanges(lengthChange, ChangeLocation.StatementsBlock);

		protected virtual ChangeLocation? GetChangeLocationFromMethodBaseSyntaxNode(BaseMethodDeclarationSyntax methodNodeBase, 
																					TextChange textChange, SpanLengthChange lengthChange)
		{
			TextSpan spanToCheck = new TextSpan(textChange.Span.Start, textChange.NewText.Length);

			if (methodNodeBase.AttributeLists.Span.Contains(spanToCheck))
			{
				return ChangeLocation.Attributes;
			}

			TextSpan? methodBodySpan = methodNodeBase.Body?.Span;

			if (methodBodySpan == null && methodNodeBase is MethodDeclarationSyntax methodNode)
			{
				methodBodySpan = methodNode.ExpressionBody?.Span;
			}
		
			return methodBodySpan == null
					? ChangeLocation.Class
					: methodBodySpan.Value.Contains(spanToCheck)
						? ChangeLocation.StatementsBlock
						: ChangeLocation.Class;
		}

		protected virtual ChangeLocation? GetChangeLocationFromPropertyBaseSyntaxNode(BasePropertyDeclarationSyntax propertyNodeBase, 
																					  TextChange textChange, SpanLengthChange lengthChange)
		{
			TextSpan spanToCheck = new TextSpan(textChange.Span.Start, textChange.NewText.Length);
			
			if (propertyNodeBase.AttributeLists.Span.Contains(spanToCheck))
			{
				return ChangeLocation.Attributes;
			}

			TextSpan? bodySpan = propertyNodeBase.AccessorList?.Span;

			if (bodySpan == null)
			{
				switch (propertyNodeBase)
				{
					case PropertyDeclarationSyntax property:
						bodySpan = property.ExpressionBody?.Span;
						break;
					case IndexerDeclarationSyntax indexer:
						bodySpan = indexer.ExpressionBody?.Span;
						break;
				}
			}

			return bodySpan == null
				? ChangeLocation.Class
				: bodySpan.Value.Contains(spanToCheck)
					? ChangeLocation.StatementsBlock
					: ChangeLocation.Class;
		}

		protected SpanLengthChange GetTextLengthChange(TextChange textChange, TextSpan existingContainingSpan)
		{
			var lengthFromChangeStart = existingContainingSpan.End - textChange.Span.Start;

			return textChange.NewText.Length == lengthFromChangeStart
				? SpanLengthChange.NotChanged
				: lengthFromChangeStart < textChange.NewText.Length
						? SpanLengthChange.Increased
						: SpanLengthChange.Decreased;
		}

		protected ChangeLocation? GetDefaultChangeLocationFromLengthChanges(SpanLengthChange lengthChange, ChangeLocation valueIfLengthNotIncreased)
		{
			switch (lengthChange)
			{
				case SpanLengthChange.NotChanged:
				case SpanLengthChange.Decreased:
					return valueIfLengthNotIncreased;
				case SpanLengthChange.Increased:
				default:
					return null;
			}
		}
	}
}
