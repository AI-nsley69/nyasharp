namespace nyasharp;

public enum TokenType
{
    // Help parsing by indicating that something is nothing, has to be used for the keywords dict
    Nothing,
    // Parentheses
    LeftParen, RightParen,
    // Code Blocks
    BlockStart, BlockEnd,
    // Semicolon, Dots & Commas
    SemiColon, Dot, Comma,
    // Ops
    Add, Sub, Mult, Div,
    // Comparison
    Not, NotEqual,
    Assign, Equal,
    Greater, GreaterEqual,
    Less, LessEqual,
    // Literals
    Identifier, String, Number,
    // Keywords
    And, Or,  Null,
    Class, Func, Return, For, While, If,
    Else, False, True, Super, 
    Var, Const,
    Print, 
    
    EOF
}