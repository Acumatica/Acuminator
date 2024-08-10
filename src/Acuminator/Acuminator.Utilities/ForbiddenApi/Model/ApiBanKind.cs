#nullable enable

using System;

namespace Acuminator.Utilities.ForbiddenApi.Model
{
	[Flags]
	public enum ApiBanKind : byte
	{
		None	  = 0b0000,
		Acumatica = 0b0001,
		ISV		  = 0b0010,
		All		  = Acumatica | ISV,
	}

	public static class ApiBanKindUtils
	{
		public static bool IsAcumatica(this ApiBanKind kind) => kind.IsKind(ApiBanKind.Acumatica);

		public static bool IsISV(this ApiBanKind kind) => kind.IsKind(ApiBanKind.ISV);
	
		public static bool IsKind(this ApiBanKind kind, ApiBanKind kindToCheck) => (kind & kindToCheck) == kindToCheck;
	}
}
