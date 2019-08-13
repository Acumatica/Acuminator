using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	/// <summary>
	/// A base interface for semantic models. 
	/// </summary>
	public interface ISemanticModel
	{
		/// <summary>
		/// The symbol for which the model is defined.
		/// </summary>
		INamedTypeSymbol Symbol { get; }
	}
}
