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
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.PropertyAndBqlFieldTypesMismatch
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class PropertyAndBqlFieldTypesMismatchFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.Id);

		protected override async Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.TryGetPropertyValue(DiagnosticProperty.PropertyType, out string? propertyDataTypeName) ||
				!diagnostic.TryGetPropertyValue(DiagnosticProperty.BqlFieldDataType, out string? bqlFieldDataTypeName))
			{
				return;
			}

			bqlFieldDataTypeName = bqlFieldDataTypeName.NullIfWhiteSpace();
			propertyDataTypeName = propertyDataTypeName.NullIfWhiteSpace();

			bool isOnPropertyNode = diagnostic.IsFlagSet(DiagnosticProperty.IsProperty);

			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var nodeWithDiagnostic = root?.FindNode(context.Span);

			if (nodeWithDiagnostic == null)
				return;

			if (isOnPropertyNode)
				RegisterFixPropertyTypeAction(context, diagnostic, root!, nodeWithDiagnostic, bqlFieldDataTypeName);
			else
				RegisterFixBqlFieldTypeAction(context, diagnostic, root!, nodeWithDiagnostic, propertyDataTypeName);
		}

		private void RegisterFixPropertyTypeAction(CodeFixContext context, Diagnostic diagnostic, SyntaxNode root, SyntaxNode nodeWithDiagnostic,
												   string? bqlFieldDataTypeName)
		{
			var propertyNode = nodeWithDiagnostic.ParentOrSelf<PropertyDeclarationSyntax>();

			if (propertyNode == null || bqlFieldDataTypeName.IsNullOrWhiteSpace()) 
				return;

			string codeActionName = nameof(Resources.PX1068FixPropertyType).GetLocalized().ToString();
			Document document 	  = context.Document;

			var codeAction = CodeAction.Create(codeActionName,
											   cToken => ChangePropertyTypeToBqlFieldType(document, root, propertyNode, bqlFieldDataTypeName, cToken),
											   equivalenceKey: codeActionName);
			context.RegisterCodeFix(codeAction, diagnostic);
		}

		private void RegisterFixBqlFieldTypeAction(CodeFixContext context, Diagnostic diagnostic, SyntaxNode root, SyntaxNode nodeWithDiagnostic,
												   string? propertyDataTypeName)
		{
			string? bqlFieldTypeName = GetBqlFieldTypeFromPropertyDataType(propertyDataTypeName);
			
			if (bqlFieldTypeName == null)
				return;

			string codeActionName = nameof(Resources.PX1068FixBqlType).GetLocalized().ToString();
			Document document	  = context.Document;

			var codeAction = CodeAction.Create(codeActionName,
											   cToken => ChangeBqlFieldTypeToPropertyType(document, root, nodeWithDiagnostic, bqlFieldTypeName, cToken),
											   equivalenceKey: codeActionName);
			context.RegisterCodeFix(codeAction, diagnostic);
		}

		private string? GetBqlFieldTypeFromPropertyDataType(string? propertyDataTypeName)
		{
			if (propertyDataTypeName.IsNullOrWhiteSpace())
				return null;

			var dataTypeName = new DataTypeName(propertyDataTypeName);
			string? mappedBqlFieldType = DataTypeToBqlFieldTypeMapping.GetBqlFieldType(dataTypeName);

			return mappedBqlFieldType;
		}

		private async Task<Document> ChangePropertyTypeToBqlFieldType(Document document, SyntaxNode root, PropertyDeclarationSyntax propertyNode,
																	  string bqlFieldDataTypeName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

		
		
		}

		private async Task<Document> ChangeBqlFieldTypeToPropertyType(Document document, SyntaxNode root, SyntaxNode nodeWithDiagnostic,
																	  string bqlFieldTypeName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			bool isInBaseList = nodeWithDiagnostic.ParentOrSelf<BaseListSyntax>() != null;

		}
	}
}