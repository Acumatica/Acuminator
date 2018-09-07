using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.Refactorings.EventHandlerSignature;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Xunit;

namespace Acuminator.Tests.Tests.Refactorings.EventHandlerSignature
{
	public class EventHandlerSignatureTests : CodeRefactoringVerifier
	{
		protected override CodeRefactoringProvider GetCSharpCodeRefactoringProvider() => new EventHandlerSignatureRefactoring();

		[Theory]
		[EmbeddedFileData("CacheAttached.cs", "CacheAttached_Expected.cs")]
		public void CacheAttached(string actual, string expected)
		{
			VerifyCSharpRefactoring(actual, expected);
		}
	}
}
