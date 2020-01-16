using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Acuminator.Utilities.DiagnosticSuppression;

namespace Acuminator.Tests.Tests.DiagnosticSuppression.SuppressionFileIO
{
	/// <summary>
	/// A suppression file ordering tests.
	/// </summary>
	public class SuppressionFileIOTests
	{
		//[Theory]
		//[InlineData(@"PX.Objects.acuminator")]
		//public void CheckLoadOfSuppressionFile(string fileName)
		//{
		//	var suppressionFileService = new SuppressionFileTestService();

		//	string examplesDirectory = GetExamplesDirectory();
		//	string suppressionFileName = Path.Combine(examplesDirectory, fileName);

		//	File.Exists(suppressionFileName).Should().BeTrue();
		//	string oldContent = File.ReadAllText(suppressionFileName);

		//	suppressionFileService

		//	var xmlDocument = XDocument.Load(suppressionFileName);
		//	xmlDocument.

		//	var merger = new SuppressionFilesMerger();

		//	//Merge file with itself and rewrite its content. This makes sorting of its content.
		//	merger.Merge(suppressionFileName, suppressionFileName, suppressionFileName);

		//	File.Exists(suppressionFileName).Should().BeTrue();

		//	string newContent = File.ReadAllText(suppressionFileName);
		//	newContent.Should().Equals(oldContent);
		//}

		//[Theory]
		//[InlineData(@"PX.Objects.acuminator")]
		//public void CheckThatOrderDidNotChange(string fileName)
		//{
		//	var suppressionFileService = new SuppressionFileTestService();

		//	string examplesDirectory = GetExamplesDirectory();
		//	string suppressionFileName = Path.Combine(examplesDirectory, fileName);

		//	File.Exists(suppressionFileName).Should().BeTrue();
		//	string oldContent = File.ReadAllText(suppressionFileName);

		//						suppressionFileService

		//	var xmlDocument = XDocument.Load(suppressionFileName);
		//	xmlDocument.

		//	var merger = new SuppressionFilesMerger();

		//	//Merge file with itself and rewrite its content. This makes sorting of its content.
		//	merger.Merge(suppressionFileName, suppressionFileName, suppressionFileName);

		//	File.Exists(suppressionFileName).Should().BeTrue();

		//	string newContent = File.ReadAllText(suppressionFileName);
		//	newContent.Should().Equals(oldContent);
		//}

		private string GetExamplesDirectory()
		{
			DirectoryInfo debugOrReleaseDir = new DirectoryInfo(Environment.CurrentDirectory);
			string solutionDir = debugOrReleaseDir.Parent.Parent.FullName;
			return Path.Combine(solutionDir, "Sort Tests", "Examples");
		}
	}
}
