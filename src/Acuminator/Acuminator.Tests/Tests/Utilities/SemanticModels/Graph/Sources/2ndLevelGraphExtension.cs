using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Graph.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active

	public class DerivedSecondLevelGraphExtension : SecondLevelGraphExtension
	{
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SecondLevelGraphExtension : PXGraphExtension<BaseExtension, MyGraph>
	{
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class BaseExtension : PXGraphExtension<MyGraph>
	{
		
	}

	public class MyGraph : PXGraph<MyGraph>
	{
		
	}
}
