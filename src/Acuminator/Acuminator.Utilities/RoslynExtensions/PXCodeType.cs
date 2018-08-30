using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Acuminator.Utilities
{
	/// <summary>
	/// Values that represent Acumatica specific classification types for the coloring of code fragments and other purposes.
	/// </summary>
	public enum PXCodeType
	{
		BqlCommand,
		BqlOperator,
		Dac,
		DacField,
		DacExtension,
		BqlParameter,
		BQLConstantPrefix,
		BQLConstantEnding,
		PXGraph,
		PXAction
	}
}
