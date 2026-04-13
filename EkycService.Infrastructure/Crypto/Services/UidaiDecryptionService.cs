using System.Text;
using System.Xml.Linq;
using EkycService.Application.Interfaces;

namespace EkycService.Infrastructure.Crypto.Services;

public class UidaiDecryptionService : IUidaiDecryptionService
{
    public string ExtractAndDecode(string uidaiResponseXml)
    {
        var doc = XDocument.Parse(uidaiResponseXml);

        var kycResBase64 = doc.Descendants("kycRes").FirstOrDefault()?.Value;

        if (string.IsNullOrEmpty(kycResBase64))
            throw new Exception("kycRes not found");

        var decodedBytes = Convert.FromBase64String(kycResBase64);

        return Encoding.UTF8.GetString(decodedBytes);
    }
}