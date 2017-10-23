using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace PX.Analyzers.Coloriser
{
	public static class StringExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);
	}   
}
