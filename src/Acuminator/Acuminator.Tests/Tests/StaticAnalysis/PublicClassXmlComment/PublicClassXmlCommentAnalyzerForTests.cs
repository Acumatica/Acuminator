using Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis;

namespace Acuminator.Tests.Tests.StaticAnalysis.PublicClassXmlComment
{
	/// <summary>
	/// An XML comment analyzer for tests - overrides <see cref="PublicClassXmlCommentAnalyzer.IsUnitTestAssembly(Compilation)"/> method to work in unit tests.
	/// </summary>
	internal class PublicClassXmlCommentAnalyzerForTests : PublicClassXmlCommentAnalyzer
	{
		public PublicClassXmlCommentAnalyzerForTests(CodeAnalysisSettings codeAnalysisSettings) :
												base(codeAnalysisSettings)
		{
		}

		public PublicClassXmlCommentAnalyzerForTests()									
		{
		}

		protected override bool IsUnitTestAssembly(Compilation compilation) => false;
	}
}
