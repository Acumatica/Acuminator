using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;



namespace Acuminator.Vsix.ChangesClassification
{
	/// <summary>
	/// Values that represent the syntax scope affected by the changes in the document.
	/// </summary>
	[Flags]
	public enum ChangeInfluenceScope
	{
		None = 0b0000_0000,

		/// <summary>
		/// The change affects a body of a method, property getter/setter, event add/remove or a field storing a delegate with a field initializer setting lambda.
		/// </summary>
		StatementsBlock = 0b0000_0001,

		/// <summary>
		/// The change is affects a block of attributes.
		/// </summary>
		Attributes = 0b0000_0010,

		/// <summary>
		/// The change affects a syntax node trivia.
		/// </summary>
		Trivia = 0b0000_0100,

		/// <summary>
		/// The change affects the whole class declaration.
		/// </summary>
		Class = 0b0100_0000,

		/// <summary>
		/// The change affects whole namespace declaration, for example, change of type name or declaration of a new type.
		/// </summary>
		Namespace = 0b1000_0000
	}


	public static class ChangeInfluenceScopeScopeUtils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ContainsLocation(this ChangeInfluenceScope scope, ChangeInfluenceScope scopeToCheck) => (scope & scopeToCheck) == scopeToCheck;
	}
}
