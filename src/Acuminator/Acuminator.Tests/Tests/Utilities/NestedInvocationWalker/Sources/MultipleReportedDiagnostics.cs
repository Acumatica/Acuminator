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
			bar.DoAwesomeThing();
		}
	}

	public class Bar
	{
		public void DoAwesomeThing(string str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));
			if (str.Length < 5)
				throw new ArgumentException();

			throw new NotImplementedException();
		}
	}
}
