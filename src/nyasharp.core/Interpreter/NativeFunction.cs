namespace nyasharp.Interpreter;

public class NativeFunction : ICallable
{
    public readonly int arity;
    public readonly string name;

    private readonly Func<Interpreter, List<object?>, object?> action;

    public NativeFunction(string name, Func<Interpreter, List<object?>, object?> action, int arity = 0)
    {
        this.name = name;
        this.action = action;
        this.arity = arity;
    }
    int ICallable.Arity()
    {
        return arity;
    }

    public object? Call(Interpreter interpreter, List<object> args)
    {
        return action(interpreter, args);
    }

    public override string ToString()
    {
        return "<native fn " + name + ">";
    }
}