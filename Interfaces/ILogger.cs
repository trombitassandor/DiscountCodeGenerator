namespace DiscountCode;

public interface ILogger
{
    public void WriteLine(string source, string message);

    public void Write(string source, string message);
    
    protected static string TimeStamp => 
        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
}