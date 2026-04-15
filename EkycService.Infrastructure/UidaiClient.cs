using EkycService.Application.Interfaces;
using Microsoft.Extensions.Configuration;


namespace EkycService.Infrastructure
{
    public class UidaiClient : IUidaiClient
    {
        private readonly HttpClient _http;

        public UidaiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _http.BaseAddress = new Uri(config["Uidai:BaseUrl"]);
        }

        public async Task<string> GetKycXmlAsync(string uid)
        {
            var xmlRequest = $"<Auth uid=\"{uid}\"></Auth>";

            var content = new StringContent(xmlRequest, System.Text.Encoding.UTF8, "application/xml");

            var response = await _http.PostAsync("/kyc/2.5", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"UIDAI returned error: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}