using System;

namespace MyLang.CodeAnalysis
{
    public class Evaluator
    {
        private readonly ExpressionSyntax _root;

        public Evaluator(ExpressionSyntax root)
        {
            _root = root;
        }

        public int Evaluate() => EvaluateExpression(_root);

        private int EvaluateExpression(ExpressionSyntax node)
        {
            // <NumberExpression>
            if (node is LiteralExpressionSyntax n)
<<<<<<< HEAD
                return (int) n.LiteralToken.Value;
=======
                return (int) n.NumberToken.Value;
>>>>>>> 87003e657047ba372f2fe7f4ee564279b23b3adc

            // <BinaryExpression>
            if (node is BinaryExpressionSyntax b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);

                if (b.OperatorToken.Kind == SyntaxKind.PlusToken)
                    return left + right;
                if (b.OperatorToken.Kind == SyntaxKind.MinusToken)
                    return left - right;
                if (b.OperatorToken.Kind == SyntaxKind.StarToken)
                    return left * right;
                if (b.OperatorToken.Kind == SyntaxKind.SlashToken)
                    return left / right;
                throw new Exception($"Unexpected binary operator found. {b.OperatorToken.Kind}");
            }

            if (node is ParenthesizedExpressionSyntax p)
                return EvaluateExpression(p.Expression);

            throw new Exception($"Unexpected node found. {node.Kind}");
        }
    }
}