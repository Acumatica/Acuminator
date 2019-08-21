using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Syntax
{
    public static class ArgumentSyntaxExtensions
    {
        public static IParameterSymbol DetermineParameter(
            this ArgumentSyntax argument,
            ImmutableArray<IParameterSymbol> parameters,
            bool allowParams = false)
        {
            argument.ThrowOnNull(nameof(argument));

            if (!(argument.Parent is BaseArgumentListSyntax argumentList))
                return null;

            // Handle named argument
            if (argument.NameColon != null && !argument.NameColon.IsMissing)
            {
                string name = argument.NameColon.Name.Identifier.ValueText;
                return parameters.FirstOrDefault(p => p.Name == name);
            }

            // Handle positional argument
            int index = argumentList.Arguments.IndexOf(argument);
            if (index < 0)
                return null;

            if (index < parameters.Length)
                return parameters[index];

            if (allowParams)
            {
                IParameterSymbol lastParameter = parameters.LastOrDefault();
                if (lastParameter == null)
                    return null;

                if (lastParameter.IsParams)
                    return lastParameter;
            }

            return null;
        }
    }
}
