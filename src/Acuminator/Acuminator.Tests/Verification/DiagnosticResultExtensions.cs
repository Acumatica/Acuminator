using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Acuminator.Tests.Verification
{
	public static class DiagnosticResultExtensions
	{
		public static DiagnosticResult CreateFor(this DiagnosticDescriptor descriptor, int column, int line,
			params object[] messageArgs)
		{
			return CreateDiagnosticResult(descriptor, messageArgs, (column, line));
		}

		public static DiagnosticResult CreateFor(this DiagnosticDescriptor descriptor, 
			(int column, int line) location,
			(int column, int line) extraLocation,
			params object[] messageArgs)
		{
			return CreateDiagnosticResult(descriptor, messageArgs, location, extraLocation);
		}

		private static DiagnosticResult CreateDiagnosticResult(DiagnosticDescriptor descriptor,
			object[] messageArgs, 
			params (int column, int line)[] locations)
		{
			return new DiagnosticResult()
			{
				Id = descriptor.Id,
				Severity = descriptor.DefaultSeverity,
				Message = messageArgs == null || messageArgs.Length == 0 
					? descriptor.Title.ToString() 
					: String.Format(descriptor.MessageFormat.ToString(), messageArgs),
				Locations = locations.Select(l => new DiagnosticResultLocation("Test0.cs", l.column, l.line)).ToArray()
			};
		}
	}
}
