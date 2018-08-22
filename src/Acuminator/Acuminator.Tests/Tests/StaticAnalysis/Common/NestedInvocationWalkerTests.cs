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
		class ExceptionWalker : NestedInvocationWalker
		{
			private readonly List<Location> _locations = new List<Location>();
			public IReadOnlyList<Location> Locations => _locations;

			public ExceptionWalker(SemanticModel semanticModel, CancellationToken cancellationToken) 
				: base(semanticModel, cancellationToken)
			{
			}

			public override void VisitThrowStatement(ThrowStatementSyntax node)
			{
				_locations.Add(node.GetLocation());
			}
		}

		[Theory]
		[EmbeddedFileData(@"Common\NestedInvocationWalker\SanityCheck.cs")]
		public async Task SanityCheck(string text)
		{
			Document document = CreateDocument(text);
			SemanticModel semanticModel = await document.GetSemanticModelAsync();
			var root = (CSharpSyntaxNode) await document.GetSyntaxRootAsync();
			var walker = new ExceptionWalker(semanticModel, CancellationToken.None);

			root.Accept(walker);
			
			walker.Locations.Should().BeEquivalentTo((line: 13, column: 4));
		}
	}
}
