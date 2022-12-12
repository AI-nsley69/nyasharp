namespace nyasharp.AST;

public abstract class Expr
{
    public abstract T Accept<T>(Visitor<T> visitor);
    
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

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitExpressionBinary(this);
        }
    }

    public class Grouping : Expr
    {
        public readonly Expr expr;
        
        public Grouping(Expr expr)
        {
            this.expr = expr;
        }
        
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitExpressionGrouping(this);
        }
        
    }

    public class Literal : Expr
    {
        public readonly object value;
        
        public Literal(object value)
        {
            this.value = value;
        }
        
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitExpressionLiteral(this);
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
        
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitExpressionUnary(this);
        }
        
    }
}