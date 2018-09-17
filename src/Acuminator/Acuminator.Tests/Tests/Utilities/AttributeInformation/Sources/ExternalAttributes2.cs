using System;
using PX.Data;
namespace PX.Objects.HackathonDemo
{
    public class AcctSub2Attribute : PXAggregateAttribute
    {
        public bool IsDBField { get; set; } = true;
    }
}