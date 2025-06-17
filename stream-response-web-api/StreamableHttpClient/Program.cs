using var client = new HttpClient
{
    Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite)
};

var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5036/stream");

using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
response.EnsureSuccessStatusCode();

using var stream = await response.Content.ReadAsStreamAsync();
using var reader = new StreamReader(stream);

while (!reader.EndOfStream)
{
    var line = await reader.ReadLineAsync();
    if (!string.IsNullOrWhiteSpace(line))
    {
        Console.WriteLine($"[Client] Received: {line}");
    }
}
