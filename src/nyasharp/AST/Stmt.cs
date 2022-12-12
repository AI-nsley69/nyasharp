namespace nyasharp.AST;

public abstract class Stmt
{
    public interface Visitor
    {
        void VisitStmtExpression(Expression expr);
        void VisitStmtPrint(Print print);
        void VisitStmtVar(Var var);
        void VisitStmtBlock(Block block);
    }
    public abstract void Accept(Visitor visitor);

    public class Expression : Stmt
    {
        public Expr expression;

        public Expression(Expr expression)
        {
            this.expression = expression;
        }

        public override void Accept(Visitor visitor)
        {
            visitor.VisitStmtExpression(this);
        }
    };

    public class Print : Stmt
    {
        public Expr expression;

        public Print(Expr expression)
        {
            this.expression = expression;
        }

        public override void Accept(Visitor visitor)
        {
            visitor.VisitStmtPrint(this);
        }
    }

    public class Var : Stmt
    {
        public Token name;
        public Expr initializer;

        public Var(Token name, Expr? initializer)
        {
            this.name = name;
            this.initializer = initializer ?? null;
        }

        public override void Accept(Visitor visitor)
        {
            visitor.VisitStmtVar(this);
        }
    }

    public class Block : Stmt
    {
        public readonly List<Stmt> statements;

        public Block(List<Stmt> statements)
        {
            this.statements = statements;
        } 
        public override void Accept(Visitor visitor)
        {
            visitor.VisitStmtBlock(this);
        }
    }
}