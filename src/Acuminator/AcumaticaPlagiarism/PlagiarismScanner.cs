using AcumaticaPlagiarism.Method;
using AcumaticaPlagiarism.Solution;
using Microsoft.Build.Locator;
using System;
using System.Collections.Generic;

namespace AcumaticaPlagiarism
{
    public class PlagiarismScanner
    {
        public const double SimilarityThresholdDefault = 0.8;

        private readonly List<PlagiarismInfo> _scanResults = new List<PlagiarismInfo>();
        private readonly double _similarityThreshold;
        private readonly string _referenceSolutionPath;
        private readonly string _sourceSolutionPath;
        private readonly Func<MethodIndex, MethodIndex, double> _methodSimilarityStrategy;

        public PlagiarismScanner(string referenceSolutionPath, string sourceSolutionPath,
                                 double similarityThreshold = SimilarityThresholdDefault,
                                 Func<MethodIndex, MethodIndex, double> methodSimilarityStrategy = null)
        {
            _referenceSolutionPath = referenceSolutionPath;
            _sourceSolutionPath = sourceSolutionPath;
            _similarityThreshold = similarityThreshold;

            if (methodSimilarityStrategy == null)
            {
                _methodSimilarityStrategy = DefaultSimilarityStrategies.MethodSimilarityStrategy;
            }
        }

        public IEnumerable<PlagiarismInfo> Scan(bool callFromVS)
        {
            if (!callFromVS)
            {
                MSBuildLocator.RegisterDefaults();
            }

            ScanMethods();

            return _scanResults;
        }

        private void ScanMethods()
        {
            SolutionIndex referenceIndex = SolutionIndexBuilder.BuildIndex(_referenceSolutionPath);
            SolutionIndex solutionIndex = SolutionIndexBuilder.BuildIndex(_sourceSolutionPath);

            foreach (MethodIndex r in referenceIndex.MethodIndices)
            {
                foreach (MethodIndex s in solutionIndex.MethodIndices)
                {
                    double similarity = _methodSimilarityStrategy(r, s);
                    bool isPlagiat = similarity >= _similarityThreshold;

                    if (!isPlagiat)
                        continue;

                    _scanResults.Add(new PlagiarismInfo(PlagiarismType.Method, similarity, r, s));
                }
            }
        }
    }
}
