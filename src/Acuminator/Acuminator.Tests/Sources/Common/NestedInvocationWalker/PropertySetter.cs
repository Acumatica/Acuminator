using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator
{
	public class Foo
	{
		public void DoSomething()
		{
			var bar = new Bar();
			bar.Count = 5;
		}
	}

	public class Bar
	{
		public int Count
		{
			set
			{
				throw new NotSupportedException();
			}
		}
	}
}
