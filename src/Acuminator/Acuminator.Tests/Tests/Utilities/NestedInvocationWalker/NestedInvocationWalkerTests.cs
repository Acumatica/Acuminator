using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using static Acuminator.Tests.Verification.VerificationHelper;

namespace Acuminator.Tests.Tests.Utilities.NestedInvocationWalker
{
	public class NestedInvocationWalkerTests : DiagnosticVerifier
	{
		private class ExceptionWalker : Acuminator.Utilities.Roslyn.NestedInvocationWalker
		{
			private static readonly DiagnosticDescriptor DiagnosticDescriptor =
				new DiagnosticDescriptor("PX9999", "Test", "Test", "Default", DiagnosticSeverity.Error, true);

			private readonly List<Location> _locations = new List<Location>();
			public IReadOnlyList<Location> Locations => _locations;

			public ExceptionWalker(Compilation compilation, CancellationToken cancellationToken) 
				: base(compilation, cancellationToken)
			{
			}

			public override void VisitThrowStatement(ThrowStatementSyntax node)
			{
				base.VisitThrowStatement(node);
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
			
			walker.Locations.Should().BeEquivalentTo((line: 13, column: 4));
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

			walker.Locations.Should().BeEquivalentTo((line: 13, column: 4));
		}

		[Theory]
		[EmbeddedFileData("PropertyGetter.cs")]
		public async Task PropertyGetter(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((line: 14, column: 16));
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

			walker.Locations.Should().BeEquivalentTo((line: 14, column: 16));
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

			walker.Locations.Should().BeEquivalentTo((line: 14, column: 4));
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

			walker.Locations.Should().BeEquivalentTo((line: 13, column: 26));
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

			walker.Locations.Should().BeEquivalentTo((line: 13, column: 14));
		}

		[Theory]
		[EmbeddedFileData("LocalLambda.cs")]
		public async Task LocalLambda(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((line: 13, column: 20));
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

			walker.Locations.Should().BeEquivalentTo((line: 14, column: 4));
		}

		[Theory(Skip = "Expression-bodied methods are not supported by Roslyn v1")]
		[EmbeddedFileData("InstanceExpressionBodiedMethod.cs")]
		public async Task InstanceExpressionBodiedMethod(string text)
		{
			Document document = CreateDocument(text);
			Compilation compilation = await document.Project.GetCompilationAsync();
			var walker = new ExceptionWalker(compilation, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((line: 14, column: 4));
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

			walker.Locations.Should().BeEquivalentTo((line: 14, column: 4));
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

			walker.Locations.Should().BeEquivalentTo((line: 14, column: 4));
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

			walker.Locations.Should().BeEquivalentTo((line: 14, column: 4));
		}
	}
}
