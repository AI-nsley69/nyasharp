using System.IO.Compression;
using System.Linq.Expressions;
using nyasharp.AST;

namespace nyasharp.Interpreter;

public class Interpreter : Expr.Visitor<object>, Stmt.Visitor
{
    public Environment _globals = new();
    public Environment _environment;

    public Interpreter()
    {
        _environment = _globals;
    }
    public void interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var stmt in statements)
            {
                Execute(stmt);
            }
        }
        catch (RuntimeError error)
        {
            Program.RuntimeError(error);
        }
    }

    public string Stringify(object obj)
    {
        if (obj == null) return "null";

        if (obj is double)
        {
            string text = obj.ToString();
            if (text.EndsWith(".0"))
            {
                text = text.Substring(0, text.Length - 2);
            }

            return text;
        }

        return obj.ToString();
    }

    public object VisitExpressionAssign(Expr.Assign assign)
    {
        Object value = Evaluate(assign.value);
        _environment.Assign(assign.name, value);
        return value;
    }

    public object VisitExpressionBinary(Expr.Binary binary)
    {
        var left = Evaluate(binary.left);
        var right = Evaluate(binary.right);

        switch (binary.op.type)
        {
            // Comparisons
            case TokenType.Greater:
                CheckNumberOperands(binary.op, left, right);
                return (double)left > (double)right;
            case TokenType.GreaterEqual:
                CheckNumberOperands(binary.op, left, right);
                return (double)left >= (double)right;
            case TokenType.Less:
                CheckNumberOperands(binary.op, left, right);
                return (double)left < (double)right;
            case TokenType.LessEqual:
                CheckNumberOperands(binary.op, left, right);
                return (double)left <= (double)right;
            case TokenType.Equal: return IsEqual(left, right);
            case TokenType.NotEqual: return !IsEqual(left, right);
            // Arithmetic
            case TokenType.Sub: return (double)left - (double)right;
            case TokenType.Add:
                if (left is double d1 && right is double d2) return d1 + d2;
                if (left is string s1 && right is string s2) return s1 + s2;
                if (left is string || right is string) return left.ToString() + right;
                throw new RuntimeError(binary.op,
                    "Operands must be two numbers or two strings.");
            case TokenType.Div:
                CheckNumberOperands(binary.op, left, right);
                return (double)left / (double)right;
            case TokenType.Mult:
                CheckNumberOperands(binary.op, left, right);
                return (double)left * (double)right;
        }
        // Unreachable
        return null;
    }

    public object VisitExpressionCall(Expr.Call call)
    {
        object callee = Evaluate(call.callee);

        List<object> arguments = new List<object>();
        foreach (var arg in call.args)
        {
            arguments.Add(Evaluate(arg));
        }

        if (callee is ICallable func)
        {
            if (arguments.Count != func.Arity())
            {
                throw new RuntimeError(call.paren,
                    "Expected" + func.Arity() + " arguments but got " + arguments.Count + ".");
            }
            return func.Call(this, arguments);
        }
        else
        {
            throw new RuntimeError(call.paren, "Can only call functions and classes.");
        }
    }
    
    private void CheckNumberOperands(Token op, object left, object right)
    {
        if (left is double && right is double) return;
        throw new RuntimeError(op, "Operands must be numbers");
    }

    private bool IsEqual(object a, object b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a.Equals(b);
    }
    
    public object VisitExpressionGrouping(Expr.Grouping grouping)
    {
        return Evaluate(grouping.expr);
    }

    private object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    public void ExecuteBlock(List<Stmt> statements, Environment environment)
    {
        Environment previous = this._environment;
        try
        {
            this._environment = environment;
            foreach (var stmt in statements)
            {
                Execute(stmt);
            }
        }
        finally
        {
            this._environment = previous;
        }
    }

    public object VisitExpressionLiteral(Expr.Literal literal)
    {
        return literal.value;
    }

    public object VisitExpressionLogical(Expr.Logical logical)
    {
        object left = Evaluate(logical.left);

        if (logical.op.type == TokenType.Or)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(logical.right);
    }

    public object VisitExpressionUnary(Expr.Unary unary)
    {
        var right = Evaluate(unary.expr);

        switch (unary.op.type)
        {
            case TokenType.Not:
                return !IsTruthy(right);
            case TokenType.Sub:
                CheckNumberOperand(unary.op, right);
                return -(double)right;
        }
        // Unreachable
        return null;
    }

    public object VisitExpressionVariable(Expr.Variable variable)
    {
        return _environment.Get(variable.name);
    }

    public void VisitStmtExpression(Stmt.Expression stmt)
    {
        Evaluate(stmt.expression);
    }

    public void VisitStmtFunc(Stmt.Func func)
    {
        Function function = new Function(func);
        _environment.Define(func.name.lexeme, function);
    }

    public void VisitStmtIf(Stmt.If ifStmt)
    {
        var condition = Evaluate(ifStmt.condition);
        
        if (IsTruthy(condition))
        {
            Execute(ifStmt.thenBranch);
        } else if (ifStmt.elseBranch != null)
        {
            Execute(ifStmt.elseBranch);
        }
    }

    public void VisitStmtPrint(Stmt.Print print)
    {
        object value = Evaluate(print.expression);
        Console.WriteLine(Stringify(value));
    }

    public void VisitStmtReturn(Stmt.Return rtrn)
    {
        object value = null;
        if (rtrn.value != null) value = Evaluate(rtrn.value);

        throw new Return(value);
    }

    public void VisitStmtVar(Stmt.Var var)
    {
        object value = Evaluate(var.initializer);
        _environment.Define(var.name.lexeme, value);
    }

    public void VisitStmtWhile(Stmt.While whileStmt)
    {
        while (IsTruthy(Evaluate(whileStmt.condition)))
        {
            Execute(whileStmt.body);
        }
    }

    public void VisitStmtBlock(Stmt.Block block)
    {
        ExecuteBlock(block.statements, new Environment(_environment));
    }

    private void CheckNumberOperand(Token op, object operand)
    {
        if (operand is double) return;
        throw new RuntimeError(op, "Operand must be a number");
    }

    private bool IsTruthy(object obj)
    {
        if (obj == null) return false;
        if (obj is Expr.Literal && ((Expr.Literal)obj).value is bool b1) return b1;
        if (obj is bool b) return b;
        return true;
    }
}