using System.Security.Cryptography;
using System.Text;
using EkycService.Application.Interfaces;
using EkycService.Application.DTOs.Response;
using Microsoft.Extensions.Configuration;

namespace EkycService.Infrastructure.Crypto.Services;

public class CryptoService : ICryptoService
{
    private readonly byte[] _piiKey;
    private readonly byte[] _hmacKey;

    public CryptoService(IConfiguration config)
    {
        var piiHex = config["Crypto:PiiKeyHex"];
        var hmacHex = config["Crypto:HmacKeyHex"];

        _piiKey = Convert.FromHexString(piiHex!);
        _hmacKey = Convert.FromHexString(hmacHex!);
    }

    // AES-256-GCM (Vault)
    public GcmResult EncryptGcm(byte[] plain, byte[] dek)
    {
        var iv = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var cipher = new byte[plain.Length];

        using var aes = new AesGcm(dek);
        aes.Encrypt(iv, plain, cipher, tag);

        return new GcmResult(
            Convert.ToBase64String(cipher),
            Convert.ToHexString(iv),
            Convert.ToHexString(tag)
        );
    }

    public byte[] DecryptGcm(string cipher, byte[] dek, string iv, string tag)
    {
        var cipherBytes = Convert.FromBase64String(cipher);
        var ivBytes = Convert.FromHexString(iv);
        var tagBytes = Convert.FromHexString(tag);

        var plain = new byte[cipherBytes.Length];

        using var aes = new AesGcm(dek);
        aes.Decrypt(ivBytes, cipherBytes, tagBytes, plain);

        return plain;
    }

    // AES-256-CBC (PII)
    public CbcResult EncryptCbc(string plain)
    {
        var iv = RandomNumberGenerator.GetBytes(16);
        var plainBytes = Encoding.UTF8.GetBytes(plain);

        using var aes = Aes.Create();
        aes.Key = _piiKey;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var encryptor = aes.CreateEncryptor();
        var cipher = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return new CbcResult(
            Convert.ToBase64String(cipher),
            Convert.ToHexString(iv)
        );
    }

    public string DecryptCbc(string cipher, string iv)
    {
        var cipherBytes = Convert.FromBase64String(cipher);
        var ivBytes = Convert.FromHexString(iv);

        using var aes = Aes.Create();
        aes.Key = _piiKey;
        aes.IV = ivBytes;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var decryptor = aes.CreateDecryptor();
        var plain = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plain);
    }

    // HMAC-SHA256 (Dedup)
    public string ComputeHmac(string input)
    {
        using var hmac = new HMACSHA256(_hmacKey);
        var bytes = Encoding.UTF8.GetBytes(input);

        return Convert.ToHexString(hmac.ComputeHash(bytes));
    }
}