using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace AcumaticaPlagiarism.Method
{
    internal static class MethodIndexBuilder
    {
        private const int _defaultMinMethodSize = 100;

        public static MethodIndex BuildIndex(MethodDeclarationSyntax method, SemanticModel semanticModel)
        {
            if (method.Body == null)
            {
                return null;
            }

            var statements = method.Body.Statements;
            var statementIndexes = new List<int>();

            foreach (var s in statements)
            {
                var words = BuildWords(s);
                var wordIndex = GetWordsIndex(words);

                statementIndexes.AddRange(wordIndex);
            }

            if (statementIndexes.Count < _defaultMinMethodSize)
            {
                return null;
            }

            var symbol = semanticModel.GetDeclaredSymbol(method);
            var methodName = symbol.ToDisplayString();
            var location = method.GetLocation().GetMappedLineSpan();

            if (methodName == @"PX.Objects.IN.KitAssemblyEntry.ExecuteUpdate(string, IDictionary, IDictionary, params object[])"
                /*@"PX.Objects.IN.KitAssemblyEntry.ExecuteUpdate(string, IDictionary, IDictionary, params object[])"*/)
            {
            }

            return new MethodIndex(methodName, location, statementIndexes);
        }

        private static IEnumerable<int> GetWordsIndex(IEnumerable<string> words)
        {
            return words.Select(w => w.GetHashCode());
        }

        private static IEnumerable<string> BuildWords(StatementSyntax statement)
        {
            var walker = new MethodStatementWalker();

            walker.Visit(statement);

            return walker.Words;
        }
    }
}
