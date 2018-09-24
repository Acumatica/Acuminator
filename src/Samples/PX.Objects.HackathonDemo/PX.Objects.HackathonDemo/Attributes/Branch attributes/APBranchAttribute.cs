using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	
	/// <summary>
	/// Attribute with invalid aggregator declaration.
	/// </summary>
	[PXDBString]
	public class APBranchAttribute : BranchBaseAttribute
	{
		public APBranchAttribute(Type sourceType)
			: base(sourceType, addDefaultAttribute: true, onlyActive: true)
		{
			throw new NotImplementedException();
		}

		protected APBranchAttribute(Type sourceType, Type searchType)
			: base(sourceType, searchType, onlyActive: true)
		{
			throw new NotImplementedException();
		}

		public APBranchAttribute() : base()
		{
			throw new NotImplementedException();
		}

		public APBranchAttribute(bool onlyActive) : base(onlyActive)
		{
			throw new NotImplementedException();
		}
	}
}
