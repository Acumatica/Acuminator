using System;
using PX.Data;
namespace PX.Objects.HackathonDemo
{
    public class AcctSubBoundAttribute : PXAggregateAttribute
    {
        public bool IsDBField { get; set; } = true;
    }
}