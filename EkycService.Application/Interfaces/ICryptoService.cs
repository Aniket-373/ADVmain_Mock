using EkycService.Application.DTOs.Response;

namespace EkycService.Application.Interfaces;

public interface ICryptoService
{
    GcmResult EncryptGcm(byte[] plain, byte[] dek);
    byte[] DecryptGcm(string cipher, byte[] dek, string iv, string tag);

    CbcResult EncryptCbc(string plain);
    string DecryptCbc(string cipher, string iv);

    string ComputeHmac(string input);
}