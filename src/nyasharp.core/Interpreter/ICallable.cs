namespace nyasharp.Interpreter;

public interface ICallable
{
    public abstract int Arity();
    public abstract object? Call(Interpreter interpreter, List<object?> args);
}