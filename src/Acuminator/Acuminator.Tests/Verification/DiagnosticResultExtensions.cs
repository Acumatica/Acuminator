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
		public static DiagnosticResult CreateFor(this DiagnosticDescriptor descriptor, int line, int column,
			params object[] messageArgs)
		{
			return CreateDiagnosticResult(descriptor, messageArgs, (line, column));
		}

		public static DiagnosticResult CreateFor(this DiagnosticDescriptor descriptor, 
			(int line, int column) location,
			(int line, int column) extraLocation,
			params object[] messageArgs)
		{
			return CreateDiagnosticResult(descriptor, messageArgs, location, extraLocation);
		}

		public static DiagnosticResult CreateFor(this DiagnosticDescriptor descriptor,
			//(int line, int column) location,
			IEnumerable<(int line, int column)> locations)
		{
			return CreateDiagnosticResult(descriptor,null, locations);
		}

		private static DiagnosticResult CreateDiagnosticResult(DiagnosticDescriptor descriptor,
			object[] messageArgs, 
			params (int line, int column)[] locations)
		{
			return new DiagnosticResult()
			{
				Id = descriptor.Id,
				Severity = descriptor.DefaultSeverity,
				Message = messageArgs == null || messageArgs.Length == 0 
					? descriptor.Title.ToString() 
					: String.Format(descriptor.MessageFormat.ToString(), messageArgs),
				Locations = locations.Select(l => new DiagnosticResultLocation("Test0.cs", l.line, l.column)).ToArray()
			};
		}

		private static DiagnosticResult CreateDiagnosticResult(DiagnosticDescriptor descriptor,
			object[] messageArgs,
			IEnumerable<(int line, int column)> locations)
		{
			return new DiagnosticResult()
			{
				Id = descriptor.Id,
				Severity = descriptor.DefaultSeverity,
				Message = messageArgs == null || messageArgs.Length == 0
					? descriptor.Title.ToString()
					: String.Format(descriptor.MessageFormat.ToString(), messageArgs),
				Locations = locations.Select(l => new DiagnosticResultLocation("Test0.cs", l.line, l.column)).ToArray()
			};
		}
	}
}
