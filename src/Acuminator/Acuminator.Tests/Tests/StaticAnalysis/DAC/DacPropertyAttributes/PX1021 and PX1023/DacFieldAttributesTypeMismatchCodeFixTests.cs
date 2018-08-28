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
	public class DacFieldAttributesTypeMismatchCodeFixTests : CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData(@"PX1021\Diagnostics\DacFieldAttributesTypeMismatch.cs",
						  @"PX1021\CodeFixes\DacFieldAttributesTypeMismatchExpected.cs", true)]
		public void Test_DAC_Property_Type_Not_Compatible_With_Field_Attribute_CodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacPropertyAttributesAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new IncompatibleDacPropertyAndFieldAttributeFix();
	}
}
