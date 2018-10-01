using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacExtensionDefaultAttribute;
using Acuminator.Analyzers.StaticAnalysis.DacKeyFieldDeclaration;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacKeyFieldDeclaration
{
	public class KeyFieldInDacTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new KeyFieldDeclarationAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new KeyFieldDeclarationFix();

		[Theory]
		[EmbeddedFileData("CompoundKey.cs")]
		public async Task TestCompoundKeyAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("Key.cs")]
		public async Task TestKeyAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityKey.cs")]
		public async Task TestIdentityKeyAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs")]
		public async Task TestIdentityKey_KeyAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.CreateFor(
					locations: new (int Line, int Column)[] 
					{
						(Line: 10, Column: 4),
						(Line: 10, Column: 4),
						(Line: 16, Column: 4)
					}),
				Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.CreateFor(
					locations: new (int Line, int Column)[] 
					{
						(Line: 16, Column: 4),
						(Line: 10, Column: 4),
						(Line: 16, Column: 4)
					}));

		
		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey.cs")]
		public async Task TestIdentityKey_CompoundKeyAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.CreateFor(
					locations: new (int Line, int Column)[] 
					{
						(Line: 9, Column: 4),
						(Line: 9, Column: 4),
						(Line: 17, Column: 4),
						(Line: 27, Column: 4)
					}),
				Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.CreateFor(
					locations: new (int Line, int Column)[] 
					{
						(Line: 17, Column: 4),
						(Line: 9,  Column: 4),
						(Line: 17, Column: 4),
						(Line: 27, Column: 4)
					}),
				Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.CreateFor(
					locations: new (int Line, int Column)[] 
					{
						(Line: 27, Column: 4),
						(Line: 9,  Column: 4),
						(Line: 17, Column: 4),
						(Line: 27, Column: 4)
					}));

		[Theory]
		[EmbeddedFileData("IdentityNoKey.cs")]
		public async Task TestIdentityNoKeyAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityNoKey_CompoundKey.cs")]
		public async Task TestIdentityNoKey_CompoundKeyAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityNoKey_Key.cs")]
		public async Task TestIdentityNoKey_KeyAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs",
						"IdentityKey_Key_EditFieldsAttr_Expected.cs")]
		public async Task TestFixForIdentityKey_KeyAsync(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey.cs",
							"IdentityKey_CompoundKey_EditFieldsAttr_Expected.cs")]
		public async Task TestFixForIdentityKey_CompoundKeyAsync(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs",
						"IdentityKey_Key_EditIdentityAttr_Expected.cs")]
		public async Task TestFixForIdentityKey_Key_EditIdentityAttributeAsync(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected,1);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey.cs",
							"IdentityKey_CompoundKey_EditIdentityAttr_Expected.cs")]
		public async Task TestFixForIdentityKey_CompoundKey_EditIdentityAttributeAsync(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected,1);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs",
						"IdentityKey_Key_RemoveIdentityAttr_Expected.cs")]
		public async Task TestFixForIdentityKey_Key_RemoveIdentityAttributeAsync(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, 2);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey.cs",
							"IdentityKey_CompoundKey_RemoveIdentityAttr_Expected.cs")]
		public async Task TestFixForIdentityKey_CompoundKey_RemoveIdentityAttributeAsync(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, 2);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey_AttributeListSyntax.cs",
							"IdentityKey_CompoundKey_AttributeListSyntax_Expected.cs")]
		public async Task TestFixForIdentityKey_CompoundKey_AttributeListSyntaxAsync(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, 2);
	}
}
