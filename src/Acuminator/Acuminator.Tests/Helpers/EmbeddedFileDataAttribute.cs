using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Acuminator.Tests.Helpers
{
	public class EmbeddedFileDataAttribute : DataAttribute
	{
	    private readonly string[] _fileNames;

	    public EmbeddedFileDataAttribute(params string[] fileNames)
	    {
	        _fileNames = fileNames;
	    }

		public override IEnumerable<object[]> GetData(MethodInfo testMethod)
		{
		    yield return _fileNames.Select(ReadFile).ToArray<object>();
		}

		private static string ReadFile(string fileName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			fileName = fileName?.Replace('\\', '.');
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
