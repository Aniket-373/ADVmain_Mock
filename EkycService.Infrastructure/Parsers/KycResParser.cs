using System.Xml.Linq;
using EkycService.Application.DTOs.Response;
using EkycService.Application.Interfaces;
using System.Text;

namespace EkycService.Infrastructure.Parsers;

public class KycResParser : IKycResParser
{
    public ParsedKycData Parse(string xml)
    {
        var doc = XDocument.Parse(xml);

        var uidData = doc.Descendants("UidData").First();

        var poi = uidData.Element("Poi");
        var poa = uidData.Element("Poa");
        var photo = uidData.Element("Pht");

        return new ParsedKycData
        {
            Uid = uidData.Attribute("uid")?.Value,

            Name = poi?.Attribute("name")?.Value,
            Dob = poi?.Attribute("dob")?.Value,
            Gender = poi?.Attribute("gender")?.Value,
            Phone = poi?.Attribute("phone")?.Value,
            Email = poi?.Attribute("email")?.Value,

            Address = string.Join(", ",
                poa?.Attributes().Select(a => a.Value) ?? []),

            Photo = photo != null
                ? Convert.FromBase64String(photo.Value)
                : null
        };
    }
}
