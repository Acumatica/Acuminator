using System;
using System.Text.Json.Serialization;

using Acuminator.Runner.Output.Json;
using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Output.Data
{
	[JsonConverter(typeof(TitleConverter))]
	internal readonly struct Title
	{
		[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
		public TitleKind Kind { get; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string Text { get; }

        public Title(string text, TitleKind titleKind)
        {
			Text = text.CheckIfNullOrWhiteSpace();
			Kind = titleKind;
        }

		public override string ToString() => $"{Kind.ToString()}: { Text ?? string.Empty }";
	}
}
