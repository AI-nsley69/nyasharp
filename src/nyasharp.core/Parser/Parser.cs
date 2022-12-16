using System.Linq.Expressions;
using nyasharp.AST;

namespace nyasharp.Parser;

public class Parser
{
    private class ParserError : Exception {};
    private readonly List<Token> _tokens;
    private int _current;

    public Parser(List<Token> tokens)
    {
        this._tokens = tokens;
    }

    public List<Stmt?> Parse()
    {
        List<Stmt?> statements = new List<Stmt?>();
        while (!IsEof())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Stmt? Declaration()
    {
        try
        {
            if (Match(TokenType.Func)) return Function("function");
            if (Match(TokenType.Var)) return VarDeclaration();
            return Statement();
        }
        catch (ParserError error)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt Statement()
    {
        if (Match(TokenType.For)) return ForStatement();
        if (Match(TokenType.If)) return IfStatement();
        if (Match(TokenType.Print)) return PrintStatement();
        if (Match(TokenType.Return)) return ReturnStatement();
        if (Match(TokenType.While)) return WhileStatement();
        if (Match(TokenType.BlockStart)) return new Stmt.Block(Block());
        return ExpressionStatement();
    }

    private Stmt ForStatement()
    {
        Consume(TokenType.LeftParen, "Expected '(' after '^o^'.");
        Stmt? initializer;
        if (Match(TokenType.SemiColon))
        {
            initializer = null;
        } else if (Match(TokenType.Var))
        {
            initializer = VarDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        Expr? condition = null;
        if (!Check(TokenType.SemiColon))
        {
            condition = Expression();
        }

        Consume(TokenType.SemiColon, "Expected ';' after loop condition");

        Expr? increment = null;
        if (!Check(TokenType.RightParen))
        {
            increment = Expression();
        }

        Consume(TokenType.RightParen, "Expected ')' after clause");
        
        Stmt body = Statement();
        if (increment != null)
        {
            body = new Stmt.Block(new List<Stmt?>()
            {
                body,
                new Stmt.Expression(increment)
            });
        }

        condition ??= new Expr.Literal(true);
        body = new Stmt.While(condition, body);

        if (initializer != null)
        {
            body = new Stmt.Block(new List<Stmt?>() {initializer, body});
        }

        return body;
    }

    private Stmt IfStatement()
    {
        Consume(TokenType.LeftParen, "Expected '(' after '^u^'.");
        Expr condition = Expression();
        Consume(TokenType.RightParen, "Expected ')' after if statement.");

        Stmt thenBranch = Statement();
        Stmt? elseBranch = null;

        if (Match(TokenType.Else))
        {
            elseBranch = Statement();
        }

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt PrintStatement()
    {
        Expr value = Expression();
        Consume(TokenType.SemiColon, "Expected ';' after value.");
        return new Stmt.Print(value);
    }

    private Stmt ReturnStatement()
    {
        Token keyword = Previous();
        Expr? value = null;
        if (!Check(TokenType.SemiColon))
        {
            value = Expression();
        }

        Consume(TokenType.SemiColon, "Expected ';' after return.");
        return new Stmt.Return(keyword, value);
    }

    private Stmt VarDeclaration()
    {
        Token name = Consume(TokenType.Identifier, "Expected variable name.");

        Expr? initializer = null;

        if (Match(TokenType.Assign))
        {
            initializer = Expression();
        }

        Consume(TokenType.SemiColon, "Expected ';' after variable declaration");
        return new Stmt.Var(name, initializer);
    }

    private Stmt WhileStatement()
    {
        Consume(TokenType.LeftParen, "Expected '(' after '^w^'");
        Expr condition = Expression();
        Consume(TokenType.RightParen, "Expected ')' after condition.");
        Stmt body = Statement();

        return new Stmt.While(condition, body);
    }
    
    private Stmt ExpressionStatement() {
        Expr expr = Expression();
        Consume(TokenType.SemiColon, "Expected ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private Stmt.Func Function(string kind)
    {
        Token name = Consume(TokenType.Identifier, "Expected" + kind + " name.");
        Consume(TokenType.LeftParen, "Expected '(' after " + kind + " name.");
        List<Token> parameters = new List<Token>();
        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }

                parameters.Add(Consume(TokenType.Identifier, "Expected parameter name."));
            } while (Match(TokenType.Comma));
        }

        Consume(TokenType.RightParen, "Expected ')' after parameters.");

        Consume(TokenType.BlockStart, "Expected ':>' before " + kind + " body.");
        List<Stmt?> body = Block();
        return new Stmt.Func(name, parameters, body);
    }

    private List<Stmt?> Block()
    {
        List<Stmt?> statements = new List<Stmt?>();

        while (!Check(TokenType.BlockEnd) && !IsEof())
        {
            statements.Add(Declaration());
        }

        Consume(TokenType.BlockEnd, "Expected '<:' after block.");
        return statements;
    }

    private Expr Assignment()
    {
        Expr expr = Or();

        if (Match(TokenType.Assign))
        {
            Token equals = Previous();
            Expr value = Assignment();

            if (expr is Expr.Variable variable)
            {
                Token name = variable.name;
                return new Expr.Assign(name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Or()
    {
        Expr expr = And();

        while (Match(TokenType.Or))
        {
            Token op = Previous();
            Expr right = And();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr And()
    {
        Expr expr = Equality();

        while (Match(TokenType.And))
        {
            Token op = Previous();
            Expr right = Equality();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }
    
    private Expr Equality()
    {
        Expr expr = Comparison();

        while (Match(TokenType.NotEqual, TokenType.Equal))
        {
            Token op = Previous();
            Expr right = Comparison();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        Expr expr = Mod();

        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            Token op = Previous();
            Expr right = Mod();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Mod()
    {
        Expr expr = Term();

        while (Match(TokenType.Mod))
        {
            Token op = Previous();
            Expr right = Term();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();

        while (Match(TokenType.Add, TokenType.Sub))
        {
            Token op = Previous();
            Expr right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        Expr expr = Unary();

        while (Match(TokenType.Mult, TokenType.Div))
        {
            Token op = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(TokenType.Sub, TokenType.Sub))
        {
            Token op = Previous();
            Expr right = Unary();
            return new Expr.Unary(op, right);
        }

        return Call();
    }

    private Expr Call()
    {
        Expr expr = Primary();

        while (true)
        {
            if (Match(TokenType.LeftParen))
            {
                expr = FinishCall(expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        List<Expr> arguments = new List<Expr>();
        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }
                arguments.Add(Expression());
            } while (Match(TokenType.Comma));
        }

        Token paren = Consume(TokenType.RightParen, "Expected ')' after arguments.");

        return new Expr.Call(callee, paren, arguments);
    }

    private Expr Primary()
    {
        if (Match(TokenType.False)) return new Expr.Literal(false);
        if (Match(TokenType.True)) return new Expr.Literal(true);
        if (Match(TokenType.Null)) return new Expr.Literal(null);

        if (Match(TokenType.LiteralBool)) return new Expr.Literal(TokenType.LiteralBool);
        if (Match(TokenType.LiteralNull)) return new Expr.Literal(TokenType.LiteralNull);
        if (Match(TokenType.LiteralNumber)) return new Expr.Literal(TokenType.LiteralNumber);
        if (Match(TokenType.LiteralString)) return new Expr.Literal(TokenType.LiteralNumber);

        if (Match(TokenType.Number, TokenType.String))
        {
            return new Expr.Literal(Previous().literal);
        }

        if (Match(TokenType.Identifier))
        {
            return new Expr.Variable(Previous());
        }

        if (Match(TokenType.LeftParen))
        {
            Expr expr = Expression();
            Consume(TokenType.RightParen, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), "Expected expression");
    }

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (!Check(type)) continue;
            Advance();
            return true;
        }

        return false;
    }
    
    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();

        throw Error(Peek(), message);
    }

    private bool Check(TokenType type)
    {
        if (IsEof()) return false;
        return Peek().type == type;
    }

    private Token Advance()
    {
        if (!IsEof()) _current++;
        return Previous();
    }

    private bool IsEof()
    {
        return Peek().type == TokenType.EOF;
    }

    private Token Peek()
    {
        return _tokens[_current];
    }

    private Token Previous()
    {
        return _tokens[_current - 1];
    }
    
    private ParserError Error(Token token, string message)
    {
        core.Error(token, message);
        return new ParserError();
    }

    private void Synchronize()
    {
        Advance();
        while (!IsEof())
        {
            if (Previous().type == TokenType.SemiColon) return;

            switch (Peek().type)
            {
                case TokenType.Class:
                case TokenType.Func:
                case TokenType.Const:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }

            Advance();
        }
    }
}