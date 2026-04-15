using Microsoft.Extensions.Configuration;
using Serilog.Configuration;
using Serilog.Sinks.Http;
using System.Net.Http.Headers;

namespace EkycService.Infrastructure.Logging;

public class OpenObserveHttpClient : IHttpClient
{
    private readonly HttpClient _client;

    public OpenObserveHttpClient()
    {
        _client = new HttpClient();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Basic",
                "YWRtaW5AZXhhbXBsZS5jb206dmpSS1doWlB4WkdxNXpNZw=="
            );
    }

    // REQUIRED METHOD 1
    public void Configure(IConfiguration configuration)
    {
        // Not needed → keep empty
    }

    // REQUIRED METHOD 2 (IMPORTANT CHANGE)
    public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
    {
        using var content = new StreamContent(contentStream);

        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return await _client.PostAsync(requestUri, content);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}