using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class ProcessingDelegateInfo : DataViewDelegateInfo
    {
        public DataViewDelegateInfo ProcessItem { get; }
        public DataViewDelegateInfo ProcessList { get; }
        public DataViewDelegateInfo Parameters { get; }
        public DataViewDelegateInfo FinallyProcess { get; }

        public ProcessingDelegateInfo(MethodDeclarationSyntax node, IMethodSymbol symbol)
            : base(node, symbol)
        {
        }
    }
}
