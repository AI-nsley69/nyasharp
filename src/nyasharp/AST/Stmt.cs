using System.Diagnostics;

namespace nyasharp.AST;

public abstract class Stmt
{
    public interface Visitor
    {
        void VisitStmtExpression(Expression expr);
        void VisitStmtFunc(Func func);
        void VisitStmtIf(If ifStmt);
        void VisitStmtPrint(Print print);
        void VisitStmtReturn(Return rtrn);
        void VisitStmtVar(Var var);
        void VisitStmtWhile(While whileStmt);
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

    public class Func : Stmt
    {
        public readonly Token name;
        public readonly List<Token> parameters;
        public readonly List<Stmt> body;

        public Func(Token name, List<Token> parameters, List<Stmt> body)
        {
            this.name = name;
            this.parameters = parameters;
            this.body = body;
        }
        public override void Accept(Visitor visitor)
        {
            visitor.VisitStmtFunc(this);
        }
    }

    public class If : Stmt
    {
        public readonly Expr condition;
        public readonly Stmt thenBranch;
        public readonly Stmt? elseBranch;

        public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
        {
            this.condition = condition;
            this.thenBranch = thenBranch;
            this.elseBranch = elseBranch;
        }
        
        public override void Accept(Visitor visitor)
        {
            visitor.VisitStmtIf(this);
        }
    }

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

    public class Return : Stmt
    {
        public readonly Token keyword;
        public readonly Expr? value;

        public Return(Token keyword, Expr? value)
        {
            this.keyword = keyword;
            this.value = value;
        }
        public override void Accept(Visitor visitor)
        {
            visitor.VisitStmtReturn(this);
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

    public class While : Stmt
    {
        public readonly Expr condition;
        public readonly Stmt body;

        public While(Expr condition, Stmt body)
        {
            this.condition = condition;
            this.body = body;
        }

        public override void Accept(Visitor visitor)
        {
            visitor.VisitStmtWhile(this);
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