using System;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	/// <summary>
	/// Values that represent types of referential integrity DAC keys.
	/// </summary>
	public enum RefIntegrityDacKeyType : byte
	{
		PrimaryKey,

		UniqueKey,

		ForeignKey
	}
}
