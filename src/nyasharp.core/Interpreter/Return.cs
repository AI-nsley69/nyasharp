namespace nyasharp.Interpreter;

public class Return : Exception
{
    public readonly object? value;

    public Return(object? value) : base("")
    {
        this.value = value;
    }
}