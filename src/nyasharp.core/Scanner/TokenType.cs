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
    Add, Sub, Mult, Div, Mod,
    // Comparison
    Not, NotEqual,
    Assign, Equal,
    Greater, GreaterEqual,
    Less, LessEqual,
    // Literals
    Identifier, String, Number,
    // defines a literal type keyword
    LiteralString, LiteralNumber, LiteralBool, LiteralNull,
    // Keywords
    And, Or,  Null,
    Class, Func, Return, For, While, If,
    Else, False, True, Super, 
    Var, Const,
    Print, 
    
    EOF,

    // basically a "//"
    CommentLStart,
    //used when the tokenisation fail starting from a character. this character is added as an invalid token
    Invalid, 

}