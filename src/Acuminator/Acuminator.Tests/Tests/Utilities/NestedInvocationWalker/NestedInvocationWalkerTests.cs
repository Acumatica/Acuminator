#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

using static Acuminator.Tests.Verification.VerificationHelper;

namespace Acuminator.Tests.Tests.Utilities.NestedInvocationWalker
{
	public class NestedInvocationWalkerTests
	{
		private class ExceptionWalker : Acuminator.Utilities.Roslyn.NestedInvocationWalker
		{
			private static readonly DiagnosticDescriptor DiagnosticDescriptor =
				new DiagnosticDescriptor("PX9999", "Test", "Test", "Default", DiagnosticSeverity.Error, true);

			private readonly List<Location> _locations = new List<Location>();
			public IReadOnlyList<Location> Locations => _locations;

			public ExceptionWalker(Compilation compilation, CancellationToken cancellationToken) 
				: base(new PXContext(compilation, CodeAnalysisSettings.Default), cancellationToken)
			{
			}

			public override void VisitThrowStatement(ThrowStatementSyntax node)
			{
				base.VisitThrowStatement(node);
				ReportDiagnostic(AddDiagnostic, DiagnosticDescriptor, node);
			}

			public override void VisitThrowExpression(ThrowExpressionSyntax node)
			{
				base.VisitThrowExpression(node);
				ReportDiagnostic(AddDiagnostic, DiagnosticDescriptor, node);
			}

			private void AddDiagnostic(Diagnostic diagnostic)
			{
				_locations.Add(diagnostic.Location);
			}
		}

		[Theory]
		[EmbeddedFileData("SanityCheck.cs")]
		public async Task SanityCheck(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode) await document.GetSyntaxRootAsync();

			node.Accept(walker);
			
			walker.Locations.Should().BeEquivalentTo((Line: 13, Column: 4));
		}

		[Theory]
		[EmbeddedFileData("StaticMethod.cs")]
		public async Task StaticMethod(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode) (await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((Line: 13, Column: 4));
		}

		[Theory]
		[EmbeddedFileData("PropertyGetter.cs")]
		public async Task PropertyGetter(string text) //-V3013
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((Line: 14, Column: 16));
		}

		[Theory]
		[EmbeddedFileData("PropertyGetterConditionalAccess.cs")]
		public async Task PropertyGetterConditionalAccess(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((Line: 14, Column: 16));
		}

		[Theory]
		[EmbeddedFileData("PropertySetter.cs")]
		public async Task PropertySetter(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((Line: 14, Column: 4));
		}

		[Theory]
		[EmbeddedFileData("PropertySetterFromInitializer.cs")]
		public async Task PropertySetterFromInitializer(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((Line: 13, Column: 26));
		}

		[Theory]
		[EmbeddedFileData("PropertyValid.cs")]
		public async Task Property_ShouldNotFindAnything(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEmpty();
		}

		[Theory]
		[EmbeddedFileData("Constructor.cs")]
		public async Task Constructor(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((Line: 13, Column: 14));
		}

		[Theory(Skip = "The test is incorrect for current implementation of nested walker which does not step inside lambdas")]
		[EmbeddedFileData("LocalLambda.cs")]
		public async Task LocalLambda(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((Line: 13, Column: 20));
		}

		[Theory]
		[EmbeddedFileData("InstanceMethod.cs")]
		public async Task InstanceMethod(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((Line: 14, Column: 4));
		}

		[Theory]
		[EmbeddedFileData("InstanceExpressionBodiedMethod.cs")]
		public async Task InstanceExpressionBodiedMethod(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((Line: 14, Column: 4));
		}

		[Theory]
		[EmbeddedFileData("InstanceMethodConditionalAccess.cs")]
		public async Task InstanceMethodConditionalAccess(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((Line: 14, Column: 4));
		}

		[Theory]
		[EmbeddedFileData("SeparateFiles_AnalyzedClass.cs", "SeparateFiles_ExternalClass.cs")]
		public async Task SeparateFiles(string text1, string text2)
		{
			Document document = CreateCSharpDocument(text1, text2);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First(n => n.Identifier.Text == "Foo");

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((Line: 14, Column: 4));
		}

		[Theory]
		[EmbeddedFileData("MultipleReportedDiagnostics.cs")]
		public async Task MultipleReportedDiagnostics(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((Line: 14, Column: 4));
		}
	}
}
