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

    public Scanner(string source)
    {
        this.source = source;
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
            // Check for var and const declaration
            case '>':
                var isVar = DoubleMatch('.', '<');
                var isConst = !isVar && DoubleMatch('w', '<');
                if (isVar || isConst) AddToken(isVar ? TokenType.Var : TokenType.Const);
                break;
            // Check for assignment
            case 'o':
                if (Match('/')) AddToken(TokenType.Assign);
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
                if (DoubleMatch('u', '^')) AddToken(TokenType.If);
                else if (DoubleMatch('o', '^')) AddToken(TokenType.For);
                else if (DoubleMatch('w', '^')) AddToken(TokenType.While);
                break;
            
            case ':':
                if (Match('D')) AddToken(TokenType.Func);
                break;
            
            case 'c':
                if (Match(':')) AddToken(TokenType.Return);
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
            
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;
            
            case '\n':
                _line++;
                break;

            default:
                if (IsDigit(c))
                {
                    Number();
                } else if (isAlpha(c)) {
                    Identifier();
                }else
                {
                    Program.Error(_line, "Unexpected character");
                    
                }
                break;
        }
    }

    private bool DoubleMatch(char next, char last)
    {
        _tmp = _current;
        var hasNext = Match(next);
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

        var value = source.Substring(_start, _current);
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
        
        AddToken(TokenType.Number, double.Parse(source.Substring(_start, _current)));
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();
        
        AddToken(TokenType.Identifier);
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
        var text = source.Substring(_start, _current);
        tokens.Add(new Token(type, text, literal, _line));
    }


    private bool IsEof()
    {
        return _current >= source.Length;
    }
}