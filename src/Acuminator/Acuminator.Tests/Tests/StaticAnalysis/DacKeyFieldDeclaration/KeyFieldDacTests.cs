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
		public async Task TestCompoundKey_ShouldNotShowDiagnosticAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("Key.cs")]
		public async Task TestKey_ShouldNotShowDiagnosticAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityKey.cs")]
		public async Task TestIdentityKey_ShouldNotShowDiagnosticAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs")]
		public async Task TestIdentityKeyWithKeyAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.CreateFor(
					location: (Line: 10, Column: 4),
					extraLocations: new (int Line, int Column)[] 
					{
						(Line: 16, Column: 4)
					}),
				Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.CreateFor(
					location: (Line: 16, Column: 4),
					extraLocations: new (int Line, int Column)[] 
					{
						(Line: 10, Column: 4)
					}));

		
		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey.cs")]
		public async Task TestIdentityKeyWithCompoundKeyAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.CreateFor(
					location: (Line: 9, Column: 4),
					extraLocations: new (int Line, int Column)[] 
					{
						(Line: 17, Column: 4),
						(Line: 27, Column: 4)
					}),
				Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.CreateFor(
					location: (Line: 17, Column: 4),
					extraLocations: new (int Line, int Column)[] 
					{
						(Line: 9,  Column: 4),
						(Line: 27, Column: 4)
					}),
				Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.CreateFor(
					location: (Line: 27, Column: 4),
					extraLocations: new (int Line, int Column)[] 
					{
						(Line: 9,  Column: 4),
						(Line: 17, Column: 4)
					}));

		[Theory]
		[EmbeddedFileData("IdentityNoKey.cs")]
		public async Task TestIdentityNoKey_ShouldNotShowDiagnosticAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityNoKey_CompoundKey.cs")]
		public async Task TestIdentityNoKeyWithCompoundKey_ShouldNotShowDiagnosticAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityNoKey_Key.cs")]
		public async Task TestIdentityNoKeyWithKey_ShouldNotShowDiagnosticAsync(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs",
						"IdentityKey_Key_EditFieldsAttr_Expected.cs")]
		public async Task TestFixForIdentityKeyWithKeyAsync(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey.cs",
							"IdentityKey_CompoundKey_EditFieldsAttr_Expected.cs")]
		public async Task TestFixForIdentityKeyWithCompoundKey_EditFieldsAttrAsync(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs",
						"IdentityKey_Key_EditIdentityAttr_Expected.cs")]
		public async Task TestFixForIdentityKeyWithKey_EditIdentityAttributeAsync(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected,1);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey.cs",
							"IdentityKey_CompoundKey_EditIdentityAttr_Expected.cs")]
		public async Task TestFixForIdentityKeyWithCompoundKey_EditIdentityAttributeAsync(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected,1);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs",
						"IdentityKey_Key_RemoveIdentityAttr_Expected.cs")]
		public async Task TestFixForIdentityKeyWithKey_RemoveIdentityAttributeAsync(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, 2);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey.cs",
							"IdentityKey_CompoundKey_RemoveIdentityAttr_Expected.cs")]
		public async Task TestFixForIdentityKeyWithCompoundKey_RemoveIdentityAttributeAsync(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, 2);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey_AttributeListSyntax.cs",
							"IdentityKey_CompoundKey_AttributeListSyntax_Expected.cs")]
		public async Task TestFixForIdentityKeyWithCompoundKey_AttributeListSyntaxAsync(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, 2);
	}
}
