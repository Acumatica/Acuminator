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
		[EmbeddedFileData("DacKeyFieldsUnbound.cs")]
		public virtual void TestDacKeyFieldWithBoundAttribute(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(line: 11, column: 4),
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(line: 19, column: 4));

	}
}
