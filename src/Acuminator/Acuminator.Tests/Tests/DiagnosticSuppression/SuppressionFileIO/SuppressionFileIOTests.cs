using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
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
		private const string RelativeTestPath = @"Tests\DiagnosticSuppression\SuppressionFileIO\Examples";
		private SuppressionFileTestService _fileService = new SuppressionFileTestService();

		[Fact]
		public void CheckSuppressionMessageConversion_FromXml()
		{
			var xElement = GetXElementsToCheck().Take(1).Single();
			var target = xElement.Element("target").Value;
			var syntaxNode = xElement.Element("syntaxNode").Value;

			var messageToCheck = SuppressMessage.MessageFromElement(xElement);

			messageToCheck.Should().NotBeNull();
			messageToCheck.Value.Id.Should().Be(xElement.Attribute("id").Value);
			messageToCheck.Value.Target.Should().Be(target);
			messageToCheck.Value.SyntaxNode.Should().Be(syntaxNode);
		}

		[Fact]
		public void CheckSuppressionMessageConversion_ToXml()
		{
			var expectedXElement = GetXElementsToCheck().Take(1).Single();
			var message = new SuppressMessage(id: "PX1001",
											  target: @"PX.Objects.CS.Email.ExchangeBaseLogicSyncCommand<GraphType, TPrimary, ExchangeType>.Uploader",
											  syntaxNode: @"new UploadFileMaintenance()");

			var xElement = message.ToXml();

			xElement.Should().NotBeNull();
			xElement.Should().Be(expectedXElement);	
		}

		[Theory]
		[InlineData(@"PX.Objects.acuminator")]
		public void CheckLoadOfSuppressionFile(string fileName)
		{
			const int expectedCount = 3629;

			var messagesToCheck = GetSuppressionMessagesToCheck();
			string suppressionFilePath = GetFileFullPath(fileName);
			File.Exists(suppressionFilePath).Should().BeTrue();

			HashSet<SuppressMessage> messages = SuppressionFile.LoadMessages(_fileService, suppressionFilePath);

			messages.Should().NotBeNull();
			messages.Should().HaveCount(expectedCount);
			messages.Should().ContainInOrder(messagesToCheck);
		}

//		[Theory]
//		[InlineData(@"PX.Objects.acuminator")]
//		public void CheckThatOrderDidNotChange(string fileName)
//		{
//			string examplesDirectory = GetFileFullPath(fileName);
//			string suppressionFileName = Path.Combine(examplesDirectory, fileName);

//			File.Exists(suppressionFileName).Should().BeTrue();
//			string oldContent = File.ReadAllText(suppressionFileName);

//			suppressionFileService

//var xmlDocument = XDocument.Load(suppressionFileName);
//			xmlDocument.

//			var merger = new SuppressionFilesMerger();

//			//Merge file with itself and rewrite its content. This makes sorting of its content.
//			merger.Merge(suppressionFileName, suppressionFileName, suppressionFileName);

//			File.Exists(suppressionFileName).Should().BeTrue();

//			string newContent = File.ReadAllText(suppressionFileName);
//			newContent.Should().Equals(oldContent);
//		}

		private string GetFileFullPath(string shortFileName)
		{
			DirectoryInfo debugOrReleaseDir = new DirectoryInfo(Environment.CurrentDirectory);
			string solutionDir = debugOrReleaseDir.Parent.Parent.FullName;
			return Path.Combine(solutionDir, RelativeTestPath, shortFileName);
		}

		private IEnumerable<SuppressMessage> GetSuppressionMessagesToCheck() =>
			GetXElementsToCheck()
				.Select(element => SuppressMessage.MessageFromElement(element).Value);

		private IEnumerable<XElement> GetXElementsToCheck() => GetXmlStrings().Select(s => XElement.Parse(s));

		private IEnumerable<string> GetXmlStrings()
		{
			yield return 
				@"<suppressMessage id=""PX1001"">
					<target>PX.Objects.CS.Email.ExchangeBaseLogicSyncCommand&lt;GraphType, TPrimary, ExchangeType&gt;.Uploader</target>	
					<syntaxNode>new UploadFileMaintenance()</syntaxNode>
				</suppressMessage>";
			yield return 
				@"<suppressMessage id=""PX1003"">
					<target>PX.Objects.CA.Descriptor.ReportFunctions.CuryConvBase(object, object, object, object)</target>
					<syntaxNode>new PXGraph()</syntaxNode>
				 </suppressMessage>";
			yield return 
				@"<suppressMessage id=""PX1094"">
					<target>PX.Objects.EP.EPOtherAttendeeWithNotification</target>
					<syntaxNode>EPOtherAttendeeWithNotification</syntaxNode>
				 </suppressMessage>";
		}
	}
}
