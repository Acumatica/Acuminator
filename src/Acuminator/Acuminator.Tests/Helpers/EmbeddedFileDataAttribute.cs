using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Xunit.Sdk;
using Acuminator.Utilities;

namespace Acuminator.Tests.Helpers
{
	public class EmbeddedFileDataAttribute : DataAttribute
	{
		private const string TestsRoot = "Acuminator.Tests";
		private const string SourcesPrefix = "Sources";
		private const string DefaultPrefix = TestsRoot + "." + SourcesPrefix;

		private readonly string _prefix; 
		private readonly string[] _fileNames;

		public EmbeddedFileDataAttribute(string[] fileNames, bool paramForOverloadResolution, [CallerFilePath]string testFilePath = null)
		{
			if (fileNames.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(fileNames));

			_prefix = GetPrefixFromTestFilePath(testFilePath);
			_fileNames = fileNames;
		}

		public EmbeddedFileDataAttribute(params string[] fileNames)
	    {
			if (fileNames.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(fileNames));

			_prefix = DefaultPrefix;
			_fileNames = fileNames;
	    }

		public override IEnumerable<object[]> GetData(MethodInfo testMethod)
		{
			yield return _fileNames.Select(ReadFile).ToArray<object>();
		}

		private static string GetPrefixFromTestFilePath(string testFilePath)
		{
			string folderFullPath = testFilePath.IsNullOrWhiteSpace()
				? null
				: Path.GetDirectoryName(testFilePath);

			if (folderFullPath.IsNullOrWhiteSpace())
				return DefaultPrefix;

			int startingIndex = folderFullPath.IndexOf(TestsRoot);

			if (startingIndex < 0)
				return DefaultPrefix;

			string prefix = folderFullPath.Substring(startingIndex);
			string preparedPrefix = TransformFileNameToAssemblyResourceID(prefix);
			return !preparedPrefix.IsNullOrWhiteSpace() 
					? $"{preparedPrefix}.{SourcesPrefix}"
					: DefaultPrefix;
		}

		private string ReadFile(string fileName)
		{	
			var assembly = Assembly.GetExecutingAssembly();
			string resourceID = TransformFileNameToAssemblyResourceID(fileName);

			using (var stream = assembly.GetManifestResourceStream($"{_prefix}.{resourceID}"))
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string TransformFileNameToAssemblyResourceID(string fileName) =>
			fileName?.Replace(Path.DirectorySeparatorChar, '.')
					 .Replace(' ', '_');
	}
}
