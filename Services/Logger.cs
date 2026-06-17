namespace FileZenPro.Services;

public class Logger
{
    public void Log(string message)
    {
        Console.WriteLine($"[FileZenPro] {DateTime.Now}: {message}");
    }
}