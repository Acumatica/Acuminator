using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacNonAbstractFieldType;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacNonAbstractFieldType
{
	public class DacFieldNotAbstractTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacNonAbstractFieldTypeAnalyzer();

		[Theory]
		[EmbeddedFileData("SOOrderNotAbstractField.cs")]
		public virtual void Test_Dac_With_Not_Abstract_Fields(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1024_DacNonAbstractFieldType.CreateFor(line: 22, column: 16),
				Descriptors.PX1024_DacNonAbstractFieldType.CreateFor(line: 34, column: 16),
				Descriptors.PX1024_DacNonAbstractFieldType.CreateFor(line: 45, column: 16));	
	}
}
