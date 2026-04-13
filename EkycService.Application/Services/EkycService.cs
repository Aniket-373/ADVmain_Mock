using EkycService.Application.DTOs.Request;
using EkycService.Application.DTOs.Response;
using EkycService.Application.Interfaces;
using System.Text;

namespace EkycService.Application.Services;

public class EkycService : IEkycService
{
    private readonly IVaultRepository _vault;
    private readonly IEkycRecordRepository _record;
    private readonly IDemographicsRepository _demo;
    private readonly IAuditRepository _audit;
    private readonly ICryptoService _crypto;
    private readonly IHsmProvider _hsm;
    private readonly IKycResParser _parser;
    private readonly IUidaiDecryptionService _uidai;

    public EkycService(
        ICryptoService crypto,
        IHsmProvider hsm,
        IKycResParser parser,
        IUidaiDecryptionService uidai,
        IVaultRepository vault,
        IEkycRecordRepository record,
        IDemographicsRepository demo,
        IAuditRepository audit)
    {
        _crypto = crypto;
        _hsm = hsm;
        _parser = parser;
        _uidai = uidai;
        _vault = vault;
        _record = record;
        _demo = demo;
        _audit = audit;
    }

    public async Task<SaveEkycResponse> SaveAsync(SaveEkycRequest request)
    {
        var refId = Guid.NewGuid();

        try
        {
            var decryptedXml = _uidai.ExtractAndDecode(request.UidaiResponseXml);

            var parsed = _parser.Parse(decryptedXml);

            var dek = await _hsm.GenerateDekAsync();

            var encryptedXml = _crypto.EncryptGcm(
                Encoding.UTF8.GetBytes(decryptedXml),
                dek);

            var nameEnc = _crypto.EncryptCbc(parsed.Name);

            var wrappedDek = await _hsm.WrapDekAsync(dek);

            var uidEnc = _crypto.EncryptGcm(
                Encoding.UTF8.GetBytes(parsed.Uid),
                dek);

            var uidToken = _crypto.ComputeHmac(parsed.Uid);

            var uidTokenEnc = _crypto.EncryptCbc(uidToken);

            await _record.SaveAsync(
                refId,
                uidTokenEnc.CipherB64,
                uidTokenEnc.IvHex
            );

            await _demo.SaveAsync(
                refId,
                nameEnc.CipherB64,
                nameEnc.IvHex);

            await _vault.SaveAsync(
                refId,
                encryptedXml.CipherB64,
                encryptedXml.IvHex,
                encryptedXml.TagHex,
                wrappedDek,
                uidEnc.CipherB64,
                uidEnc.IvHex,
                uidEnc.TagHex
            );

            await _audit.LogAsync(refId, "SAVE", "SUCCESS");

            return new SaveEkycResponse
            {
                RefId = refId.ToString(),
                Status = "SUCCESS"
            };
        }
        catch (Exception)
        {
            await _audit.LogAsync(refId, "SAVE", "FAILURE");
            throw;
        }
    }

    public async Task<string> GetRawXmlAsync(Guid refId)
    {
        var data = await _vault.GetByRefIdAsync(refId);

        if (data == null)
            throw new Exception("Record not found");

        // 🔐 Step 1 — Unwrap DEK
        var dek = await _hsm.UnwrapDekAsync(data.WrappedDek);

        // 🔐 Step 2 — Decrypt XML (GCM)
        var xmlBytes = _crypto.DecryptGcm(
            data.XmlCipher,
            dek,
            data.IvXml,
            data.TagXml);

        return Encoding.UTF8.GetString(xmlBytes);
    }

    public async Task<object> GetDemographicsAsync(Guid refId)
    {
        var data = await _demo.GetByRefIdAsync(refId);

        if (data == null)
            throw new Exception("Record not found");

        // 🔐 Decrypt Name
        var name = _crypto.DecryptCbc(data.NameCipher, data.IvName);

        return new
        {
            Name = name
        };
    }
}