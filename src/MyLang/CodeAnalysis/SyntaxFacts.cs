namespace MyLang.CodeAnalysis {
    internal static class SyntaxFacts
    {
        /// <summary>
        /// Gets the operator precedence for a given <see cref="SyntaxKind"/>. 
        /// </summary>
        /// <returns>The Precedence. 0 if not an operator.</returns>
        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 2;
                
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 1;
                
                
                default:
                    return 0;
            }
        }
    }
}
