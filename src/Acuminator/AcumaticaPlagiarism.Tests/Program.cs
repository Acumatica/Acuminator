using System;
using System.Collections.Generic;

namespace AcumaticaPlagiarism.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            string referenceSolutionPath = @"C:\repos\ReferenceSolution\ReferenceSolution.sln";
            string solutionToAnalyzePath = @"C:\repos\SolutionToAnalyze\SolutionToAnalyze.sln";
            PlagiarismScanner scanner = new PlagiarismScanner(referenceSolutionPath, solutionToAnalyzePath);
            IEnumerable<PlagiarismInfo> scanResults = scanner.Scan();

            foreach (PlagiarismInfo info in scanResults)
            {
                Console.WriteLine(info.Type);
                Console.WriteLine(info.Similarity);
                Console.WriteLine(info.Reference.Name);
                Console.WriteLine(info.Source.Name);
                Console.WriteLine();
            }
        }
    }
}
