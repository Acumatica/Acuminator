﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Analyzers.StaticAnalysis.UnderscoresInDac;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.UnderscoresInDac
{
	public class UnderscoresInDacTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new UnderscoresInDacAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new UnderscoresInDacFix();

		[Theory]
		[EmbeddedFileData("DacWithUnderscores.cs")]
		public virtual void DacWithUnderscoresInDeclaration(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1026_UnderscoresInDacDeclaration.CreateFor(line: 10, column: 15),
				Descriptors.PX1026_UnderscoresInDacDeclaration.CreateFor(line: 13, column: 25),
				Descriptors.PX1026_UnderscoresInDacDeclaration.CreateFor(line: 18, column: 17),
				Descriptors.PX1026_UnderscoresInDacDeclaration.CreateFor(line: 44, column: 25));

		[Theory]
		[EmbeddedFileData("DacExtensionWithUnderscores.cs")]
		public virtual void DacExtensionWithUnderscoresInDeclaration(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1026_UnderscoresInDacDeclaration.CreateFor(line: 13, column: 25),
				Descriptors.PX1026_UnderscoresInDacDeclaration.CreateFor(line: 17, column: 18),
				Descriptors.PX1026_UnderscoresInDacDeclaration.CreateFor(line: 21, column: 25),
				Descriptors.PX1026_UnderscoresInDacDeclaration.CreateFor(line: 24, column: 18));

		[Theory]
		[EmbeddedFileData("DacWithUnderscores.cs",
						  "DacWithUnderscores_Expected.cs")]
		public virtual void ForDacWithUnderscoresInDeclaration_CodeFix(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacExtensionWithUnderscores.cs",
						  "DacExtensionWithUnderscores_Expected.cs")]
		public virtual void ForDacExtensionWithUnderscoresInDeclaration_CodeFix(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);
	}
}
