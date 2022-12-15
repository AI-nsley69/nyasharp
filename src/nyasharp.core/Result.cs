namespace nyasharp;

public class Result
{
    public List<object> Value { get; set; }

    public List<string> Errors { get; }

    public bool Print;

    public Result()
    {
        this.Value = new List<object>();
        this.Errors = new List<string>();
    }

    public void Update(object? value, bool shouldPrint)
    {
        this.Print = shouldPrint;
    }
}