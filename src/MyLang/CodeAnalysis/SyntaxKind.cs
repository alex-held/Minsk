namespace MyLang.CodeAnalysis
{
    public enum SyntaxKind
    {
        // Tokens
        BadToken,
        EndOfFileToken,
        WhiteSpaceToken,
        NumberToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        
        // Expressions
        BinaryExpression,
        ParenthesizedExpression,
        LiteralToken
    }
}
