using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn;

/// <summary>
/// Caches Roslyn symbol lookup results for expression syntax nodes visited by a syntax walker.
/// </summary>
public sealed class SymbolInfoCache
{
	private readonly Dictionary<ExpressionSyntax, SymbolInfo?> _map = new();

	/// <summary>
	/// Gets the cached symbol information for the specified expression, or creates and stores it using the provided factory.
	/// </summary>
	public SymbolInfo? GetOrCreate(ExpressionSyntax key, Func<SymbolInfo?> factory)
	{
		if (_map.TryGetValue(key, out SymbolInfo? cached))
		{
			return cached;
		}

		SymbolInfo? potentialValue = factory();
		_map[key] = potentialValue;

		return potentialValue;
	}
}

