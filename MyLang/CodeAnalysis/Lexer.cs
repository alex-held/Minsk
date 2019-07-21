using System.Collections.Generic;

namespace MyLang.CodeAnalysis {
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
}