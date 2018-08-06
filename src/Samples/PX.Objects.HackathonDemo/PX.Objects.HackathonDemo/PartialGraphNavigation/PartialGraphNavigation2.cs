using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.HackathonDemo
{
	public partial class SimpleOrdersMaint : PXGraph<LEPMaint>
	{
		public IEnumerable currentOrder()
		{
			return Enumerable.Empty<SOOrder>();
		}
	}
}