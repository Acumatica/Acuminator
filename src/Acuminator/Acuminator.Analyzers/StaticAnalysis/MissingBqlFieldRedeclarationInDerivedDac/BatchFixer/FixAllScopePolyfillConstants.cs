using FixAllScope = Microsoft.CodeAnalysis.CodeFixes.FixAllScope;

namespace Acuminator.Analyzers.StaticAnalysis.MissingBqlFieldRedeclarationInDerived;

/// <summary>
/// Class with integer constants for <see cref="FixAllScope"/> that provides polyfill constant values for <see cref="FixAllScope"/>.ContainingMember<br/>
/// and <see cref="FixAllScope"/>.ContainingType. These constants are not available in earlier versions of Roslyn such as Roslyn 3.11.0.
/// </summary>
internal static class FixAllScopePolyfillConstants
{
	public const int Document = (int)FixAllScope.Document;

	public const int Project = (int)FixAllScope.Project;

	public const int Solution = (int)FixAllScope.Solution;

	public const int Custom = (int)FixAllScope.Custom;

	public const int ContainingMember = 4;

	public const int ContainingType = 5;
}