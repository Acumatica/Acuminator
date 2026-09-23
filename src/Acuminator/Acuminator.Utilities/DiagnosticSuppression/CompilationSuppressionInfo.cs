using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Utilities.DiagnosticSuppression;

internal sealed class CompilationSuppressionInfo
{
	private static CompilationSuppressionInfo Empty { get; } = new(null);

	private static readonly SourceTextValueProvider<ImmutableHashSet<SuppressMessage>> _suppressionMessagesProvider =
		new(fileText => SuppressionFile.LoadMessagesFromString(fileText.ToString()));

	private readonly IReadOnlyDictionary<string, ImmutableHashSet<SuppressMessage>>? _suppressionsByAssembly;

	internal bool IsEmpty => _suppressionsByAssembly is null || _suppressionsByAssembly.Count == 0;

	private CompilationSuppressionInfo(IReadOnlyDictionary<string, ImmutableHashSet<SuppressMessage>>? suppressionsByAssembly)
		=> _suppressionsByAssembly = suppressionsByAssembly;

	[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1012:Start action has no registered actions",
		Justification = "Context is used only to read additional files and memoize parsed suppressions via TryGetValue, not to register actions")]
	internal static CompilationSuppressionInfo CreateFromAdditionalFiles(CompilationStartAnalysisContext compilationStartContext)
	{
		compilationStartContext.ThrowOnNull();

		try
		{
			List<(string AssemblyName, ImmutableHashSet<SuppressMessage> Suppressions)>? suppressionsPerAssembly = null;

			foreach (AdditionalText additionalFile in compilationStartContext.Options.AdditionalFiles)
			{
				compilationStartContext.CancellationToken.ThrowIfCancellationRequested();

				if (!additionalFile.Path.IsSuppressionFile(checkFileExists: false))
					continue;

				string assemblyName = Path.GetFileNameWithoutExtension(additionalFile.Path);
				SourceText? fileText = additionalFile.GetText(compilationStartContext.CancellationToken);

				if (assemblyName.IsNullOrWhiteSpace() || fileText == null)
					continue;

				if (!compilationStartContext.TryGetValue(fileText, _suppressionMessagesProvider, out ImmutableHashSet<SuppressMessage>? suppressions))
				{
					suppressions = SuppressionFile.LoadMessagesFromString(fileText.ToString());
				}

				suppressionsPerAssembly ??= new(capacity: 1);
				suppressionsPerAssembly.Add((assemblyName, suppressions));
			}

			return suppressionsPerAssembly != null
				? Create(suppressionsPerAssembly)
				: Empty;
		}
		catch (Exception e) when (e is not OperationCanceledException)
		{
			return Empty;
		}
	}

	internal bool IsSuppressed(string assemblyName, in SuppressMessage message)
	{
		if (IsEmpty || assemblyName.IsNullOrWhiteSpace() || !message.IsValid)
		{
			return false;
		}

		return _suppressionsByAssembly!.TryGetValue(assemblyName, out ImmutableHashSet<SuppressMessage>? suppressions)
				&& suppressions.Contains(message);
	}

	private static CompilationSuppressionInfo Create(IEnumerable<(string AssemblyName, ImmutableHashSet<SuppressMessage> Suppressions)> suppressionsPerAssembly)
	{
		suppressionsPerAssembly.ThrowOnNull();

		Dictionary<string, ImmutableHashSet<SuppressMessage>>? suppressionsByAssembly = null;

		foreach (var (assemblyName, suppressions) in suppressionsPerAssembly)
		{
			if (assemblyName.IsNullOrWhiteSpace() || suppressions == null || suppressions.Count == 0)
			{
				continue;
			}

			suppressionsByAssembly ??= new(StringComparer.Ordinal);

			if (suppressionsByAssembly.TryGetValue(assemblyName, out ImmutableHashSet<SuppressMessage> existingSuppressions))
			{
				var mergedSuppressions = existingSuppressions.Union(suppressions);

				suppressionsByAssembly[assemblyName] = mergedSuppressions;
			}
			else
			{
				suppressionsByAssembly.Add(assemblyName, suppressions);
			}
		}

		return suppressionsByAssembly == null
			? Empty
			: new CompilationSuppressionInfo(suppressionsByAssembly);
	}
}
