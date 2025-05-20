using DiscountCode;

public class ConsoleLogger : ILogger
{
    public void WriteLine(string source, string message)
    {
        Console.WriteLine($"[{TimeStamp}] [{source}] {message}");
    }
    
    public void Write(string source, string message)
    {
        Console.Write($"[{TimeStamp}] [{source}] {message}");
    }
    
    private static string TimeStamp => 
        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
}