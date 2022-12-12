using System.IO.Compression;
using System.Linq.Expressions;
using nyasharp.AST;

namespace nyasharp.Interpreter;

public class Interpreter : Visitor<Object>
{
    public void interpret(Expr expression)
    {
        try
        {
            object value = Evaluate(expression);
            Console.WriteLine(Stringify(value));
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

    public object VisitExpressionLiteral(Expr.Literal literal)
    {
        return literal.value;
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

    private void CheckNumberOperand(Token op, object operand)
    {
        if (operand is double) return;
        throw new RuntimeError(op, "Operand must be a number");
    }

    private bool IsTruthy(object obj)
    {
        if (obj == null) return false;
        if (obj is bool b) return b;
        return true;
    }
}