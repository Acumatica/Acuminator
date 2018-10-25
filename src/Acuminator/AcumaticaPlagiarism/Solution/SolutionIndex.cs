using AcumaticaPlagiarism.Method;
using System.Collections.Generic;

namespace AcumaticaPlagiarism.Solution
{
    internal class SolutionIndex
    {
        public string Path { get; private set; }
        public IEnumerable<MethodIndex> MethodIndices { get; private set; }

        public SolutionIndex(string path, IEnumerable<MethodIndex> methodIndices)
        {
            Path = path;
            MethodIndices = methodIndices;
        }
    }
}
