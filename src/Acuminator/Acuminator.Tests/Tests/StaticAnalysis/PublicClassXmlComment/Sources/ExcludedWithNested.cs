using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects
{
	/// <exclude/>
	public class ExcludedWithNested
	{
		public class Nested { } //Should not show

		public interface INested { }  //Should not show

		/// <summary>
		///
		/// </summary>
		public struct NestedStruct { }  //Should not show

		public enum NestedEnum { }  //Should not show

		///
		public delegate void NestedAction(); //Should not show
	}

	public class Public //Should show
	{
		/// <exclude/>
		public class ExcludedNested 
		{
			public struct Struct { } //Should not show
		}

		public class Nested { } //Should  show

		public interface INested { }  //Should  show

		/// <summary>
		///
		/// </summary>
		public struct NestedStruct { }  //Should  show

		///
		public enum NestedEnum { }  //Should  show

		public delegate void NestedAction(); //Should  show
	}
}
