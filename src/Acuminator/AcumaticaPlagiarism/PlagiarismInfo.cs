namespace AcumaticaPlagiarism
{
    public class PlagiarismInfo
    {
        public PlagiarismType Type { get; private set; }
        public double Similarity { get; private set; }
        public Index Reference { get; private set; }
        public Index Source { get; private set; }

        internal PlagiarismInfo(PlagiarismType type, double similarity, Index reference, Index source)
        {
            Type = type;
            Similarity = similarity;
            Reference = reference;
            Source = source;
        }
    }
}
