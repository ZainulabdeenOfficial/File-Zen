using System.IO;

namespace FileZenPro.Core;

public class NamingEngine
{
    public string GenerateSmartName(string filePath)
    {
        string ext = Path.GetExtension(filePath);
        string folder = new DirectoryInfo(Path.GetDirectoryName(filePath)).Name;

        string date = DateTime.Now.ToString("yyyy");

        string baseName = folder switch
        {
            "Images" => $"Image_KarachiTrip_{date}",
            "Videos" => $"Video_Event_{date}",
            "PDFs" => $"Document_Study_{date}",
            _ => $"File_{date}"
        };

        return baseName + ext;
    }
}
