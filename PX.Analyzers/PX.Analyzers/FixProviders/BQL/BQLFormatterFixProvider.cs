//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Composition;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CodeActions;
//using Microsoft.CodeAnalysis.CodeFixes;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.Formatting;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Editing;

//using PX.Analyzers.Analyzers.BQL;

//namespace PX.Analyzers.FixProviders
//{
//	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
//	public class BQLFormatterFixProvider : CodeFixProvider
//	{
//		public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Descriptors.PXF1001_PXBadBqlFormat.Id);

//		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

//		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
//		{
//			SyntaxNode root = await context.Document.GetSyntaxRootAsync().ConfigureAwait(false);
//			SyntaxNode node = root.FindNode(context.Span);
//			string title = nameof(Resources.PXF1001Fix).GetLocalized().ToString();

//			if (node == null)
//				return;
			
//			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

//			context.RegisterCodeFix(CodeAction.Create(title, c =>
//			{
//				var pxContext = new PXContext(semanticModel.Compilation);
//				var rewriter = new GenericNameRewriter(pxContext, context.Document, semanticModel);
//				var newNode = rewriter.Visit(node);
//				newNode = Formatter.Format(newNode, new AdhocWorkspace());
//				return Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(node, newNode)));
//			}, title),
//			context.Diagnostics);
//		}

//		private class GenericNameRewriter : CSharpSyntaxRewriter
//		{
//			private readonly PXContext pxContext;
//			private readonly Document document;
//			private readonly SemanticModel semanticModel;
//			private readonly SyntaxGenerator generator;

//			public GenericNameRewriter(PXContext aPxContext, Document aDocument, SemanticModel aSemanticModel)
//			{
//				pxContext = aPxContext;
//				document = aDocument;
//				semanticModel = aSemanticModel;
//				generator = SyntaxGenerator.GetGenerator(document);
//			}
			
//			public override SyntaxNode VisitGenericName(GenericNameSyntax genericNode)
//			{
//				if (!genericNode.CheckGenericNodeParentKind())
//					return base.VisitGenericName(genericNode);
						
//				ITypeSymbol typeSymbol = semanticModel.GetSymbolInfo(genericNode).Symbol as ITypeSymbol;

//				if (typeSymbol == null || !typeSymbol.InheritsFrom(pxContext.BQL.PXSelectBase))
//					return base.VisitGenericName(genericNode);

//				return GetFormatted(genericNode);
				
//				//return generator.GenericName(genericNode.Identifier.ValueText, typeArgs)
//				//				.WithLeadingTrivia(genericNode.GetLeadingTrivia());
//			}

//			private SyntaxNode GetFormatted(GenericNameSyntax genericNode)
//			{
                 
//				var typeArgs = genericNode.TypeArgumentList.Arguments;
//				List<SyntaxNode> newTypeArgs = new List<SyntaxNode>(typeArgs.Count);

//				if (typeArgs.Count == 0)
//					return genericNode;

//				if (typeArgs.Count == 1)
//				{
//					newTypeArgs.Add(typeArgs[0]);
//					return genericNode;
//				}

//				//MainDacRewriter mainDacVisitor = new MainDacRewriter(pxContext, document, semanticModel, generator);
//				//newTypeArgs.Add(mainDacVisitor.Visit(typeArgs[0]));

//				//OperandDacRewriter opRewriter = new OperandDacRewriter(pxContext, document, semanticModel, generator);

//				//for (int i = 1; i < typeArgs.Count; i++)
//				//{

//				//}

//				var argsList = genericNode.TypeArgumentList.Arguments;

//				foreach (var comma in genericNode.TypeArgumentList.Arguments.GetSeparators().Where(sep => !sep.HasExactlyOneEOL()))
//				{
//					argsList = argsList.ReplaceSeparator(comma, comma.WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine)));
//				}

//				genericNode = genericNode.WithTypeArgumentList(
//					genericNode.TypeArgumentList.WithArguments(argsList)
//					);
				 
//				return genericNode;// typeArgs.ToArray();
//			}

//            //private SyntaxTriviaList GetStartingTrivia(GenericNameSyntax genericNode)
//            //{
//            //    if (genericNode.ContainsAnnotations 
//            //}
//        }

//		//private class MainDacRewriter : CSharpSyntaxRewriter
//		//{
//		//	private readonly PXContext pxContext;
//		//	private readonly Document document;
//		//	private readonly SemanticModel semanticModel;
//		//	private readonly SyntaxGenerator generator;

//		//	public MainDacRewriter(PXContext aPxContext, Document aDocument, SemanticModel aSemanticModel, SyntaxGenerator aGenerator)
//		//	{
//		//		pxContext = aPxContext;
//		//		document = aDocument;
//		//		semanticModel = aSemanticModel;
//		//		generator = aGenerator;
//		//	}

//		//	public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax mainDac)
//		//	{
//		//		ITypeSymbol typeSymbol = semanticModel.GetSymbolInfo(mainDac).Symbol as ITypeSymbol;

//		//		if (typeSymbol == null || !typeSymbol.InheritsFrom(pxContext.IBqlTableType))
//		//			return base.VisitIdentifierName(mainDac);

//		//		if (mainDac.HasExactlyOneEOL())
//		//			return base.VisitIdentifierName(mainDac);

//		//		return generator.IdentifierName(mainDac.Identifier.ValueText)
//		//						.WithLeadingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine));
//		//	}
//		//}


//		//private class OperandDacRewriter : CSharpSyntaxRewriter
//		//{
//		//	private readonly PXContext pxContext;
//		//	private readonly Document document;
//		//	private readonly SemanticModel semanticModel;
//		//	private readonly SyntaxGenerator generator;

//		//	public OperandDacRewriter(PXContext aPxContext, Document aDocument, SemanticModel aSemanticModel, SyntaxGenerator aGenerator)
//		//	{
//		//		pxContext = aPxContext;
//		//		document = aDocument;
//		//		semanticModel = aSemanticModel;
//		//		generator = aGenerator;
//		//	}

//		//	public override SyntaxNode VisitGenericName(GenericNameSyntax operand)
//		//	{
//		//		ITypeSymbol typeSymbol = semanticModel.GetSymbolInfo(operand).Symbol as ITypeSymbol;

//		//		if (typeSymbol == null)
//		//			return base.VisitGenericName(operand);

//		//		if (typeSymbol.InheritsFrom(pxContext.BQL.OrderBy))
//		//			return VisitOrderBy(operand);

//		//		if (mainDac.HasExactlyOneEOL())
//		//			return base.VisitGenericName(mainDac);

//		//		return generator.IdentifierName(mainDac.Identifier.ValueText)
//		//						.WithLeadingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine));
//		//	}

//		//	private SyntaxNode VisitOrderBy(GenericNameSyntax operand)
//		//	{
//		//		if (operand.TypeArgumentList.Arguments.Count == 0)
//		//			return VisitOrderBy(operand);

//		//		SyntaxToken? lessThan = null;

//		//		if (!operand.TypeArgumentList.LessThanToken.HasExactlyOneEOL())
//		//		{
//		//			lessThan = operand.TypeArgumentList.LessThanToken.WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine));
//		//		}

//		//		if (lessThan.HasValue)
//		//		{
//		//			operand = operand.WithTypeArgumentList(
//		//				operand.TypeArgumentList.WithLessThanToken(
//		//					lessThan.Value));
//		//		}

//		//		OperandDacRewriter ascRewriter = new OperandDacRewriter(pxContext, document, semanticModel, generator);
//		//		TypeSyntax askNode = ascRewriter.Visit(operand.TypeArgumentList.Arguments[0]) as TypeSyntax;

//		//		if (askNode == null)
//		//			return operand;

//		//		return operand.WithTypeArgumentList(
//		//			operand.TypeArgumentList.WithArguments(
//		//				operand.TypeArgumentList.Arguments.Replace(
//		//					operand.TypeArgumentList.Arguments[0], askNode)));
//		//	}
//		//}
//	}
//}
