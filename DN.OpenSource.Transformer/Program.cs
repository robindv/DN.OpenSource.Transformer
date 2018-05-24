using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DN.OpenSource.Transformer
{
    internal class Program
    {
        private static void Main()
        {
            var compilation = CreateCompilation();

            foreach (SyntaxTree sourceTree in compilation.SyntaxTrees)
            {
                var model = compilation.GetSemanticModel(sourceTree);

                var rewriter = new ClosedSourceRewriter(model);

                var newSource = rewriter.Visit(sourceTree.GetRoot());

                if (newSource != sourceTree.GetRoot())
                {
                    File.WriteAllText($"{sourceTree.FilePath}", newSource.ToFullString());
                }
            }
        }

        private static Compilation CreateCompilation()
        {
            List<SyntaxTree> sourceTrees = new List<SyntaxTree>();

            foreach (var file in Directory.EnumerateFiles(@"C:\DataNose", "*.cs", SearchOption.AllDirectories))
            {
                Console.WriteLine(file);
                var programText = File.ReadAllText(file);
                sourceTrees.Add(CSharpSyntaxTree.ParseText(programText).WithFilePath(file));
            }

            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            MetadataReference codeAnalysis = MetadataReference.CreateFromFile(typeof(SyntaxTree).Assembly.Location);
            MetadataReference csharpCodeAnalysis = MetadataReference.CreateFromFile(typeof(CSharpSyntaxTree).Assembly.Location);

            MetadataReference[] references = { mscorlib, codeAnalysis, csharpCodeAnalysis };

            return CSharpCompilation.Create("OpenSourceTransformer", sourceTrees, references, new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        }
    }
}