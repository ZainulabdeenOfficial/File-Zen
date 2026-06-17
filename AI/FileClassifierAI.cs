namespace FileZenPro.AI;

public class FileClassifierAI
{
    public string Classify(string ext)
    {
        return ext switch
        {
            ".jpg" or ".png" => "Image",
            ".mp4" => "Video",
            ".pdf" => "Document",
            _ => "Other"
        };
    }
}