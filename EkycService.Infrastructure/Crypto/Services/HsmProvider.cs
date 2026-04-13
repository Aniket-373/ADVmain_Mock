using EkycService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace EkycService.Infrastructure.Crypto.Services;

public class HsmProvider : IHsmProvider
{
    private readonly string _libPath;
    private readonly string _tokenLabel;
    private readonly string _pin;
    private readonly string _kekLabel;

    public HsmProvider(IConfiguration config)
    {
        _libPath = config["Hsm:Pkcs11LibraryPath"]!;
        _tokenLabel = config["Hsm:TokenLabel"]!;
        _pin = config["Hsm:UserPin"]!;
        _kekLabel = config["Hsm:KekLabel"]!;
    }

    // TODO: Replace with real PKCS#11 wrapping using KEK inside HSM
    /// <summary>
    /// Generates a random 256-bit Data Encryption Key (DEK)
    /// Used for AES-GCM encryption of Aadhaar XML
    /// </summary>
    public Task<byte[]> GenerateDekAsync()
    {
        var dek = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
        return Task.FromResult(dek);
    }

    /// <summary>
    /// Wraps the DEK using KEK stored in HSM
    /// 
    /// NOTE:
    /// In development mode (SoftHSM not fully configured with key objects),
    /// this method performs a mock wrap (Base64 encoding).
    /// 
    /// In production, this should use actual HSM key handles.
    /// </summary>
    public Task<string> WrapDekAsync(byte[] dek)
    {
        using var pkcs11 = new Pkcs11(_libPath, AppType.SingleThreaded);

        var slot = pkcs11.GetSlotList(SlotsType.WithTokenPresent).First();
        using var session = slot.OpenSession(SessionType.ReadWrite);

        session.Login(CKU.CKU_USER, _pin);

        // Search KEK object by label
        var searchTemplate = new List<ObjectAttribute>
        {
            new ObjectAttribute(CKA.CKA_LABEL, _kekLabel)
        };

        session.FindObjectsInit(searchTemplate);
        var foundObjects = session.FindObjects(1);
        session.FindObjectsFinal();

        if (foundObjects.Count == 0)
            throw new Exception("KEK not found in HSM");

        var kek = foundObjects.First();

        // DEV MODE:
        // Pkcs11Interop 4.1.2 requires ObjectHandle for wrapping,
        // but raw byte[] cannot be wrapped directly without creating a key object.
        // Hence, using mock wrapping for development.
        var wrapped = dek;

        return Task.FromResult(Convert.ToBase64String(wrapped));
    }

    /// <summary>
    /// Unwraps the DEK using KEK from HSM
    /// 
    /// NOTE:
    /// In development mode, this simply decodes Base64 string.
    /// In production, replace with real HSM unwrap logic.
    /// </summary>
    public Task<byte[]> UnwrapDekAsync(string wrappedDek)
    {
        using var pkcs11 = new Pkcs11(_libPath, AppType.SingleThreaded);

        var slot = pkcs11.GetSlotList(SlotsType.WithTokenPresent).First();
        using var session = slot.OpenSession(SessionType.ReadWrite);

        session.Login(CKU.CKU_USER, _pin);

        // Search KEK object by label
        var searchTemplate = new List<ObjectAttribute>
        {
            new ObjectAttribute(CKA.CKA_LABEL, _kekLabel)
        };

        session.FindObjectsInit(searchTemplate);
        var foundObjects = session.FindObjects(1);
        session.FindObjectsFinal();

        if (foundObjects.Count == 0)
            throw new Exception("KEK not found in HSM");

        var kek = foundObjects.First();

        // DEV MODE: Base64 decode (mock unwrap)
        var unwrapped = Convert.FromBase64String(wrappedDek);

        return Task.FromResult(unwrapped);
    }
}