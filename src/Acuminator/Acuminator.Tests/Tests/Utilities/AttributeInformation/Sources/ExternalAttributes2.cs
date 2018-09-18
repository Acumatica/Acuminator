using System;
using PX.Data;
namespace PX.Objects.HackathonDemo
{
    public class AcctSubUnboundAttribute : PXAggregateAttribute
    {
        public bool IsDBField { get; set; } = false;
    }
}