using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Acuminator.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.MissingSchemaMutationGuard;

/// <summary>
/// PX1121: Detects calls to <c>PXDatabase.Execute</c> with schema-mutation SQL
/// (<c>ALTER TABLE ... ADD</c>, <c>CREATE TABLE</c>, <c>CREATE INDEX</c>) that lack
/// an existence guard preceding that specific mutation.
///
/// Acumatica customization plugins re-run <c>UpdateDatabase</c> on every publish;
/// an unguarded schema mutation will fail with "column already exists" / "table
/// already exists" on the second publish, breaking the deploy.
///
/// Guard-scope detection is per-mutation: for each mutation match in the SQL, we
/// look at the preceding text (scoped to the current semicolon chunk) and check
/// whether the *latest* match is a guard pattern (<c>IF NOT EXISTS</c>,
/// <c>IF COL_LENGTH</c>, <c>IF OBJECT_ID</c>, <c>IF INDEXPROPERTY</c>,
/// <c>FROM sys.columns/tables/indexes</c>) or another action statement keyword
/// (<c>PRINT</c>, <c>EXEC</c>, <c>INSERT</c>, etc.). A guard occluded by a later
/// action-statement keyword no longer counts for that mutation, so the canonical
/// false-negative pattern <c>IF EXISTS(...) PRINT 'x' ALTER TABLE ... ADD ...</c>
/// (where the IF guards the PRINT and the ALTER is unguarded) is flagged correctly
/// even without semicolons between statements.
///
/// Comments and string literals inside the SQL are stripped before matching so
/// their contents do not produce false positives or false negatives.
///
/// This rule is not gated to ISVs because customer-written customization plugins
/// are equally affected by the double-publish failure mode.
///
/// Known limitations (heuristic-based; full T-SQL parse would resolve them):
///   - Nested block comments and bracketed-identifier escape sequences
///     (<c>[a]]b]</c>) are not modeled.
///   - For interpolated strings with holes, literal segments are scanned;
///     a hole that stands in for a DDL keyword/modifier (e.g.
///     <c>$"CREATE {kind} INDEX ..."</c>) is not modeled.
///   - Runtime-computed SQL (<c>StringBuilder</c>-concatenated, non-constant)
///     cannot be analyzed statically and is silently skipped.
///   - <c>SELECT</c> is excluded from action keywords so it doesn't break
///     guards like <c>IF EXISTS (SELECT 1 FROM sys.X)</c>; consequence is that
///     <c>IF cond SELECT 1 ALTER TABLE ... ADD</c> is treated as guarded
///     (false negative on a rare pattern).
///   - <c>ELSE</c> branches: code like <c>IF cond PRINT 'x' ELSE ALTER TABLE ...
///     ADD</c> is flagged because <c>PRINT</c> occludes the guard for the
///     <c>ELSE</c>-branch <c>ALTER</c>. Suppress via comment for this pattern.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MissingSchemaMutationGuardAnalyzer : PXDiagnosticAnalyzer
{
	private const string ExecuteMethodName = "Execute";

	private const string InterpolationHolePlaceholder = " __pxsb_hole__ ";

	// Schema-mutation patterns. [^;] keeps each match inside one statement.
	private static readonly Regex AlterTableAddPattern = new(
		@"\bALTER\s+TABLE\b[^;]*?\bADD\b",
		RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private static readonly Regex CreateTablePattern = new(
		@"\bCREATE\s+TABLE\b",
		RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private static readonly Regex CreateIndexPattern = new(
		@"\bCREATE\s+(UNIQUE\s+)?(NONCLUSTERED\s+|CLUSTERED\s+)?INDEX\b",
		RegexOptions.IgnoreCase | RegexOptions.Compiled);

	// Existence guards. Optional parens + whitespace handle styles like
	// IF (COL_LENGTH(...) IS NULL) and IF NOT EXISTS(SELECT ...).
	private static readonly Regex GuardPattern = new(
		@"\bIF\s*\(?\s*(NOT\s+)?EXISTS\b"
		+ @"|\bIF\s*\(?\s*COL_LENGTH\b"
		+ @"|\bIF\s*\(?\s*OBJECT_ID\b"
		+ @"|\bIF\s*\(?\s*INDEXPROPERTY\b"
		+ @"|\bFROM\s+SYS\.(COLUMNS|TABLES|INDEXES)\b",
		RegexOptions.IgnoreCase | RegexOptions.Compiled);

	// Action-statement keywords that BREAK a guard's scope when they appear
	// between the guard and the mutation. Intentionally excludes:
	//   - SELECT (appears inside IF EXISTS guard conditions)
	//   - IF, THEN, ELSE, BEGIN, END (control flow that preserves guard scope)
	// Includes ALTER/CREATE because a second mutation breaks the guard for the
	// first one — each mutation needs its own guard.
	private static readonly Regex ActionStatementKeywords = new(
		@"\b(ALTER|CREATE|DROP|TRUNCATE|INSERT|UPDATE|DELETE|MERGE|EXEC|EXECUTE|PRINT|RAISERROR|WAITFOR)\b",
		RegexOptions.IgnoreCase | RegexOptions.Compiled);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
		ImmutableArray.Create(Descriptors.PX1121_MissingSchemaMutationGuard);

	public MissingSchemaMutationGuardAnalyzer() : base() { }

	public MissingSchemaMutationGuardAnalyzer(CodeAnalysisSettings codeAnalysisSettings) : base(codeAnalysisSettings) { }

	protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
	{
		compilationStartContext.RegisterSyntaxNodeAction(
			syntaxContext => AnalyzeInvocation(syntaxContext, pxContext),
			SyntaxKind.InvocationExpression);
	}

	private void AnalyzeInvocation(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
	{
		syntaxContext.CancellationToken.ThrowIfCancellationRequested();

		var invocation = (InvocationExpressionSyntax)syntaxContext.Node;

		if (!IsPxDatabaseExecuteCall(invocation, syntaxContext.SemanticModel, pxContext, syntaxContext.CancellationToken))
			return;

		var sqlArgument = invocation.ArgumentList.Arguments.FirstOrDefault();
		if (sqlArgument is null)
			return;

		string? sqlText = TryGetStringValue(sqlArgument.Expression, syntaxContext.SemanticModel, syntaxContext.CancellationToken);
		if (sqlText is null)
			return;

		if (!ContainsUnguardedSchemaMutation(sqlText))
			return;

		syntaxContext.ReportDiagnosticWithSuppressionCheck(
			Diagnostic.Create(Descriptors.PX1121_MissingSchemaMutationGuard, invocation.GetLocation()),
			pxContext.CodeAnalysisSettings);
	}

	private static bool IsPxDatabaseExecuteCall(
		InvocationExpressionSyntax invocation,
		SemanticModel semanticModel,
		PXContext pxContext,
		System.Threading.CancellationToken cancellationToken)
	{
		var symbolInfo = semanticModel.GetSymbolInfo(invocation, cancellationToken);
		if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
			return false;

		if (methodSymbol.Name != ExecuteMethodName)
			return false;

		var pxDatabaseType = pxContext.PXDatabase.Type;
		if (pxDatabaseType is null)
			return false;

		return SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, pxDatabaseType);
	}

	private static string? TryGetStringValue(ExpressionSyntax expression, SemanticModel semanticModel, System.Threading.CancellationToken cancellationToken)
	{
		var constant = semanticModel.GetConstantValue(expression, cancellationToken);
		if (constant.HasValue && constant.Value is string constStr)
			return constStr;

		if (expression is InterpolatedStringExpressionSyntax interp)
		{
			var sb = new StringBuilder();
			bool any = false;
			foreach (var content in interp.Contents)
			{
				if (content is InterpolatedStringTextSyntax text)
				{
					sb.Append(text.TextToken.ValueText);
					any = true;
				}
				else
				{
					sb.Append(InterpolationHolePlaceholder);
					any = true;
				}
			}
			return any ? sb.ToString() : null;
		}

		return null;
	}

	/// <summary>
	/// Per-mutation guard detection. For each schema mutation in the sanitized SQL,
	/// scan the preceding text and find the latest match of either a guard pattern
	/// or an action-statement keyword. A guard occluded by a later action keyword
	/// (e.g. an unrelated <c>PRINT</c>) no longer counts.
	/// </summary>
	private static bool ContainsUnguardedSchemaMutation(string sql)
	{
		string sanitized = StripCommentsAndStringLiterals(sql);

		var mutations = new List<Match>();
		foreach (Match m in AlterTableAddPattern.Matches(sanitized)) mutations.Add(m);
		foreach (Match m in CreateTablePattern.Matches(sanitized)) mutations.Add(m);
		foreach (Match m in CreateIndexPattern.Matches(sanitized)) mutations.Add(m);

		foreach (var mutation in mutations)
		{
			if (!IsGuardedAt(sanitized, mutation.Index))
				return true;
		}

		return false;
	}

	private static bool IsGuardedAt(string sanitized, int mutationIndex)
	{
		// Scope the search to the current ;-chunk (defensive boundary).
		int chunkStart = mutationIndex > 0
			? sanitized.LastIndexOf(';', mutationIndex - 1) + 1
			: 0;
		if (chunkStart < 0) chunkStart = 0;
		if (chunkStart >= mutationIndex) return false;

		string chunk = sanitized.Substring(chunkStart, mutationIndex - chunkStart);

		int latestGuardIdx = -1;
		foreach (Match m in GuardPattern.Matches(chunk))
		{
			if (m.Index > latestGuardIdx) latestGuardIdx = m.Index;
		}

		if (latestGuardIdx == -1) return false;

		int latestActionIdx = -1;
		foreach (Match m in ActionStatementKeywords.Matches(chunk))
		{
			if (m.Index > latestActionIdx) latestActionIdx = m.Index;
		}

		// Guard applies only if it appears AFTER the last action keyword.
		return latestGuardIdx > latestActionIdx;
	}

	/// <summary>
	/// Strip SQL comments (-- line, /* block */) and string literals ('...')
	/// so keyword matching does not match keywords appearing inside them.
	/// Replaces stripped characters with whitespace (preserving newlines).
	/// </summary>
	private static string StripCommentsAndStringLiterals(string sql)
	{
		var sb = new StringBuilder(sql.Length);
		int i = 0;
		while (i < sql.Length)
		{
			char c = sql[i];

			if (c == '-' && i + 1 < sql.Length && sql[i + 1] == '-')
			{
				while (i < sql.Length && sql[i] != '\n')
				{
					sb.Append(' ');
					i++;
				}
				continue;
			}

			if (c == '/' && i + 1 < sql.Length && sql[i + 1] == '*')
			{
				sb.Append("  ");
				i += 2;
				while (i + 1 < sql.Length && !(sql[i] == '*' && sql[i + 1] == '/'))
				{
					sb.Append(sql[i] == '\n' ? '\n' : ' ');
					i++;
				}
				if (i + 1 < sql.Length)
				{
					sb.Append("  ");
					i += 2;
				}
				continue;
			}

			if (c == '\'')
			{
				sb.Append(' ');
				i++;
				while (i < sql.Length)
				{
					if (sql[i] == '\'')
					{
						if (i + 1 < sql.Length && sql[i + 1] == '\'')
						{
							sb.Append("  ");
							i += 2;
							continue;
						}
						sb.Append(' ');
						i++;
						break;
					}
					sb.Append(sql[i] == '\n' ? '\n' : ' ');
					i++;
				}
				continue;
			}

			sb.Append(c);
			i++;
		}
		return sb.ToString();
	}
}
