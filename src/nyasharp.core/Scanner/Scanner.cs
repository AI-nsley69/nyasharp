namespace nyasharp.Scanner;

public class Scanner : IScanner
{
    private readonly string _source;
    private readonly List<Token> _tokens = new();
    private int _start;
    private int _current;
    private int _line = 1;

    private readonly Dictionary<string, TokenType> _keywords;
    public Scanner(string source) {
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

    public List<Token> ScanTokens(string source) {
        while (!IsEof()) {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.EOF, "", null, _line));
        return _tokens;
    }

    const char LEFT_PAR = '(';
    const char RIGHTPAR = ')';
    const char COMMA___ = ',';
    const char DOT_____ = '.';
    const char NOT_____ = '~';
    const char VAR1____ = '>';
    const char ASIGN1__ = 'o';
    const char COMP_OP1 = '\\';
    const char COMP_OP2 = '/';
    const char COMP_OP3 = '_';
    const char FLOW1___ = '^';
    const char BLOCK_ST = ':';
    const char BLOCKEND = '<';
    const char RETURN__ = 'c';
    const char OR______ = 'v';
    const char AND_____ = '&';
    const char PLUS____ = '+';
    const char MINUS___ = '-';
    const char MODULO__ = '%';
    const char STRING__ = '"';
    const char LINE_END = ';';
    const char SPACE___ = ' ';
    const char RCHARIOT = '\r';
    const char TAB_____ = '\t';
    const char NEW_LINE = '\n';


    private static bool Then(Action ac) {
        ac();
        return true;
    }
    private bool AddTk(TokenType type) => Then(() => AddToken(type));

    private void ScanToken() {
        var c = Advance();

        if /**/ (c == LEFT_PAR) AddTk(TokenType.LeftParen);
        else if (c == RIGHTPAR) AddTk(TokenType.RightParen);
        else if (c == COMMA___) AddTk(TokenType.Comma);
        else if (c == DOT_____) AddTk(TokenType.Dot);
        else if (c == NOT_____) AddTk(TokenType.Not);
        else if (c == VAR1____) { // Check for var and const declaration
            var isMaybeVar = Match('.');
            var isMaybeConst = Match('w');
            if (isMaybeConst || isMaybeVar) {
                if (!Match('<')) { Default(c); }
                else { AddToken(isMaybeVar ? TokenType.Var : TokenType.Const); }
            }
            else { Default(c); }
        }
        else if (c == ASIGN1__) { // Check for assignment
            if (Match('/')) { AddToken(TokenType.Assign); }
            else { Default(c); }
        }
        else if (c == COMP_OP1) { // Next 3 cases are for comparisons
            if /**/(!Match('o')) { Default(c); return; }
            if /**/ (Match('/')) { AddToken(TokenType.Equal); }
            else if (Match('_')) { AddToken(TokenType.GreaterEqual); }
            else if (Match('\\')) { AddToken(TokenType.Greater); }
            else { Default(c); }
        }
        else if (c == COMP_OP2) {
            if (Match('/')) { // Discard comments
                while (Peek() != '\n' && !IsEof()) Advance();
            }
            else if (Match('o')) {
                if (!Match('/')) { Default(c); }
                else { AddToken(TokenType.Less); }
            }
            else { Default(c); }
        }
        else if (c == COMP_OP3) {
            if /**/(!Match('o')) { Default(c); }
            else if (Match('_')) { AddToken(TokenType.NotEqual); }
            else if (Match('/')) { AddToken(TokenType.LessEqual); }
            else { Default(c); }
        }
        else if (c == FLOW1___) { // For loops, while loops and if statements
            var isIf = Match('u');
            var isElse = Match('e');
            var isFor = Match('o');
            var isWhile = Match('w');
            if (isIf || isElse || isFor || isWhile) {
                if (!Match('^')) { Default(c); }
                else {
                    var type = TokenType.Nothing;
                    if /*---*/(isIf) { type = TokenType.If; }
                    else if (isElse) { type = TokenType.Else; }
                    else if (isFor) { type = TokenType.For; }
                    else if (isWhile) { type = TokenType.While; }
                    if (type != TokenType.Nothing) { AddToken(type); }
                    else /*----------------------*/{ Default(c); }
                }
            }
            else { Default(c); }
        }
        else if (c == BLOCK_ST) {
            if /*-*/(Match('D')) AddToken(TokenType.Func);
            else if (Match('>')) AddToken(TokenType.BlockStart);
            else { Default(c); }
        }
        else if (c == BLOCKEND) {
            if (Match(':')) AddToken(TokenType.BlockEnd);
            else { Default(c); }
        }
        else if (c == RETURN__) {
            if (Match(':')) AddToken(TokenType.Return);
            else { Default(c); }
        }
        else if (c == OR______) { // Check for OR
            if (Match('.')) {
                if (!Match('v')) { Default(c); }
                else { AddToken(TokenType.Or); }
            }
            else { Default(c); }
        }
        else if (c == AND_____) { // Check for AND
            if (Match('.')) {
                if (!Match('&')) { Default(c); }
                else { AddToken(TokenType.And); }
            }
            else { Default(c); }
        }
        else if (c == PLUS____) // Next 2 are for Arithmetic ops
        {
            if /**/(!Match('.')) { Default(c); }
            else if (Match('+')) { AddToken(TokenType.Add); }
            else if (Match('*')) { AddToken(TokenType.Mult); }
            else { Default(c); }
        }
        else if (c == MINUS___) {
            if /**/(!Match('.')) { Default(c); }
            else if (Match('-')) { AddToken(TokenType.Sub); }
            else if (Match('*')) { AddToken(TokenType.Div); }
            else { Default(c); }
        }
        else if (c == MODULO__) // Handle modulo
        {
            if /**/(!Match('.')) { Default(c); }
            else if (Match('%')) { AddToken(TokenType.Mod); }
            else /*-----------*/ { Default(c); }
        }
        else if (c == STRING__) { String(); } // Handle strings
        else if (c == LINE_END) { AddTk(TokenType.SemiColon); }
        else if (c == SPACE___) { } // Ignore whitespace.
        else if (c == RCHARIOT) { }
        else if (c == TAB_____) { }
        else if (c == NEW_LINE) { _line++; }
        else /*--------------*/ { Default(c); }
    }

    private void Default(char c) {
        if (IsDigit(c)) {
            Number();
        }
        else if (IsAlpha(c)) {
            Identifier();
        }
        else {
            // Take care of the pesky BOM character
            if (c == 0xFEFF) return;
            core.Error(_line, "Unexpected character: " + c);
        }
    }

    private bool Match(char expected) {
        if (IsEof()) return false;
        if (_source[_current] != expected) return false;

        _current++;
        return true;
    }

    private char Peek() {
        return IsEof() ? '\0' : _source[_current];
    }

    private char Peek(int offset) {
        return IsEof(offset) ? '\0' : _source[_current + offset];
    }

    private char PeekNext() {
        return _current + 1 >= _source.Length ? '\0' : _source[_current + 1];
    }

    private char Advance() {
        return _source[_current++];
    }

    private void String() {
        while (Peek() != '"' && !IsEof()) {
            if (Peek() == '\n') _line++;
            Advance();
        }

        if (IsEof()) {
            core.Error(_line, "Unterminated string.");
            return;
        }

        Advance();
        var delta = (_current - 1) - (_start + 1);
        var value = _source.Substring(_start + 1, delta);
        AddToken(TokenType.String, value);
    }

    private void Number() {
        while (IsDigit(Peek())) Advance();
        if (Peek() == '.' && IsDigit(PeekNext())) {
            Advance();
            while (IsDigit(Peek())) Advance();
        }

        AddToken(TokenType.Number, double.Parse(GetSubstring()));
    }

    private void Identifier() {
        while (IsAlphaNumeric(Peek())) Advance();
        var text = GetSubstring();
        _keywords.TryGetValue(text, out var type);
        if (type == TokenType.Nothing) type = TokenType.Identifier;
        AddToken(type);
    }
    private bool IsAlphaNumeric(char c) {
        return IsDigit(c) || IsAlpha(c);
    }

    private bool IsDigit(char c) {
        return c is >= '0' and <= '9';
    }

    private bool IsAlpha(char c) {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               c == '_';
    }

    private void AddToken(TokenType type, object? literal = null) {
        var text = GetSubstring();
        _tokens.Add(new Token(type, text, literal, _line));
    }

    private string GetSubstring() {
        var delta = _current - _start;
        return _source.Substring(_start, delta);
    }


    private bool IsEof() {
        return _current >= _source.Length;
    }

    private bool IsEof(int offset) {
        return _current + offset >= _source.Length;
    }
}