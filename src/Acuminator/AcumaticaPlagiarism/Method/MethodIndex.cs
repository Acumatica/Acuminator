using System;
using System.Collections.Generic;
using System.Linq;

namespace AcumaticaPlagiarism.Method
{
    public class MethodIndex : Index
    {
        public int[] Statements { get; private set; }

        public MethodIndex(string name, string path, int line, int character, IEnumerable<int> statements)
            : base(name, path, line, character)
        {
            if (statements == null)
                throw new ArgumentNullException(nameof(statements));

            Statements = statements.ToArray();
        }

        public static int CalculateDistance(MethodIndex indexA, MethodIndex indexB)
        {
            if (indexA == null)
                throw new ArgumentNullException(nameof(indexA));

            if (indexB == null)
                throw new ArgumentNullException(nameof(indexB));

            int[] a = indexA.Statements;
            int[] b = indexB.Statements;

            int daSize = a.Length + 1;
            int dbSize = b.Length + 1;
            int[,] d = new int[daSize, dbSize];

            for (int i = 0; i < daSize; i++)
            {
                d[i, 0] = i;
            }

            for (int j = 0; j < dbSize; j++)
            {
                d[0, j] = j;
            }

            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < b.Length; j++)
                {
                    int cost = a[i] == b[j] ? 0 : 1;
                    int deletion = d[i, j + 1] + 1;
                    int insertion = d[i + 1, j] + 1;
                    int substitution = d[i, j] + cost;

                    int operation = Math.Min(deletion, insertion);
                    operation = Math.Min(operation, substitution);

                    d[i + 1, j + 1] = operation;

                    if (i > 0 && j > 0 && a[i] == b[j - 1] && a[i - 1] == b[j])
                    {
                        int transposition = d[i - 1, j - 1] + cost;
                        operation = Math.Min(operation, transposition);

                        d[i + 1, j + 1] = operation;
                    }
                }
            }

            return d[a.Length, b.Length];
        }
    }
}
