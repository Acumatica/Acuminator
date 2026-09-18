using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using Acuminator.Runner.Output.Data;

namespace Acuminator.Runner.Output.Json
{
	/// <summary>
	/// JSON title converter.
	/// </summary>
	internal class TitleConverter : JsonConverter<Title>
	{
		public override Title Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			throw new NotSupportedException();

		public override void Write(Utf8JsonWriter writer, Title title, JsonSerializerOptions options)
		{
			if (!string.IsNullOrWhiteSpace(title.Text))
				writer.WriteStringValue(title.Text);
		}
	}
}