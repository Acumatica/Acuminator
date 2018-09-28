using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Analyzers.StaticAnalysis.ViewDeclarationOrder
{
	/// <summary>
	/// An analyzer for the order of view declaration in graph/graph extension.
	/// </summary>
	public partial class ViewDeclarationOrderAnalyzer : IPXGraphAnalyzer
	{
		private class AnalysisPassInfo
		{
			private DiagnosticDescriptor _diagnosticDescriptor;

			public bool HasDiagnostic => _diagnosticDescriptor != null;

			public DiagnosticDescriptor DiagnosticDescriptor
			{
				get { return _diagnosticDescriptor; }
				set
				{
					if (!HasDiagnostic)
					{
						_diagnosticDescriptor = value;
					}
				}
			}

			public ITypeSymbol ViewDacType { get; }		

			public List<DataViewInfo> VisitedViews { get; }

			public AnalysisPassInfo(ITypeSymbol viewDacType, DataViewInfo firstVisitedViewInfo)
			{
				ViewDacType = viewDacType;
				VisitedViews = new List<DataViewInfo> { firstVisitedViewInfo };
			}
		}
	}
}