using System.IO;
namespace FileZenPro.Safety;

public class SafeDelete
{
    public void Delete(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}
