using System.Xml.Linq;
using EkycService.Application.DTOs.Response;
using EkycService.Application.Interfaces;

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
            UidToken = uidData.Attribute("tkn")?.Value, // ADD THIS LINE
            Name = poi?.Attribute("name")?.Value,
            Dob = poi?.Attribute("dob")?.Value,
            Gender = poi?.Attribute("gender")?.Value,
            Phone = poi?.Attribute("phone")?.Value,
            Email = poi?.Attribute("email")?.Value,
            Address = string.Join(", ",
                poa?.Attributes().Select(a => a.Value) ?? []),
            Photo = photo is not null
                ? Convert.FromBase64String(photo.Value) : null
        };
    }
}
