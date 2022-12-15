namespace nyasharp;

public class Result
{
    public object? Value { get; set; }

    public List<string> Errors { get; }

    public Result()
    {
        this.Value = null;
        this.Errors = new List<string>();
    }
}