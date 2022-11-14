using System;
using PX.Data;

namespace PX.Objects.HackathonDemo.ExceptionSerialization
{
	[Serializable]
	internal class PXWorkspaceNotExistsException : PXException
	{
		public PXWorkspaceNotExistsException(Guid workspaceId)
			 : base(string.Format("The {0} workspace does not exist or is not available for the current user.", workspaceId))
		{
		}
	}
}
