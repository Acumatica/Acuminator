#nullable enable

using System;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	/// <summary>
	/// Enum with kinds of parameters that can be passed into methods and then captured in delegate closures
	/// </summary>
	internal enum PassedParameterKind
	{
		Graph,
		PXAdapter
	}
}