using System.Globalization;

namespace nyasharp;

public class Scanner
{
    private readonly string source;
    private readonly List<Token> tokens = new List<Token>();
    private int _start = 0;
    private int _current = 0;
    private int _tmp = 0;
    private int _line = 1;

    private readonly Dictionary<string, TokenType> keywords;
    public Scanner(string source)
    {
        this.source = source;
        keywords = new Dictionary<string, TokenType>();
        keywords.Add("pwint", TokenType.Print);
    }

    public List<Token> ScanTokens()
    {
        while (!IsEof())
        {
            _start = _current;
            ScanToken();
        }
        
        tokens.Add(new Token(TokenType.EOF, "", null, _line));
        return tokens;
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
            // Check for var and const declaration
            case '>':
                var isMaybeVar = Match('.');
                var isMaybeConst = Match('w');
                if (isMaybeConst || isMaybeVar)
                {
                    if (!Match('<')) break;
                    AddToken(isMaybeVar ? TokenType.Var : TokenType.Const);
                }
                break;
            // Check for assignment
            case 'o':
                if (Match('/')) AddToken(TokenType.Assign);
                else Default(c);
                break;
            // Next 3 cases are for comparisons
            case '\\':
                if (!Match('o')) break;
                if (Match('/')) AddToken(TokenType.Equal);
                else if (Match('_')) AddToken(TokenType.GreaterEqual);
                else if (Match('\\')) AddToken(TokenType.Greater);
                break;
            
            case '/':
                // Discard comments
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsEof()) Advance();
                } else if (DoubleMatch('o', '/')) AddToken(TokenType.Less);
                break;
            
            case '_':
                if (!Match('o')) break;
                if (Match('_')) AddToken(TokenType.NotEqual);
                else if (Match('/')) AddToken(TokenType.LessEqual);
                break;
            
            // For loops, while loops and if statements
            case '^':
                var isIf = Match('u');
                var isFor = Match('o');
                var isWhile = Match('w');
                if (isIf || isFor || isWhile)
                {
                    if (!Match('^')) break;
                    var type = TokenType.Nothing;
                    if (isIf) type = TokenType.If;
                    else if (isFor) type = TokenType.For;
                    else if (isWhile) type = TokenType.While;
                    if (type != TokenType.Nothing) AddToken(type);   
                }
                break;
            
            case ':':
                if (Match('D')) AddToken(TokenType.Func);
                else if (Match('>')) AddToken(TokenType.BlockStart);
                break;
            
            case '<':
                if (Match(':')) AddToken(TokenType.BlockEnd);
                break;
            
            case 'c':
                if (Match(':')) AddToken(TokenType.Return);
                else Default(c);
                break;
            
            // Next 2 are for Arithmetic ops
            case '+':
                if (!Match('.')) break;
                if (Match('+')) AddToken(TokenType.Add);
                else if (Match('*')) AddToken(TokenType.Mult);
                break;
            
            case '-':
                if (!Match('.')) break;
                if (Match('-')) AddToken(TokenType.Sub);
                else if (Match('*')) AddToken(TokenType.Div);
                break;
            // Handle strings
            case '"': String();
                break;
            
            case ';':
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
        } else if (isAlpha(c)) {
            Identifier();
        }else
        {
            Program.Error(_line, "Unexpected character: " + c);
        }
    }

    private bool DoubleMatch(char next, char last)
    {
        var hasNext = Match(next);
        _tmp = _current;
        _current++;
        var hasLast = Match(last);
        var check = hasNext && hasLast;
        if (!check) _current = _tmp;
        return check;
    }

    private bool Match(char expected)
    {
        if (IsEof()) return false;
        if (source[_current] != expected) return false;

        _current++;
        return true;
    }

    private char Peek()
    {
        return IsEof() ? '\0' : source[_current];
    }

    private char PeekNext()
    {
        return _current + 1 >= source.Length ? '\0' : source[_current + 1];
    }

    private char Advance()
    {
        return source[_current++];
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
            Program.Error(_line, "Unterminated string.");
            return;
        }

        Advance();
        
        var value = GetSubstring();
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
        TokenType type;
        if (keywords.TryGetValue(text, out type));
        if (type == TokenType.Nothing) type = TokenType.Identifier;
        AddToken(type);
    }
    private bool IsAlphaNumeric(char c)
    {
        return IsDigit(c) || isAlpha(c);
    }

    private bool IsDigit(char c)
    {
        return c is >= '0' and <= '9';
    }
    
    private bool isAlpha(char c) {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               c == '_';
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

	private void AddToken(TokenType type, object literal)
    {
        var text = GetSubstring();
        tokens.Add(new Token(type, text, literal, _line));
    }

    private string GetSubstring()
    {
        var delta = _current - _start;
        return source.Substring(_start, delta);
    }


    private bool IsEof()
    {
        return _current >= source.Length;
    }
}