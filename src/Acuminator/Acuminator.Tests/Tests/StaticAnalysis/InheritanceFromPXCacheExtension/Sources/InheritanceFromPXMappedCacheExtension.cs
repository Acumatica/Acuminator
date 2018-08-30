using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PX.Data;

namespace PX1009
{
	public class Entity : PXMappedCacheExtension
	{
		public abstract class noteID : IBqlField { }
		public Guid? NoteID { get; set; }
		public abstract class localCD : IBqlField { }
		public string LocalCD { get; set; }
		public abstract class localID : IBqlField { }
		public int? LocalID { get; set; }
		public abstract class entityTypeID : IBqlField { }
		[PXInt]
		public int? EntityTypeID { get; set; }
	}
}