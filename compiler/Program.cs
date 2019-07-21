using System;
using System.Collections.Generic;
using System.Linq;

namespace compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var showTree = false;
            var color = Console.ForegroundColor;
            
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
                        Console.WriteLine(showTree ? "SyntaxTree visualization enabled" : "SyntaxTree visualization disabled");
                        continue;
                    case "#cls":
                        Console.Clear();
                        continue;
                }

                var syntaxTree = SyntaxTree.Parse(line);

                if (showTree)
                    PrintTree(syntaxTree.Root);
                
                
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
                    
                    Console.ForegroundColor = color;
                }
            }
        }
 

        static void PrintTree(SyntaxNode node, string indent = "", bool isLast = true)
        {
            // ├──
            // └──
            // │   └──

            var color = Console.ForegroundColor;
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


            indent += isLast ? "    " : "|   ";

            var lastChild = node.GetChildren().LastOrDefault();
            
            foreach (var child in node.GetChildren())
                PrintTree(child, indent, child == lastChild);
            
            Console.ForegroundColor = color;
        }
    }


    public class Parser
    {
        private SyntaxToken[] _tokens;
        private List<string> _diagnostics  = new List<string>();
        private int _position;

        public List<string> Diagnostics => _diagnostics;

        public Parser(string text)
        {
            var tokens = new List<SyntaxToken>();
            
            var lexer = new Lexer(text);
            
            SyntaxToken token;
            do
            {
                token = lexer.NextToken();
                
                if (token.Kind != SyntaxKind.WhiteSpaceToken
                    && token.Kind != SyntaxKind.BadToken)
                
                    tokens.Add(token);

            } while (token.Kind != SyntaxKind.EndOfFileToken);

            _tokens = tokens.ToArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }


        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            
            if (index >= _tokens.Length)
                return _tokens[^1];
            
            return _tokens[index];
        }

        private SyntaxToken Current => Peek(0);

        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }

        private SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind) return NextToken();
            
            _diagnostics.Add($"ERROR: Unexpected token <{Current.Kind}>, expected <{kind}>");
            return new SyntaxToken(kind, Current.Position, null, null);
        }
        
        public SyntaxTree Parse()
        {
            var expression = ParseTerm();
            var endOfFileToken = Match(SyntaxKind.EndOfFileToken);
            
            return new SyntaxTree(_diagnostics, expression, endOfFileToken);
        }
        
        private ExpressionSyntax ParseExpression() => ParseTerm();
        
        private ExpressionSyntax ParseTerm()
        {
            var left = ParseFactor();

            while (Current.Kind == SyntaxKind.PlusToken || 
                   Current.Kind == SyntaxKind.MinusToken)
            {
                var operatorToken = NextToken();
                var right = ParseFactor();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }
            
            return left;
        }
        
        private ExpressionSyntax ParseFactor()
        {
            var left = ParsePrimaryExpression();

            while (Current.Kind == SyntaxKind.StarToken || 
                   Current.Kind == SyntaxKind.SlashToken)
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }
            
            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            if (Current.Kind == SyntaxKind.OpenParenthesisToken)
            {
                var left = NextToken();
                var expression = ParseExpression();
                var right = Match(SyntaxKind.CloseParenthesisToken);
                
                return new ParenthesizedExpressionSyntax(left, expression, right);
            }
            
            var numberToken = Match(SyntaxKind.NumberToken);
            return new NumberExpressionSyntax(numberToken);
        }
    }
    
    
    public class Lexer
    {
        private readonly string _text;
        private int _position;
        private List<string> _diagnostics = new List<string>();

        public List<string> Diagnostics => _diagnostics;

        public Lexer(string text)
        {
            _text = text;
        }

        private char Current => _position >= _text.Length ? '\0' : _text[_position];

        private void Next() => _position++;
        
        public SyntaxToken NextToken()
        {
            // EOF
            // <numbers>
            // + - * / ( )
            // <whitespace>
            
            
            // EOF
            if (_position >= _text.Length)
            {
                return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0", null);
            }
            
            // <numbers>
            if (char.IsDigit(Current))
            {
                var start = _position;

                while (char.IsDigit(Current))
                    Next();

                var length = _position - start;

                var text = _text.Substring(start, length);

                if(!int.TryParse(text, out var value))
                    _diagnostics.Add($"ERROR: The number {text} is not a valid Int32");
                
                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
            }

            // <whitespace>
            if (char.IsWhiteSpace(Current))
            {
                var start = _position;

                while (char.IsWhiteSpace(Current))
                    Next();

                var length = _position - start;

                var text = _text.Substring(start, length);
                
                return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, text, null);
            }

            // <operands>
            if (Current == '+')
                return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null); 
            else if (Current == '-')
                return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", null);
            else if (Current == '*')
                return new SyntaxToken(SyntaxKind.StarToken, _position++, "*", null);
            else if (Current == '/')
                return new SyntaxToken(SyntaxKind.SlashToken, _position++, "/", null);
            else if (Current == '(')
                return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(", null);
            else if (Current == ')')
                return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, ")", null);
            
            
            // We found a token that we don't know
            _diagnostics.Add($"ERROR: bad character input: '{Current}'");
            return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);
        }
    }

    public class SyntaxToken : SyntaxNode
    {
        public override SyntaxKind Kind { get; }
        public override IEnumerable<SyntaxNode> GetChildren() => Enumerable.Empty<SyntaxNode>();

        public int Position { get; }
        public string Text { get; }
        public object Value { get; }

        public SyntaxToken(SyntaxKind kind, int position, string text, object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }
    }

    public enum SyntaxKind
    {
        NumberToken,
        WhiteSpaceToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        BadToken,
        EndOfFileToken,
        BinaryExpression,
        ParenthesizedExpression
    }

    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }
        public abstract IEnumerable<SyntaxNode> GetChildren();
    }

    public abstract class ExpressionSyntax : SyntaxNode
    {
    }

    public sealed class NumberExpressionSyntax : ExpressionSyntax
    {
        public SyntaxToken NumberToken { get; }

        public NumberExpressionSyntax(SyntaxToken numberToken)
        {
            NumberToken = numberToken;
        }

        public override SyntaxKind Kind => SyntaxKind.NumberToken;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NumberToken;
        }
    }


    public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        public SyntaxToken OpenParenthesisToken { get; }
        public ExpressionSyntax Expression { get; }
        public SyntaxToken CloseParenthesisToken { get; }

        public ParenthesizedExpressionSyntax(SyntaxToken openParenthesisToken, ExpressionSyntax expression, SyntaxToken closeParenthesisToken)
        {
            OpenParenthesisToken = openParenthesisToken;
            Expression = expression;
            CloseParenthesisToken = closeParenthesisToken;
        }
        
        public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenParenthesisToken;
            yield return Expression;
            yield return CloseParenthesisToken;
        }
    }
    
    public sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public ExpressionSyntax Left { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Right { get; }

        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }

        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }
    }

    public sealed class SyntaxTree
    {
        public IReadOnlyList<string> Diagnostics { get; }
        public ExpressionSyntax Root { get; }
        public SyntaxToken EndOfFileToken { get; }

        public SyntaxTree(IEnumerable<string> diagnostics, ExpressionSyntax root, SyntaxToken endOfFileToken)
        {
            Diagnostics = diagnostics.ToArray();
            Root = root;
            EndOfFileToken = endOfFileToken;
        }


        public static SyntaxTree Parse(string text)
        {
            var parser = new Parser(text);
            return parser.Parse();
        }
    }

    public class Evaluator
    {
        private readonly ExpressionSyntax _root;

        public Evaluator(ExpressionSyntax root)
        {
            _root = root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(ExpressionSyntax node)
        {
            // <NumberExpression>
            if (node is NumberExpressionSyntax n)
                return (int) n.NumberToken.Value;

            // <BinaryExpression>
            if (node is BinaryExpressionSyntax b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);

                if (b.OperatorToken.Kind == SyntaxKind.PlusToken)
                    return left + right;
                else if (b.OperatorToken.Kind == SyntaxKind.MinusToken)
                    return left - right;
                else if (b.OperatorToken.Kind == SyntaxKind.StarToken)
                    return left * right;
                else if (b.OperatorToken.Kind == SyntaxKind.SlashToken)
                    return left / right;
                else
                    throw new Exception($"Unexpected binary operator found. {b.OperatorToken.Kind}");
            }

            if (node is ParenthesizedExpressionSyntax p)
                return EvaluateExpression(p.Expression);
            
            throw new Exception($"Unexpected node found. {node.Kind}");
        }
    }
}
