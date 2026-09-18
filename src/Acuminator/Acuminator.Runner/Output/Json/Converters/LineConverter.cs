using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using Acuminator.Runner.Output.Data;
using Acuminator.Utilities.Common;

using static Acuminator.Runner.Constants.Constant.Output;

namespace Acuminator.Runner.Output.Json
{
	/// <summary>
	/// JSON line converter.
	/// </summary>
	internal class LineConverter : JsonConverter<Line>
	{
		public override Line Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			throw new NotSupportedException();

		public override void Write(Utf8JsonWriter writer, Line line, JsonSerializerOptions options)
		{
			switch (line.Spans.Length)
			{
				case 0:
					return;

				case 2:
					{
						var (diagnosticContent, location) = (line.Spans[0].ToString(), line.Spans[1].ToString());

						writer.WriteStringValue($"{diagnosticContent}: {location}");
						return;
					}
				case 3:
					{
						var (diagnosticId, diagnosticMessage, location) = 
							(line.Spans[0].ToString(), line.Spans[1].ToString(), line.Spans[2].ToString());
						writer.WriteStringValue($"{diagnosticId}{LinePartsSeparator}{diagnosticMessage}{LinePartsSeparator}{location}");
						return;
					}

				case 4:
					{
						var (severity, diagnosticId, diagnosticMessage, location) = 
							(line.Spans[0].ToString(), line.Spans[1].ToString(), line.Spans[2].ToString(), line.Spans[3].ToString());
						var formattedSeverity = string.Format(SeverityTemplate, severity);
						writer.WriteStringValue($"{formattedSeverity}{diagnosticId}{LinePartsSeparator}{diagnosticMessage}{LinePartsSeparator}{location}");
						return;
					}

				default:
					string lineStr = line.ToString();

					if (!lineStr.IsNullOrWhiteSpace())
						writer.WriteStringValue(lineStr);

					return;
			}
		}
	}
}