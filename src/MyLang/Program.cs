using System;
using System.Linq;
using MyLang.CodeAnalysis;

namespace MyLang
{
    internal static class Program
    {
        private static void Main()
        {
            var showTree = false;

            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    return;

                switch (line)
                {
                    case "#showTree":
                        showTree = !showTree;
                        Console.WriteLine(showTree
                                              ? "SyntaxTree visualization enabled"
                                              : "SyntaxTree visualization disabled");
                        continue;
                    case "#cls":
                        Console.Clear();
                        continue;
                }

                var syntaxTree = SyntaxTree.Parse(line);

                if (showTree)
                    VisualizeTree(syntaxTree.Root);


                // If there are no diagnostics, we can evaluate the expression.
                if (!syntaxTree.Diagnostics.Any())
                {
                    var eval = new Evaluator(syntaxTree.Root);
                    var result = eval.Evaluate();
                    Console.WriteLine(result);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    
                    foreach (var diagnostic in syntaxTree.Diagnostics)
                        Console.WriteLine(diagnostic);

                    Console.ResetColor();
                }
            }
        }


        private static void VisualizeTree(SyntaxNode node, string indent = "", bool isLast = true)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;

            var marker = isLast ? "└──" : "├──";

            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Kind);

            if (node is SyntaxToken t && t.Value != null)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }

            Console.WriteLine();


            indent += isLast ? "   " : "|   ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
                VisualizeTree(child, indent, child == lastChild);

            Console.ResetColor();
        }
    }
}
