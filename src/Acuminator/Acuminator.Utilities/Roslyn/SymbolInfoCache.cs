using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn;

/// <summary>
/// Caches Roslyn symbol lookup results for expression syntax nodes visited by a syntax walker.
/// </summary>
public sealed class SymbolInfoCache
{
	private readonly Dictionary<ExpressionSyntax, SymbolInfo> _map = new();
#if SYMBOL_INFO_CACHE_STATISTICS
	private readonly SymbolInfoCacheStatistics _statistics;
#endif

	public SymbolInfoCache(Type owner)
	{
#if SYMBOL_INFO_CACHE_STATISTICS
		_statistics = SymbolInfoCacheStatisticsContext.Register(owner);
#endif
	}

	/// <summary>
	/// Gets the cached symbol information for the specified expression, or creates and stores it using the provided factory.
	/// </summary>
	public SymbolInfo? GetOrCreate(ExpressionSyntax key, Func<SymbolInfo?> factory)
	{
		if (_map.TryGetValue(key, out SymbolInfo cached))
		{
#if SYMBOL_INFO_CACHE_STATISTICS
			_statistics.Hit();
#endif
			return cached;
		}

#if SYMBOL_INFO_CACHE_STATISTICS
		_statistics.Miss();
#endif

		SymbolInfo? potentialValue = factory();
		if (potentialValue is not null)
		{
			_map[key] = potentialValue.Value;
#if SYMBOL_INFO_CACHE_STATISTICS
			_statistics.Set(key.GetType());
#endif
			return potentialValue;
		}

		return null;
	}

#if SYMBOL_INFO_CACHE_STATISTICS
	public sealed class SymbolInfoCacheStatistics
	{
		private readonly Dictionary<Type, long> _byTypeCounter = new();
		private readonly Type _owner;
		private readonly int _cacheId;

		private long _hitsTotal;
		private long _missesTotal;
		private long _setsTotal;

		public SymbolInfoCacheStatistics(int cacheId, Type owner)
		{
			_owner = owner;
			_cacheId = cacheId;
		}

		public void Hit() => _hitsTotal++;

		public void Miss() => _missesTotal++;

		public void Set(Type type)
		{
			if (!_byTypeCounter.ContainsKey(type))
			{
				_byTypeCounter[type] = 1;
			}
			else
			{
				_byTypeCounter[type]++;
			}
			_setsTotal++;
		}

		public StatisticsSnapshot GetSnapshot()
		{
			var lookupsTotal = _hitsTotal + _missesTotal;
			return new StatisticsSnapshot(
				_cacheId,
				_owner,
				_hitsTotal,
				_missesTotal,
				_setsTotal,
				_byTypeCounter,
				CalcRatio(_hitsTotal, lookupsTotal),
				CalcRatio(_missesTotal, lookupsTotal)
			);
		}
		
		private static double CalcRatio(long value, long total) => total == 0 ? 0 : (double)value / total;
	}

	public static class SymbolInfoCacheStatisticsContext
	{
		private static readonly object Gate = new();
		private static readonly List<SymbolInfoCacheStatistics> Statistics = new();
		private static int _nextCacheId;

		internal static SymbolInfoCacheStatistics Register(Type owner)
		{
			var cacheId = Interlocked.Increment(ref _nextCacheId);
			var statistics = new SymbolInfoCacheStatistics(cacheId, owner);

			lock (Gate)
			{
				Statistics.Add(statistics);
			}

			return statistics;
		}

		public static StatisticsSnapshot[] GetStatistics()
		{
			lock (Gate)
			{
				var result = new StatisticsSnapshot[Statistics.Count];
				for (var i = 0; i < Statistics.Count; i++)
				{
					result[i] = Statistics[i].GetSnapshot();
				}
				
				return result;
			}
		}
	}

	public record StatisticsSnapshot(int CacheId, Type Owner, long HitsTotal, long MissesTotal, long SetsTotal, Dictionary<Type, long> ByTypeCounter, double HitRatio, double MissRatio);
#endif
}

