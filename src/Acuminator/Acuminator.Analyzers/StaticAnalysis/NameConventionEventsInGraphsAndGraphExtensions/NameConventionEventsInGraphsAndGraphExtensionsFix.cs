#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Symbols;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.NameConventionEventsInGraphsAndGraphExtensions
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class NameConventionEventsInGraphsAndGraphExtensionsFix : CodeFixProvider
	{
		private const string ArgsParameterName 			= "e";
		private const string EventHandlerMethodName 	= "_";
		private const string EventArgsCachePropertyName = "Cache"; // Events.[EventType]<T>.Cache

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1041_NameConventionEventsInGraphsAndGraphExtensions.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (context.Diagnostics.IsDefaultOrEmpty)
				return;

			var (semanticModel, root) = await context.Document.GetSemanticModelAndRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (semanticModel is null || root is null)
				return;

			var pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);  // Code fix doesn't rely on code analysis settings and can use defaults

			if (context.Diagnostics.Length == 1)
			{
				if (FixableDiagnosticIds.Contains(context.Diagnostics[0].Id))
					await RegisterCodeFixForDiagnosticAsync(context, pxContext, context.Diagnostics[0], root, semanticModel).ConfigureAwait(false);
			}
			else
			{
				var tasks = context.Diagnostics.Where(diagnostic => FixableDiagnosticIds.Contains(diagnostic.Id))
											   .Select(diagnostic => RegisterCodeFixForDiagnosticAsync(context, pxContext, diagnostic, root, semanticModel));
				await Task.WhenAll(tasks).ConfigureAwait(false);
			}
		}

		private Task RegisterCodeFixForDiagnosticAsync(CodeFixContext context, PXContext pxContext, Diagnostic diagnostic, SyntaxNode root, SemanticModel semanticModel)
		{
			
			context.CancellationToken.ThrowIfCancellationRequested(); 

			if (!diagnostic.TryGetPropertyValue(NameConventionEventsInGraphsAndGraphExtensionsDiagnosticProperties.EventType, out string? eventTypeStr) ||
				!Enum.TryParse(eventTypeStr, out EventType eventType) ||
				!diagnostic.TryGetPropertyValue(DiagnosticProperty.DacName, out string? dacName) || dacName.IsNullOrWhiteSpace())
			{
				return Task.CompletedTask;
			}

			diagnostic.TryGetPropertyValue(DiagnosticProperty.DacFieldName, out string? dacFieldName);

			var methodNode = root.FindNode(context.Span)?.ParentOrSelf<MethodDeclarationSyntax>();

			if (methodNode == null)
				return Task.CompletedTask;

			string codeActionName = nameof(Resources.PX1041Fix).GetLocalized().ToString();
			var codeAction = CodeAction.Create(codeActionName, ct => ChangeEventSignatureAsync(context.Document, root, semanticModel, eventType,
																							   dacName, dacFieldName, methodNode, pxContext, ct),
											   equivalenceKey: codeActionName);
			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private Task<Document> ChangeEventSignatureAsync(Document document, SyntaxNode root, SemanticModel semanticModel, EventType eventType, string dacName, 
														 string? dacFieldName, MethodDeclarationSyntax methodNode, PXContext pxContext, 
														 CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			IMethodSymbol? methodSymbol = semanticModel.GetDeclaredSymbol(methodNode, cancellationToken);
			
			if (methodSymbol is null || !methodSymbol.Name.EndsWith("_" + eventType, StringComparison.Ordinal) || methodSymbol.Parameters.IsDefaultOrEmpty)
				return Task.FromResult(document);

			// Get a corresponding generic event args symbol
			var genericEventInfoKey = new EventInfo(eventType, EventHandlerSignatureType.Generic);

			if (!pxContext.Events.EventHandlerSignatureTypeMap.TryGetValue(genericEventInfoKey, out INamedTypeSymbol genericArgsSymbol))
				return Task.FromResult(document);

			SeparatedSyntaxList<ParameterSyntax> methodParameters = methodNode.ParameterList.Parameters;

			var cacheParameterSymbol 		   = methodSymbol.Parameters[0];
			ParameterSyntax cacheParameterNode = methodParameters[0];
			SyntaxToken parameterName 		   = cacheParameterNode.Identifier;
			ParameterSyntax eventArgsParameter = CreateArgsParameter(genericArgsSymbol, parameterName, dacName, dacFieldName);

			var newParametersList = methodNode.ParameterList.WithParameters(
																SingletonSeparatedList(eventArgsParameter));
			SyntaxNode cacheParameterReplacement = GetCacheParameterReplacement(methodSymbol);
			var newMethodDeclaration = 
				ReplaceParameterUsages(methodNode, cacheParameterSymbol, cacheParameterReplacement, semanticModel, cancellationToken)
					.WithIdentifier(Identifier(EventHandlerMethodName))
					.WithParameterList(newParametersList);

			root = root.ReplaceNode(methodNode, newMethodDeclaration);
			var modifiedDocument = document.WithSyntaxRoot(root);
			return Task.FromResult(modifiedDocument);
		}

		private SyntaxNode GetCacheParameterReplacement(IMethodSymbol methodSymbol)
		{
			var parameters = methodSymbol.Parameters;
			IParameterSymbol? eventInfoParameter = parameters.Length > 1 
				? parameters[1] 
				: null;

			string newEventInfoName		  = eventInfoParameter?.Name.NullIfWhiteSpace() ?? ArgsParameterName;
			var cacheParameterReplacement = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
																IdentifierName(newEventInfoName), 
																IdentifierName(EventArgsCachePropertyName));
			return cacheParameterReplacement;
		}

		private ParameterSyntax CreateArgsParameter(INamedTypeSymbol genericArgsSymbol, SyntaxToken parameterName, string dacName, string? dacFieldName)
		{
			SeparatedSyntaxList<TypeSyntax> syntaxList;

			if (dacFieldName.IsNullOrWhiteSpace())
			{
				syntaxList = SingletonSeparatedList<TypeSyntax>(IdentifierName(dacName));
			}
			else
			{
				dacFieldName = dacFieldName.ToCamelCase();

				if (genericArgsSymbol.TypeParameters.Length == 2)
				{
					syntaxList = SeparatedList(
									new TypeSyntax[] 
									{ 
										IdentifierName(dacName), QualifiedName(IdentifierName(dacName), IdentifierName(dacFieldName)) 
									});
				}
				else
				{
					syntaxList = SingletonSeparatedList<TypeSyntax>(
						QualifiedName(IdentifierName(dacName), IdentifierName(dacFieldName)));
				}
			}

			var parameterType = 
				QualifiedName(
					IdentifierName(genericArgsSymbol.ContainingType.Name),
					GenericName(
							Identifier(genericArgsSymbol.Name))
						.WithTypeArgumentList(
							TypeArgumentList(syntaxList)));

			return Parameter(parameterName).WithType(parameterType);
		}

		private MethodDeclarationSyntax ReplaceParameterUsages(MethodDeclarationSyntax methodDeclaration, IParameterSymbol cacheParameter,
															   SyntaxNode replacement, SemanticModel semanticModel, CancellationToken cancellation)

		{
			var rewriter = new ParameterUsagesRewriter(cacheParameter, replacement, semanticModel, cancellation);
			return (MethodDeclarationSyntax)methodDeclaration.Accept(rewriter);
		}
	}
}