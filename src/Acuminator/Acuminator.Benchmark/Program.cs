using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Running;

namespace Acuminator.Benchmark
{
	public class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<RoslynCodeAnalysis>();
		}
	}
}
