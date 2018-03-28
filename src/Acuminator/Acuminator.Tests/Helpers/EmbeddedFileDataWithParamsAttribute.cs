using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Formatting;

namespace Acuminator.Tests.Helpers
{
	public class EmbeddedFileDataWithParamsAttribute : EmbeddedFileDataAttribute
	{
		private readonly object[] _args;

		public EmbeddedFileDataWithParamsAttribute(string fileName1, string fileName2, params object[] args)
			: base(fileName1, fileName2)
		{
			_args = args;
		}

		public EmbeddedFileDataWithParamsAttribute(string fileName, params object[] args)
			: base(fileName)
		{
			_args = args;
		}

		public override IEnumerable<object[]> GetData(MethodInfo testMethod)
		{
			foreach (var item in base.GetData(testMethod))
			{
				yield return item.Concat(_args).ToArray();
			}
		}
	}
}
