using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.NoIsActiveMethodForExtension.Sources
{
	public class Ext<TGraph> : PXGraphExtension<TGraph>
	where TGraph : PXGraph, new()
	{

	}
}
