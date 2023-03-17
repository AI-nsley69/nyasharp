using nyasharp.AST;

namespace nyasharp.Resolver;

public class Resolver : Expr.Visitor<string>, Stmt.Visitor
{

    private readonly Interpreter.Interpreter _interpreter;
    private readonly Stack<Dictionary<String, bool>> _scopes = new();
    private FunctionType currentFunc = FunctionType.None;

    public Resolver(Interpreter.Interpreter interpreter)
    {
        this._interpreter = interpreter;
    }

    // Resolve all statements
    public void VisitStmtBlock(Stmt.Block block)
    {
        BeginScope();
        Resolve(block.statements);
        EndScope();
    }

    public void VisitStmtExpression(Stmt.Expression stmt)
    {
        Resolve(stmt.expression);
    }

    public void VisitStmtFunc(Stmt.Func func)
    {
        Declare(func.name);
        Define(func.name);
        
        ResolveFunction(func, FunctionType.Function);
    }

    public void VisitStmtIf(Stmt.If stmt)
    {
        Resolve(stmt.condition);
        Resolve(stmt.thenBranch);
        if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
    }

    public void VisitStmtPrint(Stmt.Print print)
    {
        Resolve(print.expression);
    }

    public void VisitStmtReturn(Stmt.Return stmt)
    {
        if (currentFunc == FunctionType.None) core.Error(stmt.keyword, "Can't return from top level code.");
        if (stmt.value == null) return;
        Resolve(stmt.value);
    }

    public void VisitStmtVar(Stmt.Var? var)
    {
        Declare(var.name);
        if (var.initializer != null)
        {
            Resolve(var.initializer);
        }

        Define(var.name);
    }

    public void VisitStmtWhile(Stmt.While stmt)
    {
        Resolve(stmt.condition);
        Resolve(stmt.body);
    }
    
    // Resolve all expressions

    public string? VisitExpressionAssign(Expr.Assign assign)
    {
        Resolve(assign.value);
        ResolveLocal(assign, assign.name);
        return null;
    }

    public string? VisitExpressionBinary(Expr.Binary? binary)
    {
        Resolve(binary.left);
        Resolve(binary.right);
        return null;
    }

    public string? VisitExpressionCall(Expr.Call call)
    {
        Resolve(call.callee);

        foreach (var arg in call.args)
        {
            Resolve(arg);
        }

        return null;
    }

    public string? VisitExpressionGrouping(Expr.Grouping grouping)
    {
        Resolve(grouping.expr);
        return null;
    }

    public string? VisitExpressionLiteral(Expr.Literal literal)
    {
        return null;
    }

    public string? VisitExpressionLogical(Expr.Logical logical)
    {
        Resolve(logical.left);
        Resolve(logical.right);
        return null;
    }

    public string? VisitExpressionUnary(Expr.Unary unary)
    {
        Resolve(unary.expr);
        return null;
    }
    
    public string? VisitExpressionVariable(Expr.Variable var)
    {
        if (_scopes.Count > 0)
        {
            if (_scopes.Peek().TryGetValue(var.name.lexeme, out var value))
            {
                if (!value) core.Error(var.name, "Can't read local variable in its own initializer");   
            }
        }   

        ResolveLocal(var, var.name);
        return null;
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        for (var i = _scopes.Count - 1; i >= 0; i--)
        {
            if (!_scopes.ElementAt(i).ContainsKey(name.lexeme)) continue;
            _interpreter.Resolve(expr, _scopes.Count - 1 - i);
            return;
        }
    }

    public void Resolve(List<Stmt?> statements)
    {
        foreach (var stmt in statements)
        {
            Resolve(stmt);
        }
    }

    private void Resolve(Stmt? stmt)
    {
        stmt?.Accept(this);
    }

    private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    private void ResolveFunction(Stmt.Func func, FunctionType type)
    {
        FunctionType enclosingFunction = currentFunc;
        currentFunc = type;
        
        BeginScope();
        foreach (var param in func.parameters)
        {
            Declare(param);
            Define(param);
        }
        
        Resolve(func.body);
        EndScope();

        currentFunc = enclosingFunction;
    }

    private void BeginScope()
    {
        _scopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        _scopes.Pop();
    }

    private void Declare(Token name)
    {
        if (_scopes.Count == 0) return;
        var scope = _scopes.Peek();
        if (scope.ContainsKey(name.lexeme)) core.Error(name, "Already a variable with this name inside a scope.");
        scope.TryAdd(name.lexeme, false);
    }

    private void Define(Token name)
    {
        if (_scopes.Count == 0) return;
        _scopes.Peek()[name.lexeme] = true;
    }
}