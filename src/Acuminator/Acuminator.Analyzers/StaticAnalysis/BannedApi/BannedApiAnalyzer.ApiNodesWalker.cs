using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.BannedApi.ApiInfoRetrievers;
using Acuminator.Utilities.BannedApi.Model;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.BannedApi;

public partial class BannedApiAnalyzer
{
	private class ApiNodesWalker : CSharpSyntaxWalker
	{
		private readonly SyntaxNodeAnalysisContext _syntaxContext;
		private readonly PXContext _pxContext;

		private readonly IApiInfoRetriever _apiBanInfoRetriever;
		private readonly IApiInfoRetriever? _allowedInfoRetriever;
		private readonly BannedTypesInfoCollector _bannedTypesInfoCollector;

		private readonly HashSet<string> _namespacesWithUsedAllowedMembers = new();
		private readonly List<(UsingDirectiveSyntax Using, INamespaceSymbol Namespace, ApiSearchResult BanApiInfo)> _suspiciousUsings = new();

		private readonly HashSet<(Location ErrorLocation, Api ErrorInfo)> _reportedErrors = new();

		public bool CheckInterfaces { get; }

		private CancellationToken Cancellation => _syntaxContext.CancellationToken;

		private SemanticModel SemanticModel => _syntaxContext.SemanticModel;

		public ApiNodesWalker(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, IApiInfoRetriever apiBanInfoRetriever,
							  IApiInfoRetriever? allowedInfoRetriever, bool checkInterfaces)
		{
			_syntaxContext 			  = syntaxContext;
			_pxContext 				  = pxContext;
			_apiBanInfoRetriever 	  = apiBanInfoRetriever;
			_allowedInfoRetriever	  = allowedInfoRetriever;
			_bannedTypesInfoCollector = new BannedTypesInfoCollector(apiBanInfoRetriever, allowedInfoRetriever, syntaxContext.CancellationToken);
			CheckInterfaces 		  = checkInterfaces;
		}

		public void CheckSyntaxTree(CompilationUnitSyntax root)
		{
			root.Accept(this);

			if (_suspiciousUsings.Count == 0)
				return;

			if (_bannedTypesInfoCollector.NamespacesWithUsedAllowedMembers.Count > 0)
				_namespacesWithUsedAllowedMembers.AddRange(_bannedTypesInfoCollector.NamespacesWithUsedAllowedMembers);

			var usingsToReport = _namespacesWithUsedAllowedMembers.Count > 0
				? _suspiciousUsings.Where(usingInfo => !_namespacesWithUsedAllowedMembers.Contains(usingInfo.Namespace.ToString()))
				: _suspiciousUsings;

			foreach (var (@using, @namespace, banInfo) in usingsToReport)
			{
				ReportApi(@namespace, banInfo, @using.Name);
			}
		}

		#region Visit XML comments methods to prevent coloring in XML comments don't call base method
		public override void VisitXmlCrefAttribute(XmlCrefAttributeSyntax node) { }

		public override void VisitXmlComment(XmlCommentSyntax node) { }

		public override void VisitCrefBracketedParameterList(CrefBracketedParameterListSyntax node) { }

		public override void VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node) { }

		public override void VisitXmlElement(XmlElementSyntax node) { }

		public override void VisitXmlText(XmlTextSyntax node) { }
		#endregion

		public override void VisitPredefinedType(PredefinedTypeSyntax predefinedType)
		{
			// predefined primitives, such as "string" and "int", should be always compatible
		}

		public override void VisitUsingDirective(UsingDirectiveSyntax usingDirectiveNode)
		{
			Cancellation.ThrowIfCancellationRequested();

			if (usingDirectiveNode.Name == null ||
				SemanticModel.GetSymbolOrFirstCandidate(usingDirectiveNode.Name, Cancellation) is not ISymbol typeOrNamespaceSymbol)
			{
				return;
			}

			switch (typeOrNamespaceSymbol)
			{
				case INamespaceSymbol namespaceSymbol:
					if (_apiBanInfoRetriever.GetInfoForApi(namespaceSymbol) is ApiSearchResult bannedNamespaceInfo)
						_suspiciousUsings.Add((usingDirectiveNode, namespaceSymbol, bannedNamespaceInfo));

					break;

				case ITypeSymbol typeSymbol:
					var bannedTypeInfos = _bannedTypesInfoCollector.GetTypeBannedApiInfos(typeSymbol, CheckInterfaces);
					ReportApiList(typeSymbol, bannedTypeInfos, usingDirectiveNode.Name);
					break;
			}
		}

		/// <summary>
		/// Skip visit of the namespace declaration name.
		/// </summary>
		/// <param name="namespaceDeclaration">The namespace declaration.</param>
		public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax namespaceDeclaration)
		{
			Cancellation.ThrowIfCancellationRequested();

			foreach (var attributeList in namespaceDeclaration.AttributeLists)
				attributeList.Accept(this);

			foreach (var @using in namespaceDeclaration.Usings)
				@using.Accept(this);

			foreach (var @extern in namespaceDeclaration.Externs)
				@extern.Accept(this);

			foreach (var member in namespaceDeclaration.Members)
				member.Accept(this);
		}

		public override void VisitGenericName(GenericNameSyntax genericNameNode)
		{
			Cancellation.ThrowIfCancellationRequested();

			if (SemanticModel.GetSymbolOrFirstCandidate(genericNameNode, Cancellation) is not ISymbol symbol)
			{
				Cancellation.ThrowIfCancellationRequested();
				base.VisitGenericName(genericNameNode);
				return;
			}

			Cancellation.ThrowIfCancellationRequested();

			if (symbol is ITypeSymbol typeSymbol)
			{
				List<ApiSearchResult>? bannedTypeInfos = typeSymbol is ITypeParameterSymbol typeParameterSymbol
					? _bannedTypesInfoCollector.GetTypeParameterBannedApiInfos(typeParameterSymbol, CheckInterfaces)
					: _bannedTypesInfoCollector.GetTypeBannedApiInfos(typeSymbol, CheckInterfaces);

				if (bannedTypeInfos?.Count > 0)
				{
					var location = GetLocationFromNode(genericNameNode);
					ReportApiList(typeSymbol, bannedTypeInfos, location);
				}
			}
			else if (GetBannedSymbolInfoForNonTypeSymbol(symbol) is ApiSearchResult bannedSymbolInfo)
			{
				var location = GetLocationFromNode(genericNameNode);
				ReportApi(symbol, bannedSymbolInfo, location, checkIfAllowed: true);
			}

			Cancellation.ThrowIfCancellationRequested();
			base.VisitGenericName(genericNameNode);

			//----------------------------------------------------Local Function-----------------------------------------
			static Location GetLocationFromNode(GenericNameSyntax genericNameNode)
			{
				var typeName = genericNameNode.Identifier.ValueText;
				return !typeName.IsNullOrWhiteSpace()
					? (genericNameNode.Identifier.GetLocation().NullIfLocationKindIsNone() ?? genericNameNode.GetLocation())
					: genericNameNode.GetLocation();
			}
		}

		public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax conditionalAccessExpression)
		{
			bool visitChildNodes =
				AnalyzeAccessExpressionAndDecideIfShouldVisitChildNodes(conditionalAccessExpression, conditionalAccessExpression.Expression, 
																		conditionalAccessExpression.WhenNotNull);
			if (visitChildNodes)
				base.VisitConditionalAccessExpression(conditionalAccessExpression);
		}

		public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpression)
		{
			bool visitChildNodes =
				AnalyzeAccessExpressionAndDecideIfShouldVisitChildNodes(memberAccessExpression, memberAccessExpression.Expression, 
																		memberAccessExpression.Name);
			if (visitChildNodes)
				base.VisitMemberAccessExpression(memberAccessExpression);
		}

		private bool AnalyzeAccessExpressionAndDecideIfShouldVisitChildNodes(ExpressionSyntax wholeAccessExpression, ExpressionSyntax expressionBeingAccessed,
																			 ExpressionSyntax accessMemberExpression)
		{
			Cancellation.ThrowIfCancellationRequested();

			ISymbol? accessedMember = SemanticModel.GetSymbolOrFirstCandidate(wholeAccessExpression, Cancellation);
			accessedMember ??= SemanticModel.GetSymbolOrFirstCandidate(accessMemberExpression, Cancellation);

			if (accessedMember == null)
				return true;

			Cancellation.ThrowIfCancellationRequested();

			if (CheckSymbolForBannedInfo(accessedMember, accessMemberExpression))
				return false;

			expressionBeingAccessed = UnwrapAccessExpressionFromArrayAccess(expressionBeingAccessed);
			var symbolBeingAccessed = SemanticModel.GetSymbolOrFirstCandidate(expressionBeingAccessed, Cancellation);
			
			if (symbolBeingAccessed == null || symbolBeingAccessed.Equals(accessedMember.ContainingType, SymbolEqualityComparer.Default))
				return !IsAllowedApi(accessedMember);
			
			Cancellation.ThrowIfCancellationRequested();

			var typeInfo = SemanticModel.GetTypeInfo(expressionBeingAccessed, Cancellation);
			var typeOfContainingSymbol = typeInfo.Type;

			if (typeOfContainingSymbol != null && CheckSymbolForBannedInfo(typeOfContainingSymbol, expressionBeingAccessed))
				return false;

			Cancellation.ThrowIfCancellationRequested();
			return true;
		}

		private ExpressionSyntax UnwrapAccessExpressionFromArrayAccess(ExpressionSyntax expressionBeingAccessed) =>
			(expressionBeingAccessed as ElementAccessExpressionSyntax)?.Expression ?? expressionBeingAccessed;

		public override void VisitIdentifierName(IdentifierNameSyntax identifierNode)
		{
			Cancellation.ThrowIfCancellationRequested();

			if (SemanticModel.GetSymbolOrFirstCandidate(identifierNode, Cancellation) is not ISymbol symbol)
				return;

			Cancellation.ThrowIfCancellationRequested();
			CheckSymbolForBannedInfo(symbol, identifierNode);
		}

		public override void VisitQualifiedName(QualifiedNameSyntax qualifiedName)
		{
			Cancellation.ThrowIfCancellationRequested();

			if (SemanticModel.GetSymbolOrFirstCandidate(qualifiedName, Cancellation) is not ISymbol symbol)
			{
				base.VisitQualifiedName(qualifiedName);
				return;
			}

			Cancellation.ThrowIfCancellationRequested();
			CheckSymbolForBannedInfo(symbol, qualifiedName.Right);
		}

		private bool CheckSymbolForBannedInfo(ISymbol symbol, SyntaxNode nodeToReport)
		{
			switch (symbol)
			{
				case ITypeParameterSymbol typeParameterSymbol:
					var bannedTypeParameterInfos = _bannedTypesInfoCollector.GetTypeParameterBannedApiInfos(typeParameterSymbol, CheckInterfaces);			
					return ReportApiList(typeParameterSymbol, bannedTypeParameterInfos, nodeToReport);

				case ITypeSymbol typeSymbol:
					var bannedTypeInfos = _bannedTypesInfoCollector.GetTypeBannedApiInfos(typeSymbol, CheckInterfaces);
					return ReportApiList(typeSymbol, bannedTypeInfos, nodeToReport);

				default:
					if (GetBannedSymbolInfoForNonTypeSymbol(symbol) is ApiSearchResult bannedSymbolInfo)
						return ReportApi(symbol, bannedSymbolInfo, nodeToReport);

					return false;
			}
		}

		private ApiSearchResult? GetBannedSymbolInfoForNonTypeSymbol(ISymbol nonTypeSymbol)
		{
			if (_apiBanInfoRetriever.GetInfoForApi(nonTypeSymbol) is ApiSearchResult bannedSymbolInfo)
				return bannedSymbolInfo;

			if (nonTypeSymbol.IsOverride)
			{
				var overridesChain = nonTypeSymbol.GetOverridden();

				foreach (var overridenSymbol in overridesChain)
				{
					if (_apiBanInfoRetriever.GetInfoForApi(overridenSymbol) is ApiSearchResult bannedOverridenSymbolInfo)
						return bannedOverridenSymbolInfo;
				}
			}

			return null;
		}

		private bool ReportApiList(ISymbol symbolToReport, List<ApiSearchResult>? bannedApisList, SyntaxNode node)
		{
			if (bannedApisList?.Count > 0)
				return ReportApiList(symbolToReport, bannedApisList, node.GetLocation());

			return false;
		}

		private bool ReportApiList(ISymbol symbolToReport, List<ApiSearchResult> bannedApisList, Location location)
		{
			if (IsAllowedApi(symbolToReport))
				return false;

			bool anythingReported = false;

			foreach (ApiSearchResult bannedApiInfo in bannedApisList)
			{
				anythingReported = ReportApi(symbolToReport, bannedApiInfo, location, checkIfAllowed: false) || anythingReported;
			}

			return anythingReported;
		}

		private bool ReportApi(ISymbol symbolToReport, ApiSearchResult banApiInfo, SyntaxNode? node) =>
			ReportApi(symbolToReport, banApiInfo, node?.GetLocation(), checkIfAllowed: true);

		private bool ReportApi(ISymbol symbolToReport, ApiSearchResult banApiInfo, Location? location, bool checkIfAllowed)
		{
			if (checkIfAllowed && IsAllowedApi(symbolToReport))
				return false;

			if (location != null && !_reportedErrors.Add((location, banApiInfo.ClosestBannedApi)!))
				return false;

			var apiKindDescription = GetApiKindLocalizedDescription(banApiInfo.ClosestBannedApi.Kind);
			Diagnostic diagnostic;

			if (!banApiInfo.ClosestBannedApi.BanReason.IsNullOrWhiteSpace())
			{
				diagnostic = Diagnostic.Create(
								Descriptors.PX1099_ForbiddenApiUsage_WithReason, location, 
								apiKindDescription, banApiInfo.ClosestBannedApiSymbolName, banApiInfo.ClosestBannedApi.BanReason);
			}
			else
			{
				diagnostic = Diagnostic.Create(
								Descriptors.PX1099_ForbiddenApiUsage_WithoutReason, location, 
								apiKindDescription, banApiInfo.ClosestBannedApiSymbolName);
			}

			_syntaxContext.ReportDiagnosticWithSuppressionCheck(diagnostic, _pxContext.CodeAnalysisSettings);
			return true;
		}

		private static string GetApiKindLocalizedDescription(ApiKind apiKind) => apiKind switch
		{
			ApiKind.Type 	  => Resources.PX1099Title_TypeFormatArg,
			ApiKind.Namespace => Resources.PX1099Title_NamespaceFormatArg,
			ApiKind.Property  => Resources.PX1099Title_PropertyFormatArg,
			ApiKind.Method 	  => Resources.PX1099Title_MethodFormatArg,
			ApiKind.Event 	  => Resources.PX1099Title_EventFormatArg,
			ApiKind.Field 	  => Resources.PX1099Title_FieldFormatArg,
			_ 				  => string.Empty
		};

		private bool IsAllowedApi(ISymbol symbol)
		{
			if (_allowedInfoRetriever?.GetInfoForApi(symbol) is ApiSearchResult)
			{
				if (symbol.ContainingNamespace != null && !symbol.ContainingNamespace.IsGlobalNamespace)
					_namespacesWithUsedAllowedMembers.Add(symbol.ContainingNamespace.ToString());

				return true;
			}

			return false;
		}
	}
}