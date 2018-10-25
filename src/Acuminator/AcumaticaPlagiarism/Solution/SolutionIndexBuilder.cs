using AcumaticaPlagiarism.Method;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Generic;
using System.Linq;

namespace AcumaticaPlagiarism.Solution
{
    internal static class SolutionIndexBuilder
    {
        public static SolutionIndex BuildIndex(string solutionPath)
        {
            IList<MethodIndex> indices = new List<MethodIndex>();
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();

            workspace.WorkspaceFailed += Workspace_WorkspaceFailed;

            Microsoft.CodeAnalysis.Solution solution = workspace.OpenSolutionAsync(solutionPath).Result;

            foreach (Project p in solution.Projects.Take(1))
            {
                Compilation compilation = p.GetCompilationAsync().Result;

                foreach (SyntaxTree t in compilation.SyntaxTrees)
                {
                    SemanticModel semanticModel = compilation.GetSemanticModel(t);
                    IEnumerable<MethodDeclarationSyntax> methodDeclarations = t.GetRoot()
                                                                               .DescendantNodesAndSelf()
                                                                               .OfType<MethodDeclarationSyntax>();

                    foreach (MethodDeclarationSyntax method in methodDeclarations)
                    {
                        MethodIndex index = MethodIndexBuilder.BuildIndex(method, semanticModel);

                        if (index != null)
                        {
                            indices.Add(index);
                        }
                    }
                }
            }

            return new SolutionIndex(solutionPath, indices);
        }

        //This is needed for debug
        private static void Workspace_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
        }
    }
}
