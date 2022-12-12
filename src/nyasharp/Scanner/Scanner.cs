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
                if (DoubleMatch('o', '/')) AddToken(TokenType.Less);
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
            
            // Arithmetic ops
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
            
            default:
                Program.Error(_line, "Unexpected character");
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

    private char Advance()
    {
        return source[_current++];
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