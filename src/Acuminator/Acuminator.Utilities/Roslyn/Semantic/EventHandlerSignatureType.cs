namespace Acuminator.Utilities.Roslyn.Semantic
{
	public enum EventHandlerSignatureType
	{
		/// <summary>
		/// Method is not an event handler
		/// </summary>
		None,

		/// <summary>
		/// Default signature based on the naming convention
		/// (e.g., <code>void ARInvoice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)</code>)
		/// </summary>
		Default,

		/// <summary>
		/// Generic signature (e.g., <code>void _(Events.RowSelected&lt;ARInvoice e&gt;)</code>)
		/// </summary>
		Generic,
	}
}
