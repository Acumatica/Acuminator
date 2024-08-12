#nullable enable

using System;
using System.Collections.Generic;

namespace Acuminator.Utilities.BannedApi.Model
{
	[Flags]
	public enum ApiBanKind : byte
	{
		None	  = 0b0000,
		General	  = 0b0001,
		ISV		  = 0b0010,
		All		  = General | ISV,
	}

	public static class ApiBanKindUtils
	{
		public static bool IsGeneral(this ApiBanKind kind) => kind.IsKind(ApiBanKind.General);

		public static bool IsISV(this ApiBanKind kind) => kind.IsKind(ApiBanKind.ISV);
	
		public static bool IsKind(this ApiBanKind kind, ApiBanKind kindToCheck) => (kind & kindToCheck) == kindToCheck;
	}
}
