using EkycService.Application.DTOs.Request;
using EkycService.Application.DTOs.Response;
using EkycService.Application.Interfaces;
using System.Text;

namespace EkycService.Application.Services;

public class EkycService : IEkycService
{
    private readonly IVaultRepository _vault;
    private readonly IDemographicsRepository _demo;
    private readonly IEkycSaveRepository _saveRepo;
    private readonly IAuditRepository _audit;
    private readonly ICryptoService _crypto;
    private readonly IHsmProvider _hsm;
    private readonly IKycResParser _parser;
    private readonly IUidaiDecryptionService _uidai;

    public EkycService(
        ICryptoService crypto, IHsmProvider hsm,
        IKycResParser parser, IEkycSaveRepository saveRepo,
        IUidaiDecryptionService uidai, IVaultRepository vault,
        IDemographicsRepository demo, IAuditRepository audit)
    {
        _crypto = crypto; _hsm = hsm;
        _parser = parser; _saveRepo = saveRepo;
        _uidai = uidai; _vault = vault;
        _demo = demo; _audit = audit;
    }

    public async Task<SaveEkycResponse> SaveAsync(SaveEkycRequest request)
    {
        // Generate ONE ref_id here. This flows to BOTH app_db (via SP) and vault.
        var refId = Guid.NewGuid();

        try
        {
            // Step 1 — Parse the UIDAI response
            var decryptedXml = _uidai.ExtractAndDecode(request.UidaiResponseXml);
            var parsed = _parser.Parse(decryptedXml);

            // Step 2 — Generate a fresh DEK for this record
            var dek = await _hsm.GenerateDekAsync();

            // Step 3 — Encrypt the full KycRes XML for vault (GCM)
            var encryptedXml = _crypto.EncryptGcm(
                Encoding.UTF8.GetBytes(decryptedXml), dek);

            // Step 4 — Encrypt the actual Aadhaar UID for vault (GCM)
            // parsed.Uid = the actual 12-digit Aadhaar number
            // This goes ONLY to the vault. Never to app_db.
            var uidVaultEnc = _crypto.EncryptGcm(
                Encoding.UTF8.GetBytes(parsed.Uid ?? string.Empty), dek);

            // Step 5 — Wrap DEK under KEK inside HSM
            var wrappedDek = await _hsm.WrapDekAsync(dek);
            Array.Clear(dek, 0, dek.Length); // Zero DEK from memory immediately

            // Step 6 — Encrypt PII fields for app_db (CBC under PII key)
            var nameEnc = _crypto.EncryptCbc(parsed.Name ?? string.Empty);
            var addressEnc = _crypto.EncryptCbc(parsed.Address ?? string.Empty);

            CbcResult? phoneEnc = null;
            if (!string.IsNullOrEmpty(parsed.Phone))
                phoneEnc = _crypto.EncryptCbc(parsed.Phone);

            CbcResult? emailEnc = null;
            if (!string.IsNullOrEmpty(parsed.Email))
                emailEnc = _crypto.EncryptCbc(parsed.Email);

            // Step 7 — Encrypt uid_token for app_db
            // parsed.UidToken = the tkn attribute from UIDAI (agency-specific pseudonym)
            // If tkn is not in your test XML, fall back to HMAC of Uid temporarily
            var uidToken = parsed.UidToken ?? _crypto.ComputeHmac(parsed.Uid ?? string.Empty);
            var uidTokenEnc = _crypto.EncryptCbc(uidToken);
            var uidHmac = _crypto.ComputeHmac(uidToken);

            // Step 8 — Write to vault FIRST (aadhaar_vault_db)
            // Uses the SAME refId generated above
            await _vault.SaveAsync(
                refId,
                encryptedXml.CipherB64, encryptedXml.IvHex, encryptedXml.TagHex,
                wrappedDek,
                uidVaultEnc.CipherB64, uidVaultEnc.IvHex, uidVaultEnc.TagHex);

            // Step 9 — Write to app_db via stored procedure
            // The SP now receives p_ref_id and uses it directly
            // So app_db ref_id == vault ref_id == the refId we generated above
            await _saveRepo.SaveAsync(
                refId,              // THE KEY: same ref_id goes to SP
                uidTokenEnc.CipherB64,  // uid_token_cipher (pseudonym, not Aadhaar UID)
                uidTokenEnc.IvHex,      // iv_token
                uidHmac,                // uid_token_hmac
                nameEnc.CipherB64, nameEnc.IvHex,
                DateTime.Parse(parsed.Dob ?? "2000-01-01"),
                parsed.Gender ?? "M",
                phoneEnc?.CipherB64, phoneEnc?.IvHex,
                emailEnc?.CipherB64, emailEnc?.IvHex,
                addressEnc.CipherB64, addressEnc.IvHex);

            // Step 10 — Log success
            await _audit.LogAsync(refId, "SAVE", "SUCCESS");

            return new SaveEkycResponse
            {
                RefId = refId.ToString(),
                Status = "SUCCESS"
            };
        }
        catch (Exception ex)
        {
            // Best effort audit log — may fail if ekyc_record insert also failed
            try { await _audit.LogAsync(refId, "SAVE", "FAILURE"); }
            catch { /* suppress audit failure — do not hide original error */ }
            throw;
        }
    }

    public async Task<string?> GetRawXmlAsync(Guid refId)
    {
        var data = await _vault.GetByRefIdAsync(refId);

        if (data == null)
            return null;

        var dek = await _hsm.UnwrapDekAsync(data.WrappedDek);

        var xmlBytes = _crypto.DecryptGcm(data.XmlCipher, dek, data.IvXml, data.TagXml);
        Array.Clear(dek, 0, dek.Length);

        return Encoding.UTF8.GetString(xmlBytes);
    }

    public async Task<object> GetDemographicsAsync(Guid refId)
    {
        var data = await _demo.GetByRefIdAsync(refId);
        if (data == null)
            return null;

        var name = string.IsNullOrEmpty(data.NameCipher) ? null : _crypto.DecryptCbc(data.NameCipher, data.IvName);
        var address = string.IsNullOrEmpty(data.AddressCipher) ? null : _crypto.DecryptCbc(data.AddressCipher, data.IvPoa);

        string? phone = data.PhoneCipher is not null && data.IvPhone is not null
            ? _crypto.DecryptCbc(data.PhoneCipher, data.IvPhone) : null;
        string? email = data.EmailCipher is not null && data.IvEmail is not null
            ? _crypto.DecryptCbc(data.EmailCipher, data.IvEmail) : null;

        return new
        {
            RefId = refId,
            Name = name,
            Dob = data.Dob.ToString("yyyy-MM-dd"),
            Gender = data.Gender.ToString(),
            Phone = phone,
            Email = email,
            Address = address
        };
    }
}
