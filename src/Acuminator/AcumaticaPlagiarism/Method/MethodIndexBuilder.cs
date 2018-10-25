using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace AcumaticaPlagiarism.Method
{
    internal static class MethodIndexBuilder
    {
        public static MethodIndex BuildIndex(MethodDeclarationSyntax method, SemanticModel semanticModel)
        {
            SyntaxList<StatementSyntax> statements = method.Body.Statements;
            List<int> statementIndexes = new List<int>();

            foreach (StatementSyntax s in statements)
            {
                IEnumerable<string> words = BuildWords(s);
                IEnumerable<int> wordIndex = GetWordsIndex(words);

                statementIndexes.AddRange(wordIndex);
            }

            ISymbol symbol = semanticModel.GetDeclaredSymbol(method);
            string methodName = symbol.ToDisplayString();
            string location = method.GetLocation().ToString();

            return new MethodIndex(methodName, location, statementIndexes);
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
