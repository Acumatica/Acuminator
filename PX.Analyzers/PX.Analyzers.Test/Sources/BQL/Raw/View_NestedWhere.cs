using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class FooEntry : PXGraph<FooEntry>
	{
		public PXSelect<Table, Where2<Where<Table.field2, Greater<Table.field1>, Or<Table.field3, Between<Table.field1, Table.field2>>>, And<Where<Table.field3, IsNull, And<Table.field1, Equal<Table.field2>>>>>> Foos;
	}
}
