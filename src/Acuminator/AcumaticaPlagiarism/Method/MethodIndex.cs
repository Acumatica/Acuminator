using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AcumaticaPlagiarism.Method
{
    public class MethodIndex : Index
    {
        public int[] Statements { get; }

        public MethodIndex(string name, FileLinePositionSpan location, IEnumerable<int> statements)
            : base(name, location)
        {
            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            Statements = statements.ToArray();
        }

        public static int CalculateDistance(MethodIndex indexA, MethodIndex indexB)
        {
            if (indexA == null)
            {
                throw new ArgumentNullException(nameof(indexA));
            }

            if (indexB == null)
            {
                throw new ArgumentNullException(nameof(indexB));
            }

            var a = indexA.Statements;
            var b = indexB.Statements;

            var daSize = a.Length + 1;
            var dbSize = b.Length + 1;
            var d = new int[daSize, dbSize];

            for (var i = 0; i < daSize; i++)
            {
                d[i, 0] = i;
            }

            for (var j = 0; j < dbSize; j++)
            {
                d[0, j] = j;
            }

            for (var i = 0; i < a.Length; i++)
            {
                for (var j = 0; j < b.Length; j++)
                {
                    var cost = a[i] == b[j] ? 0 : 1;
                    var deletion = d[i, j + 1] + 1;
                    var insertion = d[i + 1, j] + 1;
                    var substitution = d[i, j] + cost;

                    var operation = Math.Min(deletion, insertion);
                    operation = Math.Min(operation, substitution);

                    d[i + 1, j + 1] = operation;

                    if (i > 0 && j > 0 && a[i] == b[j - 1] && a[i - 1] == b[j])
                    {
                        var transposition = d[i - 1, j - 1] + cost;
                        operation = Math.Min(operation, transposition);

                        d[i + 1, j + 1] = operation;
                    }
                }
            }

            return d[a.Length, b.Length];
        }
    }
}
