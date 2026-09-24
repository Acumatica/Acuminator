using System.Collections.Generic;

using Acuminator.Utilities.DiagnosticSuppression;

using FluentAssertions;

using Xunit;

namespace Acuminator.Tests.Tests.DiagnosticSuppression
{
	/// <summary>
	/// Direct unit tests for <see cref="SuppressionFile.LoadMessagesFromString"/>, which parses the content of an Acuminator
	/// suppression file passed to a compilation as an additional file (the NuGet package scenario).
	/// </summary>
	public class LoadMessagesFromStringTests
	{
		[Fact]
		public void ValidXml_ParsesAllMessages()
		{
			const string content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<suppressions>
  <suppressMessage id=""PX1001"">
    <target>PX.Objects.Foo.Bar()</target>
    <syntaxNode>new Baz()</syntaxNode>
  </suppressMessage>
  <suppressMessage id=""PX1094"">
    <target>PX.Objects.Foo</target>
    <syntaxNode>Foo</syntaxNode>
  </suppressMessage>
</suppressions>";

			HashSet<SuppressMessage> messages = SuppressionFile.LoadMessagesFromString(content);

			messages.Should().HaveCount(2);
			messages.Should().Contain(new SuppressMessage("PX1001", "PX.Objects.Foo.Bar()", "new Baz()"));
			messages.Should().Contain(new SuppressMessage("PX1094", "PX.Objects.Foo", "Foo"));
		}

		[Fact]
		public void EmptySuppressionsElement_ReturnsEmpty()
		{
			const string content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<suppressions>
</suppressions>";

			SuppressionFile.LoadMessagesFromString(content).Should().BeEmpty();
		}

		[Fact]
		public void MalformedXml_ReturnsEmpty() =>
			SuppressionFile.LoadMessagesFromString(@"<suppressions><suppressMessage id=").Should().BeEmpty();

		[Fact]
		public void MessageWithoutTarget_IsSkipped()
		{
			const string content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<suppressions>
  <suppressMessage id=""PX1001"">
    <syntaxNode>new Baz()</syntaxNode>
  </suppressMessage>
</suppressions>";

			SuppressionFile.LoadMessagesFromString(content).Should().BeEmpty();
		}

		[Fact]
		public void DuplicateMessages_AreDeduplicated()
		{
			const string content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<suppressions>
  <suppressMessage id=""PX1001"">
    <target>PX.Objects.Foo.Bar()</target>
    <syntaxNode>new Baz()</syntaxNode>
  </suppressMessage>
  <suppressMessage id=""PX1001"">
    <target>PX.Objects.Foo.Bar()</target>
    <syntaxNode>new Baz()</syntaxNode>
  </suppressMessage>
</suppressions>";

			SuppressionFile.LoadMessagesFromString(content).Should().HaveCount(1);
		}

		[Theory]
		[InlineData("")]
		[InlineData("   ")]
		public void EmptyOrWhitespace_ReturnsEmpty(string content) =>
			SuppressionFile.LoadMessagesFromString(content).Should().BeEmpty();
	}
}
