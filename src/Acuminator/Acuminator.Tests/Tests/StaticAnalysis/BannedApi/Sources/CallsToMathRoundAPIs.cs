using System;
using System.Linq;
using System.Reflection;

using PX.Objects.AP;

namespace PX.Objects.HackathonDemo
{
	public class Service
	{
		public decimal Convert(decimal amount, decimal rate, int? precision = null, MidpointRounding? midpointRounding = null)
		{
			if (precision.HasValue)
			{
				return midpointRounding.HasValue
					? Math.Round(amount * rate, precision.Value, midpointRounding.Value)
					: Math.Round(amount * rate, precision.Value);
			}
			else
			{
				if (midpointRounding.HasValue)
				{
					return Math.Round(amount * rate, midpointRounding.Value);
				}
				else
					return Math.Round(amount * rate);
			}
		}

		public double Convert(double amount, double rate, int? precision = null, MidpointRounding? midpointRounding = null)
		{
			if (precision.HasValue)
			{
				return midpointRounding.HasValue
					? Math.Round(amount * rate, precision.Value, midpointRounding.Value)
					: Math.Round(amount * rate, precision.Value);
			}
			else
			{
				if (midpointRounding.HasValue)
				{
					return Math.Round(amount * rate, midpointRounding.Value);
				}
				else
					return Math.Round(amount * rate);
			}
		}
	}
}
