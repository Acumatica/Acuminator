#nullable enable

namespace Acuminator.Vsix.Coloriser
{
	/// <summary>
	/// Values that represent tagger types.
	/// </summary>
	public enum TaggerType
	{
		/// <summary>
		/// The general tagger which chooses other taggers according to the settings.
		/// </summary>
		General,

		/// <summary>
		/// The tagger based on Roslyn 
		/// </summary>
		Roslyn,

		/// <summary>
		/// The tagger based on regular expressions
		/// </summary>
		RegEx,

		/// <summary>
		/// The tagger used for outlining
		/// </summary>
		Outlining
	};
}
