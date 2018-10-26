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

            SyntaxList<StatementSyntax> statements = method.Body.Statements;
            List<int> statementIndexes = new List<int>();

            foreach (StatementSyntax s in statements)
            {
                IEnumerable<string> words = BuildWords(s);
                IEnumerable<int> wordIndex = GetWordsIndex(words);

                statementIndexes.AddRange(wordIndex);
            }

            if (statementIndexes.Count < _defaultMinMethodSize)
            {
                return null;
            }

            ISymbol symbol = semanticModel.GetDeclaredSymbol(method);
            string methodName = symbol.Name;
            var path = method.GetLocation().GetMappedLineSpan().Path;
            var line = method.GetLocation().GetMappedLineSpan().StartLinePosition.Line;
            var character = method.GetLocation().GetMappedLineSpan().StartLinePosition.Character;

            return new MethodIndex(methodName, path, line, character, statementIndexes);
        }

        private static IEnumerable<int> GetWordsIndex(IEnumerable<string> words)
        {
            return words.Select(w => w.GetHashCode());
        }

        private static IEnumerable<string> BuildWords(StatementSyntax statement)
        {
            MethodStatementWalker walker = new MethodStatementWalker();

            walker.Visit(statement);

            return walker.Words;
        }
    }
}
