using Dapper;
using EkycService.Application.Interfaces;
using EkycService.Infrastructure.Database;

namespace EkycService.Infrastructure.Repositories;

public class EkycSaveRepository : IEkycSaveRepository
{
    private readonly DbConnectionFactory _factory;

    public EkycSaveRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task SaveAsync(
        Guid refId,          // Caller provides this — SP uses it, not gen_random_uuid
        string uidCipher,
        string uidIv,
        string uidHmac,
        string nameCipher,
        string nameIv,
        DateTime dob,
        string gender,
        string? phoneCipher,
        string? phoneIv,
        string? emailCipher,
        string? emailIv,
        string addressCipher,
        string addressIv)
    {
        using var conn = _factory.CreateAppConnection();

        // Generate txn_id here. In production this comes from the UIDAI response.
        var txnId = $"txn-{Guid.NewGuid()}";

        // Use @param syntax (Npgsql standard). Pass p_ref_id as FIRST argument.
        // OUT parameters (o_ref_id, o_error) are passed as NULL — we ignore the
        // return value here because we already know the ref_id we passed in.
        var sql = @"CALL app.sp_save_ekyc_record(
            @p_ref_id::uuid,
            @p_uid_token_cipher::text,
            @p_iv_token::varchar,
            @p_uid_token_hmac::varchar,
            @p_app_id::varchar,
            @p_txn_id::varchar,
            @p_uidai_code::varchar,
            @p_uidai_ts::timestamptz,
            @p_uidai_ttl_at::timestamptz,
            @p_uidai_actn::varchar,
            @p_uidai_err::varchar,
            @p_ac::varchar,
            @p_sa::varchar,
            @p_risk_category::varchar,
            @p_expires_at::timestamptz,
            @p_rekyc_due_at::timestamptz,
            @p_retain_until::timestamptz,
            @p_poi_name_cipher::text,
            @p_poi_dob::date,
            @p_poi_dob_type::char,
            @p_poi_gender::char,
            @p_poi_phone_cipher::text,
            @p_poi_email_cipher::text,
            @p_poa_cipher::text,
            @p_ldata_cipher::text,
            @p_pht_storage_ref::varchar,
            @p_pht_iv::varchar,
            @p_mobile_hash::varchar,
            @p_email_hash::varchar,
            @p_pii_key_ref::varchar,
            @p_pii_key_version::varchar,
            @p_iv_name::varchar,
            @p_iv_phone::varchar,
            @p_iv_email::varchar,
            @p_iv_poa::varchar,
            @p_iv_ldata::varchar,
            NULL::uuid,
            NULL::text
        )";

        await conn.ExecuteAsync(sql, new
        {
            p_ref_id = refId,          // THE FIX: pass caller's refId
            p_uid_token_cipher = uidCipher,
            p_iv_token = uidIv,
            p_uid_token_hmac = uidHmac,
            p_app_id = "app1",
            p_txn_id = txnId,
            p_uidai_code = "code1",
            p_uidai_ts = DateTime.UtcNow,
            p_uidai_ttl_at = DateTime.UtcNow.AddYears(1),
            p_uidai_actn = (string?)null,
            p_uidai_err = (string?)null,
            p_ac = "AC",
            p_sa = "SA",
            p_risk_category = "LOW",
            p_expires_at = DateTime.UtcNow.AddYears(1),
            p_rekyc_due_at = DateTime.UtcNow.AddYears(10),
            p_retain_until = DateTime.UtcNow.AddYears(15),
            p_poi_name_cipher = nameCipher,
            p_poi_dob = dob.Date,
            p_poi_dob_type = "V",
            p_poi_gender = gender.ToUpper()[..1],
            p_poi_phone_cipher = phoneCipher,
            p_poi_email_cipher = emailCipher,
            p_poa_cipher = addressCipher,
            p_ldata_cipher = (string?)null,
            p_pht_storage_ref = (string?)null,
            p_pht_iv = (string?)null,
            p_mobile_hash = (string?)null,
            p_email_hash = (string?)null,
            p_pii_key_ref = "key1",
            p_pii_key_version = "v1",
            p_iv_name = nameIv,
            p_iv_phone = phoneIv,
            p_iv_email = emailIv,
            p_iv_poa = addressIv,
            p_iv_ldata = (string?)null
        });
    }
}
