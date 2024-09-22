#nullable enable

using System;

using Acuminator.Utilities.Roslyn;

using Xunit;

using FluentAssertions;

namespace Acuminator.Tests.Tests.Utilities.DataTypeToBqlFieldTypeMapping
{
	public class DataTypeNameTests 
	{
		[Theory]
		[InlineData("string", "string")]
		[InlineData("Byte", "Byte")]
		[InlineData("string[]", "string[]")]
		[InlineData("string   []", "string[]")]
		[InlineData("int[   ]", "int[]")]
		[InlineData("byte   [         ]", "byte[]")]
		public void DataTypeNamesCreation(string inputDataType, string expectedDataType)
		{
			var dataTypeName = new DataTypeName(inputDataType);
			dataTypeName.Value.Should().Be(expectedDataType);
		}
	}
}
