using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Acuminator.Utilities.Common
{
	/// <summary>
	/// A <see cref="StringWriter"/> derived class that allows to specify UTF-8 encoding for <see cref="StringWriter"/>.
	/// </summary>
	public sealed class Utf8StringWriter : StringWriter
	{
		public override Encoding Encoding => Encoding.UTF8;

		public Utf8StringWriter() : base() { }

		public Utf8StringWriter(IFormatProvider formatProvider) : base(formatProvider) { }

		public Utf8StringWriter(StringBuilder sb) : base(sb) { }

		public Utf8StringWriter(StringBuilder sb, IFormatProvider formatProvider) : base(sb, formatProvider) { }
	}
}
