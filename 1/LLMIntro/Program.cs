using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var client = new HttpClient();
        var requestData = new { model = "mistral", prompt = "Explain what a transformer model is." };
        var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

        var response = await client.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("http://localhost:11434/api/generate"),
            Content = content
        }, HttpCompletionOption.ResponseHeadersRead); // Read the response **as a stream**

        using var responseStream = await response.Content.ReadAsStreamAsync();
        using var streamReader = new System.IO.StreamReader(responseStream);

        string? line;
        string fullResponse = ""; // Variable to accumulate the response

        while ((line = await streamReader.ReadLineAsync()) != null)
        {
            try
            {
                var json = JsonDocument.Parse(line);
                if (json.RootElement.TryGetProperty("response", out var responseText))
                {
                    fullResponse += responseText.GetString(); // Append each chunk
                }
            }
            catch (JsonException)
            {
                // Skip malformed JSON lines (sometimes streaming data may have incomplete chunks)
                continue;
            }
        }

        Console.WriteLine("\n✅ Response from LLM:");
        Console.WriteLine(fullResponse); // Print the full accumulated response
    }
}
