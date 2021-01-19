using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
    public class SOOrdersInqExt : PXGraphExtension<SOOrdersInq> 
    {
        public SOOrder GetOrder(PXGraph graph, string orderNbr)
        {
            var order = PXSelect<SOOrder, Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>
                .Select(new PXGraph(), orderNbr);

            return order;
        }
    }
}
