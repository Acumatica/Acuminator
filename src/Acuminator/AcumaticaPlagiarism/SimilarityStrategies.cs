using AcumaticaPlagiarism.Method;
using System;

namespace AcumaticaPlagiarism
{
    public static class DefaultSimilarityStrategies
    {
        public static double MethodSimilarityStrategy(MethodIndex reference, MethodIndex source)
        {
            if (reference == null)
                throw new ArgumentNullException(nameof(reference));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            double distance = MethodIndex.CalculateDistance(reference, source);
            int maxStatementsLength = Math.Max(reference.Statements.Length, source.Statements.Length);
            double similarity = 1D - distance / maxStatementsLength;

            return similarity;
        }
    }
}
