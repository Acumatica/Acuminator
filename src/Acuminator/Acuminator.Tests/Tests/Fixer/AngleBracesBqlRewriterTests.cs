#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Tests.Verification;
using Acuminator.Vsix.BqlFixer;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

		[Theory(Skip = "BQL Reqwriter is not in production. After Roslyn upgrade to version 3.11.0 the pasing changes and the primitive tests must be reworked.")]
		[ClassData(typeof(TestData))]
		public void VisitIncompleteMember_FixesText(string text, string expected)
		{
			_outputHelper.WriteLine($"Original text:\r\n{text}\r\nExpected text:\r\n{expected}");
			var (sut, root) = CreateFromText(text);

			var nodes = root.DescendantNodes().ToList();
			var incompletedNodes = root.DescendantNodesAndSelf()
									   .OfType<IncompleteMemberSyntax>()
									   .ToList();

			incompletedNodes.Should().HaveCount(1, "Invalid input data. It must contain only single incomlete member");

			IncompleteMemberSyntax incompleteMemberNode = incompletedNodes[0];
			var fixedMemberNode = sut.VisitIncompleteMember(incompleteMemberNode);
			var modifiedRoot = root.ReplaceNode(incompleteMemberNode, fixedMemberNode);

			var modifiedRootText = modifiedRoot.ToFullString();

			modifiedRootText.Should().BeEquivalentTo(expected);
		}

		protected (AngleBracesBqlRewriter SUT, CompilationUnitSyntax Root) CreateFromText(string text)
		{
			Compilation compilation = GetCompilation(text);
			SyntaxTree? syntaxTree = compilation.SyntaxTrees.FirstOrDefault();
			syntaxTree.Should().NotBeNull();
			
			SemanticModel? semanticModel = compilation.GetSemanticModel(syntaxTree);
			semanticModel.Should().NotBeNull();
			
			var rewriter = new AngleBracesBqlRewriter(semanticModel);
			var root = syntaxTree.GetRoot(default) as CompilationUnitSyntax;

			root.Should().NotBeNull();

			// this is repeat, because each second test contains invalid root
			return (rewriter, root!);
		}

		protected Compilation GetCompilation(string text)
		{
			SyntaxTree rewriterTree = CSharpSyntaxTree.ParseText(text);
			rewriterTree.Should().NotBeNull();

			return CSharpCompilation.Create(Guid.NewGuid().ToString(), [rewriterTree]);
		}


		private class TestData : IEnumerable<object[]>
		{
			// currently doesn't support indention
			public IEnumerator<object[]> GetEnumerator()
			{
				// todo: add checking with trailing semicolons (but it adds them)
				// hack: some problems with trivias, so check without spaces or newlines
				yield return new object[]
				{
					"public PXSelect<APInvoice,Where<APInvoice.refNbr,Equal<Current<APInvoice.refNbr>>,"
						+ "And<APInvoice.docType,Equal<Current<APInvoice.docType> CurrentOrder",
					"public PXSelect<APInvoice,Where<APInvoice.refNbr,Equal<Current<APInvoice.refNbr>>,"
						+ "And<APInvoice.docType,Equal<Current<APInvoice.docType>>>>>CurrentOrder;",
				};
				yield return new object[]
				{
					"public PXSelect<APInvoice,Where<APInvoice.refNbr,Or<Nor> field",
					"public PXSelect<APInvoice,Where<APInvoice.refNbr,Or<Nor>>>field;",
				};
				yield return new object[]
				{
					"PXSelect<SOOrder,Where<SOOrder.orderType,Equal<SalesOrder>,And<SOOrder.status,Equal<Open>>>,OrderBy<Asc<SOOrder.orderNbr>>otherField",
					"PXSelect<SOOrder,Where<SOOrder.orderType,Equal<SalesOrder>,And<SOOrder.status,Equal<Open>>>,OrderBy<Asc<SOOrder.orderNbr>>>otherField;",
				};
				yield return new object[]
				{
					"PXSelect<SOOrder,Dump<DumpSimple,Dump<DumpSimple,Dump<DumpSimple>>field",
					"PXSelect<SOOrder,Dump<DumpSimple,Dump<DumpSimple,Dump<DumpSimple>>>>field;",
				};
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
