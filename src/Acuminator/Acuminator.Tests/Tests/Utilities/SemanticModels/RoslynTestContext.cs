#nullable enable

using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels
{
	public readonly record struct RoslynTestContext(Document Document, SemanticModel SemanticModel, SyntaxNode Root, PXContext PXContext){ }
}
