namespace EkycService.Application.Interfaces;

public interface IUidaiDecryptionService
{
    string ExtractAndDecode(string uidaiResponseXml);
}