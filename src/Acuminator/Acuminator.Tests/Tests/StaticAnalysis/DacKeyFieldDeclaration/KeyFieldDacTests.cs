using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacExtensionDefaultAttribute;
using Acuminator.Analyzers.StaticAnalysis.DacKeyFieldDeclaration;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacKeyFieldDeclaration
{
	public class KeyFieldInDacTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new KeyFieldDeclarationAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new KeyFieldDeclarationFix();

		[Theory]
		[EmbeddedFileData("CompoundKey.cs")]
		public async Task TestCompoundKey_ShouldNotShowDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("Key.cs")]
		public async Task TestKey_ShouldNotShowDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityKey.cs")]
		public async Task TestIdentityKey_ShouldNotShowDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs")]
		public async Task TestIdentityKeyWithKey(string source) =>
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
		public async Task TestIdentityKeyWithCompoundKey(string source) =>
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
		public async Task TestIdentityNoKey_ShouldNotShowDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityNoKey_CompoundKey.cs")]
		public async Task TestIdentityNoKeyWithCompoundKey_ShouldNotShowDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityNoKey_Key.cs")]
		public async Task TestIdentityNoKeyWithKey_ShouldNotShowDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs",
						"IdentityKey_Key_EditFieldsAttr_Expected.cs")]
		public async Task TestFixForIdentityKeyWithKey(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey.cs",
							"IdentityKey_CompoundKey_EditFieldsAttr_Expected.cs")]
		public async Task TestFixForIdentityKeyWithCompoundKey_EditFieldsAttr(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs",
						"IdentityKey_Key_EditIdentityAttr_Expected.cs")]
		public async Task TestFixForIdentityKeyWithKey_EditIdentityAttribut(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected,1);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey.cs",
							"IdentityKey_CompoundKey_EditIdentityAttr_Expected.cs")]
		public async Task TestFixForIdentityKeyWithCompoundKey_EditIdentityAttribute(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected,1);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs",
						"IdentityKey_Key_RemoveIdentityAttr_Expected.cs")]
		public async Task TestFixForIdentityKeyWithKey_RemoveIdentityAttribute(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, 2);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey.cs",
							"IdentityKey_CompoundKey_RemoveIdentityAttr_Expected.cs")]
		public async Task TestFixForIdentityKeyWithCompoundKey_RemoveIdentityAttribute(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, 2);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey_AttributeListSyntax.cs",
							"IdentityKey_CompoundKey_AttributeListSyntax_Expected.cs")]
		public async Task TestFixForIdentityKeyWithCompoundKey_AttributeListSyntax(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, 2);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey_AttributeListSyntax.cs",
							"IdentityKey_CompoundKey_AttributeListSyntax_EditFields_Expected.cs")]
		public async Task TestFixForIdentityKeyWithCompoundKey_AttributeListSyntax_EditFields(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
