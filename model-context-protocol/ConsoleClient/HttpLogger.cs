/// <summary>
/// Logging handler you might want to use to see the HTTP traffic sent by SK to LLMs.
/// </summary>
public class HttpLogger : DelegatingHandler
{
    public static HttpClient GetHttpClient(bool isShowLogging = false)
    {
        var httpclient = isShowLogging
            ? new HttpClient(new HttpLogger(new HttpClientHandler()))
            : new HttpClient();

        httpclient.Timeout = TimeSpan.FromMinutes(5);

        return httpclient;
    }

    public HttpLogger(HttpMessageHandler innerHandler) : base(innerHandler) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine("\n\x1b[34mRequest Body:\x1b[0m");
        //Console.WriteLine(request.ToString());
        //Console.WriteLine();
        if (request.Content != null)
        {
            var content = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"\x1b[1;30m{content}\x1b[0m");
        }
        //Console.WriteLine();

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        Console.WriteLine("\n\x1b[32mResponse Body:\x1b[0m");
        //Console.WriteLine(response.ToString());
        //Console.WriteLine();
        if (response.Content != null)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"\x1b[1;30m{content}\x1b[0m");
        }
        //Console.WriteLine();

        return response;
    }
}