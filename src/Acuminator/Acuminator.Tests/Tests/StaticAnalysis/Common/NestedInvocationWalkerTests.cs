using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verifiers;
using Acuminator.Utils.RoslynExtensions;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
	public class NestedInvocationWalkerTests : DiagnosticVerifier
	{
		private class ExceptionWalker : NestedInvocationWalker
		{
			private readonly List<Location> _locations = new List<Location>();
			public IReadOnlyList<Location> Locations => _locations;

			public ExceptionWalker(SemanticModel semanticModel, CancellationToken cancellationToken) 
				: base(semanticModel, cancellationToken)
			{
			}

			public override void VisitThrowStatement(ThrowStatementSyntax node)
			{
				base.VisitThrowStatement(node);
				_locations.Add((OriginalNode ?? node).GetLocation());
			}
		}

		[Theory]
		[EmbeddedFileData(@"Common\NestedInvocationWalker\SanityCheck.cs")]
		public async Task SanityCheck(string text)
		{
			Document document = CreateDocument(text);
			SemanticModel semanticModel = await document.GetSemanticModelAsync();
			var walker = new ExceptionWalker(semanticModel, CancellationToken.None);
			var node = (CSharpSyntaxNode) await document.GetSyntaxRootAsync();

			node.Accept(walker);
			
			walker.Locations.Should().BeEquivalentTo((line: 13, column: 4));
		}

		[Theory]
		[EmbeddedFileData(@"Common\NestedInvocationWalker\StaticMethod.cs")]
		public async Task StaticMethod(string text)
		{
			Document document = CreateDocument(text);
			SemanticModel semanticModel = await document.GetSemanticModelAsync();
			var walker = new ExceptionWalker(semanticModel, CancellationToken.None);
			var node = (CSharpSyntaxNode) (await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((line: 13, column: 4));
		}

		[Theory]
		[EmbeddedFileData(@"Common\NestedInvocationWalker\PropertyGetter.cs")]
		public async Task PropertyGetter(string text)
		{
			Document document = CreateDocument(text);
			SemanticModel semanticModel = await document.GetSemanticModelAsync();
			var walker = new ExceptionWalker(semanticModel, CancellationToken.None);
			var node = (CSharpSyntaxNode)(await document.GetSyntaxRootAsync()).DescendantNodes()
				.OfType<ClassDeclarationSyntax>().First();

			node.Accept(walker);

			walker.Locations.Should().BeEquivalentTo((line: 14, column: 16));
		}
	}
}
