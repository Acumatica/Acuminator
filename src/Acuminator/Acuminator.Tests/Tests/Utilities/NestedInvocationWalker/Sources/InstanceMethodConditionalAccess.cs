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
			bar?.DoAwesomeThing();
		}
	}

	public class Bar
	{
		public void DoAwesomeThing()
		{
			throw new Exception();
		}
	}
}
