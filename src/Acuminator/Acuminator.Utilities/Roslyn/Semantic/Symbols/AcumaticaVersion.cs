#nullable enable

using System.Linq;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols;

public sealed class AcumaticaVersion
{
	public readonly int Major;
	public readonly int Minor;
	public readonly int Build;

	public AcumaticaVersion(IAssemblySymbol? assemblySymbol)
	{
		if
		(
			assemblySymbol == null
			|| assemblySymbol.GetAttributes().FirstOrDefault(x => x?.AttributeClass?.Name == "AssemblyFileVersionAttribute") is not { } attributeData
			|| attributeData.ConstructorArguments is not { Length: > 0 } constructorArguments
			|| constructorArguments[0].Value is not string { } acumaticaVersion
			|| acumaticaVersion.Split('.') is not { Length: > 2 } versionParts
			|| !int.TryParse(versionParts[0], out int major)
			|| !int.TryParse(versionParts[1], out int minor)
			|| !int.TryParse(versionParts[2], out int build)
		)
			return;

		Major = major;
		Minor = minor;
		Build = build;
	}
}