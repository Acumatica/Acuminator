using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Utilities.DiagnosticSuppression;

internal static class AdditionalFileSuppressionReader
{
	private static readonly ConditionalWeakTable<SourceText, Lazy<HashSet<SuppressMessage>>>
		_suppressionMessagesBySourceText = new();

	private static HashSet<SuppressMessage> GetOrParse(SourceText fileText) =>
		_suppressionMessagesBySourceText.GetValue(fileText,
			static text => new Lazy<HashSet<SuppressMessage>>(
				() => ParseSafe(text),
				LazyThreadSafetyMode.ExecutionAndPublication)).Value;

	private static HashSet<SuppressMessage> ParseSafe(SourceText sourceText)
	{
		try
		{
			return SuppressionFile.LoadMessagesFromString(sourceText.ToString());
		}
		catch (Exception e) when (e is not OperationCanceledException)
		{
			// Must not throw: Lazy(ExecutionAndPublication) would cache the exception and poison this SourceText
			// until it changes. A malformed file yields no suppressions.
			return [];
		}
	}

	[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1012:Start action has no registered actions",
		Justification =
			"Context is used only to read additional files and memoize parsed suppressions per SourceText, not to register actions")]
	internal static HashSet<SuppressMessage>? ParseSuppressionsForAssembly(
		CompilationStartAnalysisContext compilationStartContext, string assemblyName)
	{
		compilationStartContext.ThrowOnNull();
		try
		{
			foreach (AdditionalText additionalFile in compilationStartContext.Options.AdditionalFiles)
			{
				compilationStartContext.CancellationToken.ThrowIfCancellationRequested();

				if (!additionalFile.Path.IsSuppressionFile(checkFileExists: false))
					continue;

				// A suppression file is matched to the project by name: only the file whose name equals the assembly
				// name applies (files with a non-matching name are ignored per the Acuminator suppression docs), and
				// there is at most one such file per project.
				string fileAssemblyName = Path.GetFileNameWithoutExtension(additionalFile.Path);
				if (!string.Equals(fileAssemblyName, assemblyName, StringComparison.Ordinal))
					continue;

				SourceText? fileText = additionalFile.GetText(compilationStartContext.CancellationToken);
				if (fileText == null)
					return null;

				HashSet<SuppressMessage> suppressions = GetOrParse(fileText);

				return suppressions.Count == 0 ? null : suppressions;
			}

			return null;
		}
		catch (Exception e) when (e is not OperationCanceledException)
		{
			return null;
		}
	}
}
