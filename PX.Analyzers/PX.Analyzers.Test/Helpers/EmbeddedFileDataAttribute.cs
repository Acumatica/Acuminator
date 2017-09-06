using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace PX.Analyzers.Test.Helpers
{
	public class EmbeddedFileDataAttribute : DataAttribute
	{
		private readonly string _inputFileName;
		private readonly string _expectedFileName;

		public EmbeddedFileDataAttribute(string inputFileName, string expectedFileName)
		{
			_inputFileName = inputFileName;
			_expectedFileName = expectedFileName;
		}

		public override IEnumerable<object[]> GetData(MethodInfo testMethod)
		{
			yield return new object[] { ReadFile(_inputFileName), ReadFile(_expectedFileName) };
		}

		private static string ReadFile(string fileName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var stream = assembly.GetManifestResourceStream($"PX.Analyzers.Test.Sources.{fileName}"))
			{
				if (stream != null)
				{
					using (var reader = new StreamReader(stream))
					{
						return reader.ReadToEnd();
					}
				}
			}

			return null;
		}
	}
}
