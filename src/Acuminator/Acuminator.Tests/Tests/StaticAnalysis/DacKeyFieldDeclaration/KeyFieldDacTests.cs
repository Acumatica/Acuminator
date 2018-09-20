using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacExtensionDefaultAttribute;
using Acuminator.Analyzers.StaticAnalysis.DacKeyFieldDeclaration;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacKeyFieldDeclaration
{
	public class KeyFieldInDacTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new KeyFieldDeclarationAnalyzer();

//		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DacKeyFieldDeclarationFix();

		[Theory]
		[EmbeddedFileData("DacExtensionWithBoundFields.cs")]
		public virtual void TestDacKeyFieldWithBoundAttribute(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(line: 23, column: 4),
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(line: 30, column: 4),
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(line: 44, column: 4),
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(line: 50, column: 13),
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(line: 56, column: 13),
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(line: 62, column: 4));

	}
}
