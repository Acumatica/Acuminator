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
		[EmbeddedFileData("DacKeyFields_CompoundKey.cs")]
		public virtual void TestDacKeyFields_CompoundKey(string source) =>
			VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("DacKeyFields_Key.cs")]
		public virtual void TestDacKeyFields_Key(string source) =>
			VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("DacKeyFields_IdentityKey.cs")]
		public virtual void TestDacKeyFields_IdentityKey(string source) =>
			VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("DacKeyFields_IdentityKey+Key.cs")]
		public virtual void TestDacKeyFields_IdentityKey_Key(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(line: 10, column: 4),
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(line: 16, column: 4));

		
		[Theory]
		[EmbeddedFileData("DacKeyFields_IdentityKey+CompoundKey.cs")]
		public virtual void TestDacKeyFields_IdentityKey_CompoundKey(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(line: 9, column: 4),
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(line: 17, column: 4),
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(line: 27, column: 4));

		[Theory]
		[EmbeddedFileData("DacKeyFields_IdentityNoKey.cs")]
		public virtual void TestDacKeyFields_IdentityNoKey(string source) =>
			VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("DacKeyFields_IdentityNoKey+CompoundKey.cs")]
		public virtual void TestDacKeyFields_IdentityNoKey_CompoundKey(string source) =>
			VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("DacKeyFields_IdentityNoKey+Key.cs")]
		public virtual void TestDacKeyFields_IdentityNoKey_Key(string source) =>
			VerifyCSharpDiagnostic(source);

	}
}
