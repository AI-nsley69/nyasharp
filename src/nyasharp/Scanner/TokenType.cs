namespace nyasharp;

public enum TokenType
{
    // Parentheses
    LeftParen, RightParen,
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