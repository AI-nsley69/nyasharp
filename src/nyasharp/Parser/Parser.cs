using System.ComponentModel.Design;
using System.Data;
using System.Linq.Expressions;
using Microsoft.VisualBasic.CompilerServices;
using nyasharp.AST;

namespace nyasharp.Parser;

public class Parser
{
    private class ParserError : Exception {};
    private readonly List<Token> _tokens;
    private int _current = 0;

    public Parser(List<Token> tokens)
    {
        this._tokens = tokens;
    }

    public List<Stmt> Parse()
    {
        List<Stmt> statements = new List<Stmt>();
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

    private Stmt Declaration()
    {
        try
        {
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
        if (Match(TokenType.Print)) return PrintStatement();
        if (Match(TokenType.BlockStart)) return new Stmt.Block(Block());
        return ExpressionStatement();
    }

    private Stmt PrintStatement()
    {
        Expr value = Expression();
        Consume(TokenType.SemiColon, "Expected ';' after value.");
        return new Stmt.Print(value);
    }

    private Stmt VarDeclaration()
    {
        Token name = Consume(TokenType.Identifier, "Expected variable name.");

        Expr? initalizer = null;

        if (Match(TokenType.Assign))
        {
            initalizer = Expression();
        }

        Consume(TokenType.SemiColon, "Expected ';' after variable declaration");
        return new Stmt.Var(name, initalizer);
    }
    
    private Stmt ExpressionStatement() {
        Expr expr = Expression();
        Consume(TokenType.SemiColon, "Expected ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private List<Stmt> Block()
    {
        List<Stmt> statements = new List<Stmt>();

        while (!Check(TokenType.BlockEnd) && !IsEof())
        {
            statements.Add(Declaration());
        }

        Consume(TokenType.BlockEnd, "Expected '<:' after block.");
        return statements;
    }

    private Expr Assignment()
    {
        Expr expr = Equality();

        if (Match(TokenType.Assign))
        {
            Token equals = Previous();
            Expr value = Assignment();

            if (expr is Expr.Variable)
            {
                Token name = ((Expr.Variable)expr).name;
                return new Expr.Assign(name, value);
            }

            Error(equals, "Invalid assignment target.");
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
        Expr expr = Term();

        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
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

        return Primary();
    }

    private Expr Primary()
    {
        if (Match(TokenType.False)) return new Expr.Literal(false);
        if (Match(TokenType.True)) return new Expr.Literal(true);
        if (Match(TokenType.Null)) return new Expr.Literal(null);

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
        Program.Error(token, message);
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