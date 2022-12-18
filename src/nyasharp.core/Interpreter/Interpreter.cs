using System.Linq.Expressions;
using nyasharp.AST;
using nyasharp.Interpreter.Natives;

namespace nyasharp.Interpreter;

public class Interpreter : Expr.Visitor<object>, Stmt.Visitor
{
    public Environment Globals = new();
    public Environment Environment;
    private readonly Dictionary<Expr, int> _locals = new();

    public Interpreter()
    {
        Environment = Globals;
        Globals.Define("uwuify", new NativeFunction("uwuify", (_, s) =>
        {
            var str = UwU.uwuify(Stringify(s[0])!);
            return str;
        }, 1));
        Globals.Define("emoticon", new NativeFunction("emoticon", (_, __) =>
        {
            var emoticons = new[] { ":3", "c:", "^^", "UwU", "OwO", "^~^" };
            var rand = new Random();
            var i = (int)Math.Floor(rand.NextDouble() * emoticons.Length);
            return " " + emoticons[i];
        }));
        Globals.Define("inpwut", new NativeFunction("inpwut", (_, s) =>
        {
            if (s[0] != null) Console.Write(s[0]);
            var input = Console.ReadLine();
            return input;
        }, 1));
        Globals.Define("typeof", new NativeFunction("typeof", (_, n) =>
        {
            var type = n[0];
            return WhichType(type);
        }, 1));
    }
    public void Interpret(List<Stmt?> statements)
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
            core.RuntimeError(error);
        }
    }

    public string? Stringify(object? obj)
    {
        if (obj == null) return "nuww";
        if (obj is TokenType type)
        {
            switch (type)
            {
                case TokenType.LiteralNumber:
                    return "Numbew";
                case TokenType.LiteralString:
                    return "Stwing";
                case TokenType.LiteralBool:
                    return "Boowean";
                case TokenType.LiteralNull:
                    return "nuww";
            }
        }
        if (obj is double)
        {
            string text = obj.ToString() ?? string.Empty;
            if (text.EndsWith(".0"))
            {
                text = text.Substring(0, text.Length - 2);
            }

            return text;
        }

        return obj.ToString();
    }

    public object? VisitExpressionAssign(Expr.Assign assign)
    {
        object? value = Evaluate(assign.value);
        int distance = -1;
        _locals.TryGetValue(assign, out distance);

        if (distance >= 0)
        {
            Environment.AssignAt(distance, assign.name, value);
        }
        else
        {
            Globals.Assign(assign.name, value);
        }
        
        Environment.Assign(assign.name, value);
        return value;
    }

    public object? VisitExpressionBinary(Expr.Binary? binary)
    {
        if (binary == null) return null;
        
        var left = Evaluate(binary.left);
        var right = Evaluate(binary.right);
        
        if (left == null || right == null) return null;
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
            case TokenType.Mod:
                CheckNumberOperands(binary.op, left, right);
                return (double)left % (double)right;

        }
        // Unreachable
        return null;
    }

    public object? VisitExpressionCall(Expr.Call call)
    {
        object? callee = Evaluate(call.callee);

        List<object?> arguments = new List<object?>();
        if (call.args != null)
            foreach (var arg in call.args)
            {
                arguments.Add(Evaluate(arg));
            }

        if (callee is ICallable func)
        {
            if (arguments.Count != func.Arity())
            {
                throw new RuntimeError(call.paren,
                    "Expected " + func.Arity() + " arguments but got " + arguments.Count + ".");
            }
            return func.Call(this, arguments);
        }
        else
        {
            throw new RuntimeError(call.paren, "Can only call functions and classes.");
        }
    }
    
    private TokenType? WhichType(object? type)
    {
        return type switch
        {
            null => TokenType.LiteralNull,
            string => TokenType.LiteralString,
            bool => TokenType.LiteralBool,
            double => TokenType.LiteralNumber,
            _ => null
        };
    }

    private void CheckNumberOperands(Token op, object? left, object? right)
    {
        if (left is double && right is double) return;
        throw new RuntimeError(op, "Operands must be numbers");
    }

    private bool IsEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a.Equals(b);
    }
    
    public object? VisitExpressionGrouping(Expr.Grouping grouping)
    {
        return Evaluate(grouping.expr);
    }

    private object? Evaluate(Expr expr)
    {
        var r = expr.Accept(this);
        return r;
    }

    private void Execute(Stmt? stmt)
    {
        stmt?.Accept(this);
    }

    public void Resolve(Expr expr, int depth)
    {
        _locals.Add(expr, depth);
    }

    public void ExecuteBlock(List<Stmt?> statements, Environment environment)
    {
        Environment previous = this.Environment;
        try
        {
            this.Environment = environment;
            foreach (var stmt in statements)
            {
                Execute(stmt);
            }
        }
        finally
        {
            this.Environment = previous;
        }
    }

    public object? VisitExpressionLiteral(Expr.Literal literal)
    {
        return literal.value;
    }

    public object? VisitExpressionLogical(Expr.Logical logical)
    {
        object? left = Evaluate(logical.left);

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

    public object? VisitExpressionUnary(Expr.Unary unary)
    {
        var right = Evaluate(unary.expr);

        if (right == null) return null;

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

    public object? VisitExpressionVariable(Expr.Variable variable)
    {
        // return Environment.Get(variable.name);
        return LookUpVariable(variable.name, variable);
    }

    private object LookUpVariable(Token name, Expr expr)
    {
        int distance = -1;
        _locals.TryGetValue(expr, out distance);
        if (distance >= 0)
        {
            return Environment.GetAt(distance, name.lexeme);
        }
        else
        {
            return Globals.Get(name);
        }
    }

    public void VisitStmtExpression(Stmt.Expression stmt)
    {
        Evaluate(stmt.expression);
    }

    public void VisitStmtFunc(Stmt.Func func)
    {
        Function function = new Function(func);
        Environment.Define(func.name.lexeme, function);
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
        object? value = Evaluate(print.expression);
        if (value != null)
        {
            core.PrintWorker.Invoke(Stringify(value));
        }
        // Console.WriteLine(value);
    }

    public void VisitStmtReturn(Stmt.Return rtrn)
    {
        object? value = null;
        if (rtrn.value != null) value = Evaluate(rtrn.value);

        throw new Return(value);
    }

    public void VisitStmtVar(Stmt.Var var)
    {
        object? value = Evaluate(var.initializer);
        Environment.Define(var.name.lexeme, value);
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
        ExecuteBlock(block.statements, new Environment(Environment));
    }

    private void CheckNumberOperand(Token op, object? operand)
    {
        if (operand is double) return;
        throw new RuntimeError(op, "Operand must be a number");
    }

    private bool IsTruthy(object? obj)
    {
        if (obj == null) return false;
        if (obj is Expr.Literal && ((Expr.Literal)obj).value is bool b1) return b1;
        if (obj is bool b) return b;
        return true;
    }
}