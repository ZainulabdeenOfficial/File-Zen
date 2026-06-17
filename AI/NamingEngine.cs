using System.IO;
using OpenAI.Chat;

namespace FileZenPro.AI;

public class NamingEngine
{
    public async Task<string> GenerateName(string fileName)
    {
        string fallbackName = Path.GetFileNameWithoutExtension(fileName);
        string? apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return fallbackName;
        }

        ChatClient client = new("gpt-4.1-mini", apiKey);
        ChatCompletion result = await client.CompleteChatAsync(
            new UserChatMessage($"Rename this file professionally. Return only the file name: {fileName}")
        );

        return result.Content.FirstOrDefault()?.Text?.Trim() ?? fallbackName;
    }
}
