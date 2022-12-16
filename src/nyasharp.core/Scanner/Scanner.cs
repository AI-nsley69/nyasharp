namespace nyasharp.Scanner;

public class Scanner
{
    private readonly string _source;
    private readonly List<Token> _tokens = new();
    private int _start;
    private int _current;
    private int _line = 1;

    private readonly Dictionary<string, TokenType> _keywords;
    public Scanner(string source)
    {
        this._source = source;
        _keywords = new Dictionary<string, TokenType>
        {
            { "pwint", TokenType.Print },
            { "twue", TokenType.True },
            { "fawse", TokenType.False }, 
            { "Numbew", TokenType.LiteralNumber },
            { "Stwing", TokenType.LiteralString },
            { "Boowean", TokenType.LiteralBool },
            { "nuww", TokenType.LiteralNull }
        };
    }

    public List<Token> ScanTokens()
    {
        while (!IsEof())
        {
            _start = _current;
            ScanToken();
        }
        
        _tokens.Add(new Token(TokenType.EOF, "", null, _line));
        return _tokens;
    }

    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            // Check for parentheses
            case '(': AddToken(TokenType.LeftParen);
                break;
            case ')': AddToken(TokenType.RightParen);
                break;
            // Check for dots and comma
            case ',': AddToken(TokenType.Comma);
                break;
            case '.': AddToken(TokenType.Dot);
                break;
            // Check for not operator
            case '~': AddToken(TokenType.Not);
                break;
            // Check for var and const declaration
            case '>':
                var isMaybeVar = Match('.');
                var isMaybeConst = Match('w');
                if (isMaybeConst || isMaybeVar)
                {
                    if (!Match('<'))
                    {
                        Default(c);
                        break;
                    }
                    AddToken(isMaybeVar ? TokenType.Var : TokenType.Const);
                }
                else Default(c);
                break;
            // Check for assignment
            case 'o':
                if (Match('/')) AddToken(TokenType.Assign);
                else Default(c);
                break;
            // Next 3 cases are for comparisons
            case '\\':
                if (!Match('o'))
                {
                    Default(c);
                    break;
                }
                if (Match('/')) AddToken(TokenType.Equal);
                else if (Match('_')) AddToken(TokenType.GreaterEqual);
                else if (Match('\\')) AddToken(TokenType.Greater);
                else Default(c);
                break;
            
            case '/':
                // Discard comments
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsEof()) Advance();
                } else if (Match('o'))
                {
                    if (!Match('/'))
                    {
                        Default(c);
                        break;
                    }
                    AddToken(TokenType.Less);
                }
                else Default(c);
                break;
            
            case '_':
                if (!Match('o'))
                {
                    Default(c);
                    break;
                }
                if (Match('_')) AddToken(TokenType.NotEqual);
                else if (Match('/')) AddToken(TokenType.LessEqual);
                else Default(c);
                break;
            
            // For loops, while loops and if statements
            case '^':
                var isIf = Match('u');
                var isElse = Match('e');
                var isFor = Match('o');
                var isWhile = Match('w');
                if (isIf || isElse || isFor || isWhile)
                {
                    if (!Match('^'))
                    {
                        Default(c);
                        break;
                    }
                    var type = TokenType.Nothing;
                    if (isIf) type = TokenType.If;
                    else if (isElse) type = TokenType.Else;
                    else if (isFor) type = TokenType.For;
                    else if (isWhile) type = TokenType.While;
                    if (type != TokenType.Nothing) AddToken(type);
                    else Default(c);
                }
                else Default(c);
                break;
            
            case ':':
                if (Match('D')) AddToken(TokenType.Func);
                else if (Match('>')) AddToken(TokenType.BlockStart);
                else Default(c);
                break;
            
            case '<':
                if (Match(':')) AddToken(TokenType.BlockEnd);
                else Default(c);
                break;
            
            case 'c':
                if (Match(':')) AddToken(TokenType.Return);
                else Default(c);
                break;
            // Check for OR
            case 'v':
                if (Match('.'))
                {
                    if (!Match('v'))
                    {
                        Default(c);
                        break;
                    }
                    AddToken(TokenType.Or);
                } else Default(c);
                break;
            // Check for AND
            case '&':
                if (Match('.'))
                {
                    if (!Match('&'))
                    {
                        Default(c);
                        break;
                    }
                    AddToken(TokenType.And);
                } else Default(c);
                break;
            // Next 2 are for Arithmetic ops
            case '+':
                if (!Match('.'))
                {
                    Default(c);
                    break;
                }
                if (Match('+')) AddToken(TokenType.Add);
                else if (Match('*')) AddToken(TokenType.Mult);
                else Default(c);
                break;
            
            case '-':
                if (!Match('.'))
                {
                    Default(c);
                    break;
                }
                if (Match('-')) AddToken(TokenType.Sub);
                else if (Match('*')) AddToken(TokenType.Div);
                else Default(c);
                break;
            // Handle modulo
            case '%':
                if (!Match('.'))
                {
                    Default(c);
                    break;
                }
                if (Match('%')) AddToken(TokenType.Mod);
                else Default(c);
                break;
            // Handle strings
            case '"': String();
                break;
            
            case ';': AddToken(TokenType.SemiColon);
                break;
            
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;
            
            case '\n':
                _line++;
                break;

            default:
                Default(c);
                break;
        }
    }

    private void Default(char c)
    {
        if (IsDigit(c))
        {
            Number();
        } else if (IsAlpha(c)) {
            Identifier();
        }else
        {
            // Take care of the pesky BOM character
            if (c == 0xFEFF) return;
            core.Error(_line, "Unexpected character: " + c);
        }
    }

    private bool Match(char expected)
    {
        if (IsEof()) return false;
        if (_source[_current] != expected) return false;

        _current++;
        return true;
    }

    private char Peek()
    {
        return IsEof() ? '\0' : _source[_current];
    }

    private char PeekNext()
    {
        return _current + 1 >= _source.Length ? '\0' : _source[_current + 1];
    }

    private char Advance()
    {
        return _source[_current++];
    }

    private void String()
    {
        while (Peek() != '"' && !IsEof())
        {
            if (Peek() == '\n') _line++;
            Advance();
        }

        if (IsEof())
        {
            core.Error(_line, "Unterminated string.");
            return;
        }

        Advance();
        var delta = (_current - 1) - (_start + 1);
        var value = _source.Substring(_start + 1, delta);
        AddToken(TokenType.String, value);
    }

    private void Number()
    {
        while (IsDigit(Peek())) Advance();
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();
            while (IsDigit(Peek())) Advance();
        }
        
        AddToken(TokenType.Number, double.Parse(GetSubstring()));
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();
        var text = GetSubstring();
        _keywords.TryGetValue(text, out var type);
        if (type == TokenType.Nothing) type = TokenType.Identifier;
        AddToken(type);
    }
    private bool IsAlphaNumeric(char c)
    {
        return IsDigit(c) || IsAlpha(c);
    }

    private bool IsDigit(char c)
    {
        return c is >= '0' and <= '9';
    }
    
    private bool IsAlpha(char c) {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               c == '_';
    }

    private void AddToken(TokenType type, object? literal = null)
    {
        var text = GetSubstring();
        _tokens.Add(new Token(type, text, literal, _line));
    }

    private string GetSubstring()
    {
        var delta = _current - _start;
        return _source.Substring(_start, delta);
    }


    private bool IsEof()
    {
        return _current >= _source.Length;
    }
}