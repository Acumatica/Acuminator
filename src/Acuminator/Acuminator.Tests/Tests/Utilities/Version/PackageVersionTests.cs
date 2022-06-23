using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Vsix;
using Acuminator.Tests.Verification;

using Xunit;

using FluentAssertions;

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
	}
}