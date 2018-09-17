using System;
using PX.Data;
namespace PX.Objects.HackathonDemo
{
    public class AcctSub1Attribute : PXAggregateAttribute
    {
        public bool IsDBField { get; set; } = true;
    }
}