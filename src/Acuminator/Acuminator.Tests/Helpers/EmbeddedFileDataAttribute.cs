using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit.Sdk;
using Acuminator.Utilities.Common;

namespace Acuminator.Tests.Helpers
{
	public class EmbeddedFileData2Attribute : EmbeddedFileDataAttribute
	{
		public EmbeddedFileData2Attribute(string fileName, string[] assemblies, bool overloadParams = true,
			[CallerFilePath] string testFilePath = null): base( new [] {fileName}, testFilePath, assemblies: assemblies)
		{
		}
	}

	public class EmbeddedFileDataAttribute : DataAttribute
	{
		private const string SourcesPrefix = "Sources";
		private static readonly string TestsRoot = typeof(EmbeddedFileDataAttribute).Assembly.GetName().Name;
		private static readonly string DefaultPrefix = TestsRoot + "." + SourcesPrefix;

		private readonly string _prefix;
		private readonly string[] _fileNames;
		private readonly string[] _assemblies;

		public EmbeddedFileDataAttribute(string fileName, bool overloadParam = true,
			[CallerFilePath] string testFilePath = null)
			: this(new[] { fileName }, testFilePath)
		{
		}

		public EmbeddedFileDataAttribute(string fileName1, string fileName2, bool overloadParam = true,
			[CallerFilePath] string testFilePath = null)
			: this(new[] { fileName1, fileName2 }, testFilePath)
		{
		}

		public EmbeddedFileDataAttribute(string fileName1, string fileName2, string fileName3, bool overloadParam = true,
			[CallerFilePath] string testFilePath = null)
			: this(new[] { fileName1, fileName2, fileName3 }, testFilePath)
		{
		}
		public EmbeddedFileDataAttribute(string fileName, string[] internalCodeFileNames, bool overloadParam = true,
			[CallerFilePath] string testFilePath = null)
			: this(new[] { fileName }, testFilePath, internalCodeFileNames)
		{ }

		protected EmbeddedFileDataAttribute(string[] fileNames, string testFilePath, string[] externalCodeFileNames = null, string[] assemblies = null)
		{
			if (fileNames.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(fileNames));

			foreach (string fileName in fileNames)
			{
				if (String.IsNullOrWhiteSpace(fileName))
					// ReSharper disable once LocalizableElement
					throw new ArgumentException("File name cannot be empty", nameof(fileNames));
			}
			if (!externalCodeFileNames.IsNullOrEmpty())
			{
				foreach (string internalCodeFileName in externalCodeFileNames)
				{
					if (String.IsNullOrWhiteSpace(internalCodeFileName))
						throw new ArgumentException("File name cannot be empty", nameof(internalCodeFileName));
				}
			}

			if (!assemblies.IsNullOrEmpty())
			{
				foreach (var assembly in assemblies)
				{
					if (String.IsNullOrWhiteSpace(assembly))
						throw new ArgumentException("File name cannot be empty", nameof(assembly));
				}
			}

			_prefix = GetPrefixFromTestFilePath(testFilePath);

			_fileNames = externalCodeFileNames.IsNullOrEmpty() ? fileNames :  fileNames.Concat(externalCodeFileNames).ToArray();

			_assemblies = assemblies;
		}

		public override IEnumerable<object[]> GetData(MethodInfo testMethod)
		{
			List<string> data = _fileNames.Select(ReadFile).ToList();
			if (_assemblies != null)
				data.AddRange(_assemblies);
			yield return data.ToArray<object>();
		}

		private static string GetPrefixFromTestFilePath(string testFilePath)
		{
			string folderFullPath = testFilePath.IsNullOrWhiteSpace()
				? null
				: Path.GetDirectoryName(testFilePath);

			if (folderFullPath.IsNullOrWhiteSpace())
				return DefaultPrefix;

			// ReSharper disable once PossibleNullReferenceException
			int startingIndex = folderFullPath.IndexOf(TestsRoot, StringComparison.OrdinalIgnoreCase);

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
