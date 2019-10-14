using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.ProjectSystem;
using Acuminator.Utilities.DiagnosticSuppression.IO;
using Acuminator.Utilities.DiagnosticSuppression.BuildAction;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public sealed partial class SuppressionManager
	{
		private class FilesStore
		{
			private readonly ConcurrentDictionary<string, SuppressionFile> _fileByAssembly = new ConcurrentDictionary<string, SuppressionFile>();

			public int Count => _fileByAssembly.Count;

			public SuppressionFile this[string assemblyName]
			{
				get 
				{
					return _fileByAssembly[assemblyName];
				}
				set 
				{
					_fileByAssembly[assemblyName] = value;
				}
			}

			public IEnumerable<SuppressionFile> Files => _fileByAssembly.Values;

			public void Clear() => _fileByAssembly.Clear();

			public bool TryAdd(string assemblyName, SuppressionFile file) => _fileByAssembly.TryAdd(assemblyName, file);

			public bool TryGetValue(string assemblyName, out SuppressionFile file) => _fileByAssembly.TryGetValue(assemblyName, out file);
		}
	}
}