namespace nyasharp.AST;

public abstract class Expr
{
    public interface Visitor<out T>
    {
        T? VisitExpressionAssign(Assign assign);
        T? VisitExpressionBinary(Binary? binary);
        T? VisitExpressionCall(Expr.Call call);
        T? VisitExpressionGrouping(Expr.Grouping grouping);
        T? VisitExpressionLiteral(Expr.Literal literal);
        T? VisitExpressionLogical(Expr.Logical logical);
        T? VisitExpressionUnary(Expr.Unary unary);
        T? VisitExpressionVariable(Variable variable);
    }

    public abstract T? Accept<T>(Visitor<T?> visitor);


    public class Assign : Expr
    {
        public readonly Token name;
        public readonly Expr value;

        public Assign(Token name, Expr value)
        {
            this.name = name;
            this.value = value;
        }
        
        public override T? Accept<T>(Visitor<T?> visitor) where T : default
        {
            return visitor.VisitExpressionAssign(this);
        }
    }
    public class Binary : Expr
    {
        public readonly Expr left;
        public readonly Token op;
        public readonly Expr right;
        
        public Binary(Expr left, Token op, Expr right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        public override T? Accept<T>(Visitor<T?> visitor) where T : default
        {
            return visitor.VisitExpressionBinary(this);
        }
    }

    public class Call : Expr
    {
        public readonly Expr callee;
        public readonly Token paren;
        public readonly List<Expr>? args;

        public Call(Expr callee, Token paren, List<Expr>? args)
        {
            this.callee = callee;
            this.paren = paren;
            this.args = args;
        }
        public override T? Accept<T>(Visitor<T?> visitor) where T : default
        {
            return visitor.VisitExpressionCall(this);
        }
    }

    public class Grouping : Expr
    {
        public readonly Expr expr;
        
        public Grouping(Expr expr)
        {
            this.expr = expr;
        }
        
        public override T? Accept<T>(Visitor<T?> visitor) where T : default
        {
            return visitor.VisitExpressionGrouping(this);
        }
        
    }

    public class Literal : Expr
    {
        public readonly object? value;
        
        public Literal(object? value)
        {
            this.value = value;
        }
        
        public override T? Accept<T>(Visitor<T?> visitor) where T : default
        {
            return visitor.VisitExpressionLiteral(this);
        }
    }

    public class Logical : Expr
    {
        public readonly Expr left;
        public readonly Token op;
        public readonly Expr right;

        public Logical(Expr left, Token op, Expr right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }
        
        public override T? Accept<T>(Visitor<T?> visitor) where T : default
        {
            return visitor.VisitExpressionLogical(this);
        }
    }

    public class Unary : Expr
    {
        public readonly Token op; 
        public readonly Expr expr;

        public Unary(Token op, Expr expr)
        {
            this.op = op;
            this.expr = expr;
        }
        
        public override T? Accept<T>(Visitor<T?> visitor) where T : default
        {
            return visitor.VisitExpressionUnary(this);
        }
    }

    public class Variable : Expr
    {
        public readonly Token name;

        public Variable(Token name)
        {
            this.name = name;
        }
        public override T? Accept<T>(Visitor<T?> visitor) where T : default
        {
            return visitor.VisitExpressionVariable(this);
        }
    }
}