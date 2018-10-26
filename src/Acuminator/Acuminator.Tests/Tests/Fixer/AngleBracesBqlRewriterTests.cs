using Acuminator.Tests.Verification;
using Acuminator.Vsix.BqlFixer;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Acuminator.Tests.Tests.Fixer
{
	public class AngleBracesBqlRewriterTests
	{
		private readonly ITestOutputHelper _outputHelper;
		public AngleBracesBqlRewriterTests(ITestOutputHelper outputHelper)
		{
			_outputHelper = outputHelper;
		}

		[Theory]
		[ClassData(typeof(TestData))]
		public void VisitIncompleteMember_FixesText(string text, string expected)
		{
			_outputHelper.WriteLine($"Original text:\r\n{text}\r\nExpected text:\r\n{expected}");
			var (sut, root) = CreateFromText(text);
			
			var incomleted = (root as CompilationUnitSyntax)
				?.Members
				.FirstOrDefault(m => m is IncompleteMemberSyntax) as IncompleteMemberSyntax;
			incomleted.Should().NotBeNull("invalid input data. It must contain only single incomlete member");

			var result = sut.VisitIncompleteMember(incomleted);

			result.ToFullString().Should().BeEquivalentTo(expected);
		}

		protected (AngleBracesBqlRewriter sut, SyntaxNode root) CreateFromText(string text)
		{
			Compilation compilation = GetCompilation(text);
			SyntaxTree syntaxTree = compilation.SyntaxTrees.First();
			SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
			// this is repeat, because each second test contains invalid root
			return (new AngleBracesBqlRewriter(semanticModel), syntaxTree.GetRoot());
		}

		protected Compilation GetCompilation(string text)
		{
			SyntaxTree rewriterTree = CSharpSyntaxTree.ParseText(text);

			return CSharpCompilation.Create(Guid.NewGuid().ToString(), new SyntaxTree[] { rewriterTree });
		}


		private class TestData : IEnumerable<object[]>
		{
			// currently doesn't support indention
			public IEnumerator<object[]> GetEnumerator()
			{
				// todo: add checking with trailing semicolons
				yield return new object[]
				{
					"public PXSelect<APInvoice,Where<APInvoice.refNbr, Equal<Current<APInvoice.refNbr>>, "
						+ "And<APInvoice.docType, Equal<Current<APInvoice.docType> CurrentOrder",
					"public PXSelect<APInvoice,Where<APInvoice.refNbr, Equal<Current<APInvoice.refNbr>>, "
						+ "And<APInvoice.docType, Equal<Current<APInvoice.docType>>>>> CurrentOrder",
				};
				yield return new object[]
				{
					"public PXSelect<APInvoice,Where<APInvoice.refNbr, Or<Nor> field",
					"public PXSelect<APInvoice,Where<APInvoice.refNbr, Or<Nor>>> field"
				};
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
