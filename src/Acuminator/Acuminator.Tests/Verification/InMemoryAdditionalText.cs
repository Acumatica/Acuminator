using System.Threading;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Tests.Verification
{
	/// <summary>
	/// An in-memory additional file (analyzer additional text) for tests.
	/// </summary>
	public sealed class InMemoryAdditionalText : AdditionalText
	{
		private readonly SourceText _text;

		public override string Path { get; }

		public InMemoryAdditionalText(string path, string text)
		{
			Path = path.CheckIfNullOrWhiteSpace();
			_text = SourceText.From(text.CheckIfNull());
		}

		public override SourceText GetText(CancellationToken cancellationToken = default) => _text;
	}
}
