﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PX.Data;

namespace PX.Objects
{
	/// <exclude/>
	public class ExcludedWithNested : IBqlTable  //Should not show
	{
		public class NestedDac : IBqlTable  //Should not show
		{ } 
	}

	public class PublicDac : IBqlTable  //Should show
	{
		/// <exclude/>
		public class ExcludedNested : IBqlTable
		{
			public class NestedNested : IBqlTable  //Should not show
			{ }
		}

		public class Nested : IBqlTable { } //Should  show
	}

	public class PublicGraph : PXGraph<PublicGraph>  //Should not show
	{
		/// <exclude/>
		public class ExcludedNested : IBqlTable
		{
			public class NestedNested : IBqlTable  //Should not show
			{ }
		}

		public class Nested : IBqlTable { } //Should  show
	}
}
