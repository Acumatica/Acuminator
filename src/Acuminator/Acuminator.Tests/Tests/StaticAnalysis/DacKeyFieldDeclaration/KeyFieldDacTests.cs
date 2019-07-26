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
		public async Task CompoundKey_ShouldNotShowDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("Key.cs")]
		public async Task Key_ShouldNotShowDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("BaseAndDerivedDac.cs")]
		public async Task DerivedDac_InconsistentKey_WithBaseDac(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.CreateFor(line: 28, column: 4));

		[Theory]
		[EmbeddedFileData("IdentityKey.cs")]
		public async Task IdentityKey_ShouldNotShowDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs")]
		public async Task IdentityKeyWithKey(string source) =>
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
		public async Task IdentityKeyWithCompoundKey(string source) =>
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
		public async Task IdentityNoKey_ShouldNotShowDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityNoKey_CompoundKey.cs")]
		public async Task IdentityNoKeyWithCompoundKey_ShouldNotShowDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityNoKey_Key.cs")]
		public async Task IdentityNoKeyWithKey_ShouldNotShowDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs",
						"IdentityKey_Key_EditFieldsAttr_Expected.cs")]
		public async Task FixForIdentityKeyWithKey(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey.cs",
							"IdentityKey_CompoundKey_EditFieldsAttr_Expected.cs")]
		public async Task FixForIdentityKeyWithCompoundKey_EditFieldsAttr(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs",
						"IdentityKey_Key_EditIdentityAttr_Expected.cs")]
		public async Task FixForIdentityKeyWithKey_EditIdentityAttribut(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected,1);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey.cs",
							"IdentityKey_CompoundKey_EditIdentityAttr_Expected.cs")]
		public async Task FixForIdentityKeyWithCompoundKey_EditIdentityAttribute(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected,1);

		[Theory]
		[EmbeddedFileData("IdentityKey_Key.cs",
						"IdentityKey_Key_RemoveIdentityAttr_Expected.cs")]
		public async Task FixForIdentityKeyWithKey_RemoveIdentityAttribute(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, 2);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey.cs",
							"IdentityKey_CompoundKey_RemoveIdentityAttr_Expected.cs")]
		public async Task FixForIdentityKeyWithCompoundKey_RemoveIdentityAttribute(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, 2);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey_AttributeListSyntax.cs",
							"IdentityKey_CompoundKey_AttributeListSyntax_Expected.cs")]
		public async Task FixForIdentityKeyWithCompoundKey_AttributeListSyntax(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, 2);

		[Theory]
		[EmbeddedFileData("IdentityKey_CompoundKey_AttributeListSyntax.cs",
							"IdentityKey_CompoundKey_AttributeListSyntax_EditFields_Expected.cs")]
		public async Task FixForIdentityKeyWithCompoundKey_AttributeListSyntax_EditFields(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
