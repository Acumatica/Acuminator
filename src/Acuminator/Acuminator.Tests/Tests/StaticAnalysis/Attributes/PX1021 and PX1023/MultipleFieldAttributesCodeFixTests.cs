using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes;
using Acuminator.Tests.Helpers;
using CodeFixVerifier = Acuminator.Tests.Verification.CodeFixVerifier;

namespace Acuminator.Tests
{
	public class MultipleFieldAttributesCodeFixTests : Verification.CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData(@"Attributes\PX1023\Diagnostics\DacWithMultipleFieldAttributes.cs",
						  @"Attributes\PX1023\CodeFixes\DacWithMultipleFieldAttributes_Expected.cs")]
		public void Test_Multiple_Field_Attributes_On_DAC_Property_CodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacPropertyAttributesAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new MultipleDacFieldAttributesFix();
	}
}
