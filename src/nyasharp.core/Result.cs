namespace nyasharp;

public class Result
{
    public object? Value { get; set; }

    public List<string> Errors { get; }

    public bool Print;

    public Result()
    {
        this.Value = null;
        this.Errors = new List<string>();
    }

    public void Update(object? value, bool shouldPrint)
    {
        this.Value = value;
        this.Print = shouldPrint;
    }
}