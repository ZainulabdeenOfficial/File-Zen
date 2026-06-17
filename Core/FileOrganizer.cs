using System.IO;
using FileZenPro.AI;

namespace FileZenPro.Core;

public class FileOrganizer
{
    private NamingEngine naming = new NamingEngine();

    public void Organize(string path)
    {
        if (!Directory.Exists(path)) return;

        foreach (var file in Directory.GetFiles(path))
        {
            string ext = Path.GetExtension(file).ToLower();

            string folder = ext switch
            {
                ".jpg" or ".png" => "Images",
                ".mp4" => "Videos",
                ".pdf" => "PDFs",
                ".zip" => "Archives",
                ".exe" => "Installers",
                _ => "Others"
            };

            string target = Path.Combine(path, folder);
            Directory.CreateDirectory(target);

            string newName = naming.GenerateSmartName(file);

            File.Move(file, Path.Combine(target, newName), true);
        }
    }
}