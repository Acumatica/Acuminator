using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class BranchAttribute : BranchBaseAttribute
	{
		public BranchAttribute(Type sourceType)
			: base(sourceType, addDefaultAttribute: true, onlyActive: true)
		{
			throw new NotImplementedException();
		}

		protected BranchAttribute(Type sourceType, Type searchType)
			: base(sourceType, searchType, onlyActive: true)
		{
			throw new NotImplementedException();
		}

		public BranchAttribute() : base()
		{
			throw new NotImplementedException();
		}

		public BranchAttribute(bool onlyActive) : base(onlyActive)
		{
			throw new NotImplementedException();
		}
	}
}
