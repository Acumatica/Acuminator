using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.ExceptionSerialization.Sources
{
	[Serializable]
	public class PXWorkspaceNotExistsException : PXException      // Show PX1063 but not PX1064
	{
		public PXWorkspaceNotExistsException(Guid workspaceId)
			 : base(string.Format("The {0} workspace does not exist or is not available for the current user.", workspaceId))
		{
		}

		private bool Foo() => true;
	}
}
