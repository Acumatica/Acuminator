using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.CodeGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

using PropertiesMap = System.Collections.Generic.Dictionary<string, (Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax RelatedProperty, int Index)>;
using BqlFieldWithPropertyAndIndex = (Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax NewBqlNode, 
									  Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax? RelatedProperty, 
									  int IndexToInsert);

namespace Acuminator.Analyzers.StaticAnalysis.MissingBqlFieldRedeclarationInDerived
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class MissingBqlFieldRedeclarationInDerivedDacFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			new[]
			{
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac_SingleField.Id,
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac_From_2_To_5_Fields.Id,
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac_MoreThan5Fields.Id,
			}
			.Distinct()
			.ToImmutableArray();

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.TryGetPropertyValue(DiagnosticProperty.DacName, out string? dacName) ||
				!diagnostic.TryGetPropertyValue(PX1067DiagnosticProperty.BqlFieldsWithBqlTypesData, out string? bqlFieldDataString) ||
				dacName.IsNullOrWhiteSpace() || bqlFieldDataString.IsNullOrWhiteSpace())
			{
				return Task.CompletedTask;
			}

			var notRedeclaredBqlFields = ParseBqlFieldDataString(bqlFieldDataString).OrderBy(field => field).ToList();

			if (notRedeclaredBqlFields.Count == 0)
				return Task.CompletedTask;

			string codeActionName = GetCodeActionName(dacName, notRedeclaredBqlFields);

			// Use the same equivalence key for all types of code actions to allow batch fixing them all
			var codeAction = CodeAction.Create(codeActionName,
											   cToken => RedeclareBqlFieldsAsync(context.Document, notRedeclaredBqlFields,
																				 context.Span, cToken),
											   equivalenceKey: FixableDiagnosticIds[0]);

			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private IEnumerable<(string BqlFieldName, string? BqlFieldTypeName)> ParseBqlFieldDataString(string bqlFieldDataString)
		{
			var separateFieldInfos = bqlFieldDataString.Split([Constants.FieldsSeparator], StringSplitOptions.RemoveEmptyEntries);

			if (separateFieldInfos.Length == 0)
				yield break;

			foreach (string fieldInfo in separateFieldInfos)
			{
				var fieldInfoParts = fieldInfo.Split([Constants.TypePartSeparator], StringSplitOptions.RemoveEmptyEntries);

				if (fieldInfoParts.Length == 2)
					yield return (BqlFieldName: fieldInfoParts[0], BqlFieldTypeName: fieldInfoParts[1].NullIfWhiteSpace());
			}
		}

		private string GetCodeActionName(string dacName, List<(string BqlFieldName, string? BqlFieldTypeName)> bqlFieldsToRedeclare)
		{
			switch (bqlFieldsToRedeclare.Count)
			{
				case 1:
					{
						string bqlFieldName = bqlFieldsToRedeclare[0].BqlFieldName;
						return nameof(Resources.PX1067SingleFieldFixFormat).GetLocalized(bqlFieldName, dacName).ToString();
					}
				case > 2 and <= Constants.FieldsNumberToCutOff:
					{
						string fieldNamesToDisplay = bqlFieldsToRedeclare.Select(field => $"\"{field.BqlFieldName}\"")
																		 .Join(", ");
						return nameof(Resources.PX1067From_2_To_5_FieldsFixFormat).GetLocalized(fieldNamesToDisplay, dacName).ToString();
					}
				default:
					{
						string fieldNamesToDisplay = bqlFieldsToRedeclare.Take(Constants.FieldsNumberToCutOff)
																		 .Select(field => $"\"{field.BqlFieldName}\"")
																		 .Join(", ");
						int remainingFieldsCount = bqlFieldsToRedeclare.Count - Constants.FieldsNumberToCutOff;
						string remainingFieldsArg = remainingFieldsCount == 1
							? Resources.PX1067MoreThan5Fields_RemainderSingleField
							: Resources.PX1067MoreThan5Fields_RemainderMultipleFields;

						return nameof(Resources.PX1067MoreThan5FieldsFixFormat)
									.GetLocalized(fieldNamesToDisplay, remainingFieldsCount.ToString(), remainingFieldsArg, dacName)
									.ToString();
					}
			}
		}

		private async Task<Document> RedeclareBqlFieldsAsync(Document document, List<(string BqlFieldName, string? BqlFieldTypeName)> bqlFieldsToRedeclare,
															 TextSpan span, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			SyntaxNode? nodeWithDiagnostic = root?.FindNode(span);

			if (nodeWithDiagnostic == null)
				return document;

			cancellationToken.ThrowIfCancellationRequested();

			var dacNode = nodeWithDiagnostic as ClassDeclarationSyntax;

			if (dacNode == null)
				return document;

			var newDacMembersInfo = GetDacMembersWithRedeclaredFields(document, dacNode, bqlFieldsToRedeclare, cancellationToken);

			if (newDacMembersInfo == null)
				return document;

			var (newDacMembers, insertedAtEnd) = newDacMembersInfo.Value;

			var newDacNode = AddMembersToDac(dacNode, newDacMembers, insertedAtEnd);
			var newRoot    = root!.ReplaceNode(dacNode, newDacNode);

			return document.WithSyntaxRoot(newRoot);
		}

		private (SyntaxList<MemberDeclarationSyntax>, bool InsertedAtEnd)? GetDacMembersWithRedeclaredFields(
																				Document document, ClassDeclarationSyntax dacNode,
																				List<(string BqlFieldName, string? BqlFieldTypeName)> bqlFieldsToRedeclare,
																				CancellationToken cancellationToken)
		{
			SyntaxList<MemberDeclarationSyntax> newDacMembersList;
			bool insertedAtEnd;

			if (dacNode.Members.Count > 0)
			{
				var relatedPropertiesWithIndexByFieldName = GetPropertiesWithIndexByFieldNames(bqlFieldsToRedeclare, dacNode, cancellationToken);
				var newDacMembersResult = 
					CreateBqlFieldsForDacWithMembers(bqlFieldsToRedeclare, dacNode, relatedPropertiesWithIndexByFieldName, cancellationToken);

				if (newDacMembersResult == null)
					return null;

				(newDacMembersList, insertedAtEnd) = newDacMembersResult.Value;
			}
			else
			{
				var newDacMembers = CreateBqlFieldsForDacWithoutMembers(bqlFieldsToRedeclare, cancellationToken);
				newDacMembersList = List(newDacMembers);
				insertedAtEnd	  = true;
			}

			return newDacMembersList.Count > 0
					? (newDacMembersList, insertedAtEnd)
					: null;
		}

		private IEnumerable<MemberDeclarationSyntax> CreateBqlFieldsForDacWithoutMembers(
																		List<(string BqlFieldName, string? BqlFieldTypeName)> bqlFieldsToRedeclare,
																		CancellationToken cancellationToken)
		{
			for (int i = 0; i < bqlFieldsToRedeclare.Count; i++)
			{
				var(bqlFieldName, bqlFieldTypeName)		 = bqlFieldsToRedeclare[i];
				BqlFieldTypeName? strongBqlFieldTypeName = bqlFieldTypeName != null
					? new BqlFieldTypeName(bqlFieldTypeName)
					: null;

				var newBqlFieldNode = CreateBqlFieldClassNode(adjacentMember: null, bqlFieldName, isFirstField: i == 0, strongBqlFieldTypeName);

				if (newBqlFieldNode != null)
					yield return newBqlFieldNode;
			}
		}

		private (SyntaxList<MemberDeclarationSyntax>, bool InsertedAtEnd)? CreateBqlFieldsForDacWithMembers(
																				List<(string BqlFieldName, string? BqlFieldTypeName)> bqlFieldsToRedeclare,
																				ClassDeclarationSyntax dacNode, PropertiesMap relatedPropertiesWithIndexByFieldName,
																				CancellationToken cancellationToken)
		{
			var newBqlFields = CreateBqlFieldNodesWithIndexes(bqlFieldsToRedeclare, dacNode, relatedPropertiesWithIndexByFieldName, cancellationToken);

			if (newBqlFields.Count == 0)
				return null;

			int dacMembersCount = dacNode.Members.Count;
			var newMembers = dacNode.Members;
			var orderedNewFields = newBqlFields.OrderByDescending(fieldWithIndex => fieldWithIndex.IndexToInsert)
											   .ToList(newBqlFields.Count);

			var lastFieldInsertedAtTheEnd  = orderedNewFields.LastOrDefault(field => field.IndexToInsert == dacMembersCount);
			bool insertedSomeFieldAtTheEnd = false;

			foreach (var (newBqlFieldNode, relatedProperty, indexToInsert) in orderedNewFields)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (relatedProperty != null)
				{
					var relatedPropertyWithoutRegions = CodeGeneration.RemoveRegionsFromLeadingTrivia(relatedProperty);

					// Do this two step replacement of relatedProperty indtead of ReplaceNode because relatedProperty won't be found in the new syntax tree
					newMembers = newMembers.RemoveAt(indexToInsert);
					newMembers = newMembers.Insert(indexToInsert, relatedPropertyWithoutRegions);
				}

				var newBqlFieldNodeWithTrivia = ReferenceEquals(lastFieldInsertedAtTheEnd.NewBqlNode, newBqlFieldNode)
					? CodeGeneration.CopyRegionsFromTrivia(newBqlFieldNode, dacNode.CloseBraceToken.LeadingTrivia)
					: newBqlFieldNode;

				bool insertAtTheEnd = indexToInsert == dacMembersCount;

				if (insertAtTheEnd)
				{
					newMembers = newMembers.Add(newBqlFieldNodeWithTrivia);
					insertedSomeFieldAtTheEnd = true;
				}
				else
					newMembers = newMembers.Insert(indexToInsert, newBqlFieldNodeWithTrivia);

				dacMembersCount++;
			}

			return (newMembers, insertedSomeFieldAtTheEnd);
		}

		private List<BqlFieldWithPropertyAndIndex> CreateBqlFieldNodesWithIndexes(List<(string BqlFieldName, string? BqlFieldTypeName)> bqlFieldsToRedeclare,
																		ClassDeclarationSyntax dacNode, PropertiesMap relatedPropertiesWithIndexByFieldName,
																		CancellationToken cancellationToken)
		{
			var newBqlFields = new List<BqlFieldWithPropertyAndIndex>(capacity: bqlFieldsToRedeclare.Count);
			var dacMembers   = dacNode.Members;

			foreach (var (bqlFieldName, bqlFieldTypeName) in bqlFieldsToRedeclare)
			{
				cancellationToken.ThrowIfCancellationRequested();

				BqlFieldTypeName? strongBqlFieldTypeName = !bqlFieldTypeName.IsNullOrWhiteSpace()
					? new BqlFieldTypeName(bqlFieldTypeName)
					: null;

				PropertyDeclarationSyntax? relatedPropertyNode;
				int indexToInsertBqlField;

				if (relatedPropertiesWithIndexByFieldName.TryGetValue(bqlFieldName, out var propertyWithIndex))
				{
					relatedPropertyNode   = propertyWithIndex.RelatedProperty;
					indexToInsertBqlField = propertyWithIndex.Index;
				}
				else
				{
					relatedPropertyNode   = null;
					indexToInsertBqlField = dacMembers.Count;   // insert BQL field redeclaration at the end of the DAC
				}

				var newBqlFieldNode = CreateBqlFieldClassNode(relatedPropertyNode, bqlFieldName,
															  isFirstField: indexToInsertBqlField == 0, strongBqlFieldTypeName);
				if (newBqlFieldNode != null)
				{
					newBqlFields.Add((newBqlFieldNode, relatedPropertyNode, indexToInsertBqlField));
				}
			}

			return newBqlFields;
		}

		private ClassDeclarationSyntax? CreateBqlFieldClassNode(MemberDeclarationSyntax? adjacentMember, string bqlFieldName,
																bool isFirstField, BqlFieldTypeName? bqlFieldTypeName)
		{
			if (bqlFieldTypeName.HasValue)
			{
				var stronglyTypedBqlFieldNode = BqlFieldGeneration.GenerateTypedBqlField(bqlFieldTypeName.Value, bqlFieldName, isFirstField,
																						 isRedeclaration: true, adjacentMember);
				return stronglyTypedBqlFieldNode;
			}
			else
			{
				var weaklyTypedBqlFieldNode = BqlFieldGeneration.GenerateWeaklyTypedBqlField(bqlFieldName, isFirstField, isRedeclaration: true,
																							 adjacentMember);
				return weaklyTypedBqlFieldNode;
			}
		}

		private PropertiesMap GetPropertiesWithIndexByFieldNames(List<(string BqlFieldName, string? BqlFieldTypeName)> bqlFieldsToRedeclare,
																 ClassDeclarationSyntax dacNode, CancellationToken cancellationToken)
		{
			var propertiesWithIndexByName = new PropertiesMap(StringComparer.OrdinalIgnoreCase);

			var dacMembers = dacNode.Members;

			if (dacMembers.Count == 0)
				return propertiesWithIndexByName;

			var namesOfBqlFieldsToRedeclare = bqlFieldsToRedeclare.Select(field => field.BqlFieldName)
																  .ToHashSet(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < dacMembers.Count; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (dacMembers[i] is not PropertyDeclarationSyntax dacPropertyNode)
					continue;

				string propertyName = dacPropertyNode.Identifier.Text;

				if (!namesOfBqlFieldsToRedeclare.Contains(propertyName) || propertiesWithIndexByName.ContainsKey(propertyName))
					continue;

				propertiesWithIndexByName.Add(propertyName, (dacPropertyNode, Index: i));
			}

			return propertiesWithIndexByName;
		}

		private ClassDeclarationSyntax AddMembersToDac(ClassDeclarationSyntax dacNode, SyntaxList<MemberDeclarationSyntax> newDacMembers, 
													   bool insertedAtEnd)
		{
			var newDacNode = dacNode.WithMembers(newDacMembers);

			if (insertedAtEnd)
			{
				var newCloseBraketTrivia = CodeGeneration.RemoveRegionsFromTrivia(newDacNode.CloseBraceToken.LeadingTrivia);

				if (newCloseBraketTrivia == null)
					return newDacNode;

				newDacNode = newDacNode.WithCloseBraceToken(
									newDacNode.CloseBraceToken.WithLeadingTrivia(newCloseBraketTrivia));
			}

			return newDacNode;
		}
	}
}