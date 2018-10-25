using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Text;

namespace AcumaticaPlagiarism.Method
{
    internal class MethodStatementWalker : CSharpSyntaxWalker
    {
        private readonly StringBuilder _currentWord = new StringBuilder();
        private readonly List<string> _words = new List<string>();

        public IEnumerable<string> Words
        {
            get
            {
                FlushWord();

                return _words;
            }
        }

        private void FlushWord()
        {
            if (_currentWord.Length == 0)
                return;

            _words.Add(_currentWord.ToString());
            _currentWord.Clear();
        }

        private void AddSeparator()
        {
            if (_currentWord.Length == 0)
                return;

            _currentWord.Append("_");
        }

        private void RegisterNode(SyntaxNode node)
        {
            AddSeparator();

            _currentWord.Append(node.GetType().Name);
        }

        private void RegisterBlock(string name)
        {
            FlushWord();

            _currentWord.Append(name);

            FlushWord();
        }

        public override void Visit(SyntaxNode node)
        {
            if (node is StatementSyntax statement)
            {
                FlushWord();
            }

            switch (node)
            {
                case ThisExpressionSyntax thisNode:
                    base.Visit(node);
                    break;
                case IfStatementSyntax ifNode:
                    RegisterIfNode(ifNode);
                    break;
                case ForEachStatementSyntax forEachNode:
                    RegisterForEachNode(forEachNode);
                    break;
                case ForStatementSyntax forNode:
                    RegisterForNode(forNode);
                    break;
                case WhileStatementSyntax whileNode:
                    RegisterWhileNode(whileNode);
                    break;
                case DoStatementSyntax doNode:
                    RegisterDoNode(doNode);
                    break;
                case SwitchStatementSyntax switchNode:
                    RegisterSwitchNode(switchNode);
                    break;
                case TryStatementSyntax tryNode:
                    RegisterTryNode(tryNode);
                    break;
                default:
                    RegisterNode(node);
                    base.Visit(node);
                    break;
            }
        }

        private void RegisterTryNode(TryStatementSyntax tryNode)
        {
            RegisterBlock("Try");

            if (tryNode.Block != null)
            {
                foreach (var statement in tryNode.Block.Statements)
                {
                    base.Visit(statement);
                    FlushWord();
                }
            }

            foreach (var catchClause in tryNode.Catches)
            {
                RegisterBlock("Catch");

                if (catchClause.Declaration != null)
                {
                    base.Visit(catchClause.Declaration);
                    FlushWord();
                }

                if (catchClause.Block != null)
                {
                    foreach (var statement in catchClause.Block.Statements)
                    {
                        base.Visit(statement);
                        FlushWord();
                    }
                }

                RegisterBlock("CatchEnd");
            }

            if (tryNode.Finally != null)
            {
                RegisterBlock("Finally");

                if (tryNode.Finally.Block != null)
                {
                    foreach (var statement in tryNode.Finally.Block.Statements)
                    {
                        base.Visit(statement);
                        FlushWord();
                    }
                }

                RegisterBlock("FinallyEnd");
            }

            RegisterBlock("TryEnd");
        }

        private void RegisterForNode(ForStatementSyntax forNode)
        {
            RegisterBlock("For");

            if (forNode.Declaration != null)
            {
                base.Visit(forNode.Declaration);
                FlushWord();
            }

            if (forNode.Condition != null)
            {
                base.Visit(forNode.Condition);
                FlushWord();
            }

            if (forNode.Incrementors.Count > 0)
            {
                foreach (ExpressionSyntax e in forNode.Incrementors)
                {
                    base.Visit(e);
                }

                FlushWord();
            }

            if (forNode.Statement != null)
            {
                base.Visit(forNode.Statement);
            }

            RegisterBlock("ForEnd");
        }

        private void RegisterWhileNode(WhileStatementSyntax whileNode)
        {
            RegisterBlock("While");

            base.Visit(whileNode.Condition);
            FlushWord();

            if (whileNode.Statement != null)
            {
                base.Visit(whileNode.Statement);
            }

            RegisterBlock("WhileEnd");
        }

        private void RegisterDoNode(DoStatementSyntax doNode)
        {
            RegisterBlock("Do");

            if (doNode.Statement != null)
            {
                base.Visit(doNode.Statement);
            }

            base.Visit(doNode.Condition);
            FlushWord();

            RegisterBlock("DoEnd");
        }

        private void RegisterSwitchNode(SwitchStatementSyntax switchNode)
        {
            RegisterBlock("Switch");

            foreach (SwitchSectionSyntax section in switchNode.Sections)
            {
                foreach (SwitchLabelSyntax l in section.Labels)
                {
                    base.Visit(l);
                    FlushWord();
                }

                foreach (StatementSyntax s in section.Statements)
                {
                    base.Visit(s);
                }
            }

            RegisterBlock("SwitchEnd");
        }

        private void RegisterForEachNode(ForEachStatementSyntax forEachNode)
        {
            RegisterBlock("Foreach");

            base.Visit(forEachNode.Type);
            FlushWord();

            base.Visit(forEachNode.Expression);
            FlushWord();

            if (forEachNode.Statement != null)
            {
                base.Visit(forEachNode.Statement);
            }

            RegisterBlock("ForeachEnd");
        }

        private void RegisterIfNode(IfStatementSyntax ifNode)
        {
            RegisterBlock("If");

            base.Visit(ifNode.Condition);
            FlushWord();

            if (ifNode.Statement != null)
            {
                base.Visit(ifNode.Statement);
            }

            if (ifNode.Else != null)
            {
                RegisterBlock("Else");

                base.Visit(ifNode.Else.Statement);

                RegisterBlock("ElseEnd");
            }

            RegisterBlock("IfEnd");
        }
    }
}
