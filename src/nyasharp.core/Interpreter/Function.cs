using nyasharp.AST;

namespace nyasharp.Interpreter;

public class Function : ICallable
{
    private readonly Stmt.Func declaration;

    public Function(Stmt.Func declaration)
    {
        this.declaration = declaration;
    }
    public int Arity()
    {
        return declaration.parameters.Count;
    }

    public object? Call(Interpreter interpreter, List<object?> args)
    {
        Environment environment = new Environment(interpreter.Globals);
        for (int i = 0; i < declaration.parameters.Count; i++)
        {
            environment.Define(declaration.parameters[i].lexeme, args[i]);
        }

        try
        {
            interpreter.ExecuteBlock(declaration.body, environment);
        }
        catch (Return returnValue)
        {
            return returnValue.value;
        }
        return null;
    }

    public override string ToString()
    {
        return "<fn " + declaration.name.lexeme + ">";
    }
}