using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;
using Acuminator.Analyzers;
using Acuminator.Analyzers.FixProviders;
using Acuminator.Tests.Helpers;

namespace Acuminator.Tests
{
	public class MultipleFieldAttributesCodeFixTests : CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData(@"Attributes\CodeFixes\DacExampleWithMultipleFieldAttributes.cs",
						  @"Attributes\CodeFixes\DacExampleWithMultipleFieldAttributes_Expected.cs")]
		public void TestCodeFix_Field(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacPropertyAttributesAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new MultipleDacFieldAttributesFix();
	}
}
