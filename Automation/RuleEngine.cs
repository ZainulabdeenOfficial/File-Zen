namespace FileZenPro.Automation;

public class RuleEngine
{
    public Dictionary<string, string> Rules = new()
    {
        { ".pdf", "Documents" },
        { ".jpg", "Images" }
    };
}