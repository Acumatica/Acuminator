#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using Acuminator.Tests.Verification;
using Acuminator.Vsix;

using FluentAssertions;

using Xunit;

namespace Acuminator.Tests.Tests.Utilities.Version
{
	public class PackageVersionTests
	{
		[Fact]
		public void VersionIsParsable()
		{
			string packageVersion = AcuminatorVSPackage.PackageVersion;
			bool isParsable = System.Version.TryParse(packageVersion, out var version);
			
			isParsable.Should().BeTrue();
			version.ToString().Should().Be(packageVersion);
		}

		[Fact]
		public void Check_ManifestVersion_Equals_PackageVersion()
		{
			string thisTestFilePath = GetFilePath();
			DirectoryInfo thisTestFolder = new DirectoryInfo(Path.GetDirectoryName(thisTestFilePath));
			string acuminatorFolder = thisTestFolder.Parent.Parent.Parent.Parent.FullName;
			string manifestFilePath = Path.Combine(acuminatorFolder, "Acuminator.Vsix", "source.extension.vsixmanifest");
			
			var manifest = XDocument.Load(manifestFilePath);
			var identityNode = manifest?.Root?.DescendantNodes()
											  .OfType<XElement>()
											  .FirstOrDefault(node => node.Name.LocalName == "Identity");
			string? manifestVersion = identityNode?.Attribute("Version")?.Value;

			identityNode.Should().NotBeNull();
			manifestVersion.Should().NotBeNullOrWhiteSpace();
			manifestVersion.Should().Be(AcuminatorVSPackage.PackageVersion);
		}

		private string GetFilePath([CallerFilePath] string? filePath = null) => filePath!;
	}
}