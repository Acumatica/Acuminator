using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis.Formatting;

namespace Acuminator.Tests.Helpers
{
	public class EmbeddedFileDataWithParamsAttribute : EmbeddedFileDataAttribute
	{
		private readonly object[] _args;

		public EmbeddedFileDataWithParamsAttribute(string fileName, object[] args,
			[CallerFilePath] string testFilePath = null)
			: this(new[] { fileName }, args, testFilePath)
		{
		}

		public EmbeddedFileDataWithParamsAttribute(string fileName1, string fileName2, object[] args,
			[CallerFilePath] string testFilePath = null)
			: this(new[] { fileName1, fileName2 }, args, testFilePath)
		{
		}

		public EmbeddedFileDataWithParamsAttribute(string fileName1, string fileName2, string fileName3, object[] args,
			[CallerFilePath] string testFilePath = null)
			: this(new[] { fileName1, fileName2, fileName3 }, args, testFilePath)
		{
		}

		protected EmbeddedFileDataWithParamsAttribute(string[] fileNames, object[] args, string testFilePath)
			: base(fileNames, testFilePath)
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
