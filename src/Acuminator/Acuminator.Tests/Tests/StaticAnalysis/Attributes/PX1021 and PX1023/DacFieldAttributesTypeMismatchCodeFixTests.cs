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
	public class DacFieldAttributesTypeMismatchCodeFixTests : Verification.CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData(@"Attributes\PX1021\Diagnostics\DacFieldAttributesTypeMismatch.cs",
						  @"Attributes\PX1021\CodeFixes\DacFieldAttributesTypeMismatchExpected.cs")]
		public void Test_DAC_Property_Type_Not_Compatible_With_Field_Attribute_CodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacPropertyAttributesAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new IncompatibleDacPropertyAndFieldAttributeFix();
	}
}
