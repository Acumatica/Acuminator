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
	public class KeyFieldInDacTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new KeyFieldDeclarationAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new KeyFieldDeclarationFix();

		[Theory]
		[EmbeddedFileData("CompoundKey.cs")]
		public virtual void TestCompoundKey(string source) =>
			VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("Key.cs")]
		public virtual void TestKey(string source) =>
			VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("IdentityKey.cs")]
		public virtual void TestIdentityKey(string source) =>
			VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("IdentityKey+Key.cs")]
		public virtual void TestIdentityKey_Key(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(locations: new (int line, int column)[] { (line: 10, column: 4), (line: 10, column: 4), (line: 16, column: 4) }),
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(locations: new (int line, int column)[] { (line: 16, column: 4), (line: 10, column: 4), (line: 16, column: 4) }));

		
		[Theory]
		[EmbeddedFileData("IdentityKey+CompoundKey.cs")]
		public virtual void TestIdentityKey_CompoundKey(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(locations: new (int line, int column)[] { (line: 9, column: 4) ,
																										(line: 9, column: 4), (line: 17, column: 4), (line: 27, column: 4)
																								      }),
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(locations: new (int line, int column)[] { (line: 17, column: 4), (line: 9, column: 4), (line: 17, column: 4), (line: 27, column: 4) }),
				Descriptors.PX1055_DacKeyFieldBound.CreateFor(locations: new (int line, int column)[] { (line: 27, column: 4), (line: 9, column: 4), (line: 17, column: 4), (line: 27, column: 4) }));

		[Theory]
		[EmbeddedFileData("IdentityNoKey.cs")]
		public virtual void TestIdentityNoKey(string source) =>
			VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("IdentityNoKey+CompoundKey.cs")]
		public virtual void TestIdentityNoKey_CompoundKey(string source) =>
			VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("IdentityNoKey+Key.cs")]
		public virtual void TestIdentityNoKey_Key(string source) =>
			VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("IdentityKey+Key.cs",
						"IdentityKey+Key_EditFieldsAttr_Expected.cs")]
		public virtual void TestFixForIdentityKey_Key(string actual, string expected) =>
		  VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("IdentityKey+CompoundKey.cs",
							"IdentityKey+CompoundKey_EditFiedlsAttr_Expected.cs")]
		public virtual void TestFixForIdentityKey_CompoundKey(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("IdentityKey+Key.cs",
						"IdentityKey+Key_EditIdentityAttr_Expected.cs")]
		public virtual void TestFixForIdentityKey_Key_EditIdentityAttribute(string actual, string expected) =>
		  VerifyCSharpFix(actual, expected,1);

		[Theory]
		[EmbeddedFileData("IdentityKey+CompoundKey.cs",
							"IdentityKey+CompoundKey_EditIdentityAttr_Expected.cs")]
		public virtual void TestFixForIdentityKey_CompoundKey_EditIdentityAttribute(string actual, string expected) =>
			VerifyCSharpFix(actual, expected,1);

		[Theory]
		[EmbeddedFileData("IdentityKey+Key.cs",
						"IdentityKey+Key_RemoveIdentityAttr_Expected.cs")]
		public virtual void TestFixForIdentityKey_Key_RemoveIdentityAttribute(string actual, string expected) =>
		  VerifyCSharpFix(actual, expected, 2);

		[Theory]
		[EmbeddedFileData("IdentityKey+CompoundKey.cs",
							"IdentityKey+CompoundKey_RemoveIdentityAttr_Expected.cs")]
		public virtual void TestFixForIdentityKey_CompoundKey_RemoveIdentityAttribute(string actual, string expected) =>
			VerifyCSharpFix(actual, expected, 2);

		[Theory]
		[EmbeddedFileData("IdentityKey+CompoundKey_AttributeListSyntax.cs",
							"IdentityKey+CompoundKey_AttributeListSyntax_Expected.cs")]
		public virtual void TestFixForIdentityKey_CompoundKey_AttributeListSyntax(string actual, string expected) =>
			VerifyCSharpFix(actual, expected, 2);
	}
}
