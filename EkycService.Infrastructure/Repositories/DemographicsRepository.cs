using Dapper;
using EkycService.Application.DTOs.Response;
using EkycService.Application.Interfaces;
using EkycService.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EkycService.Infrastructure.Repositories;

public class DemographicsRepository : IDemographicsRepository
{
    private readonly DbConnectionFactory _factory;

    public DemographicsRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task SaveAsync(
    Guid refId,
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

        var sql = @"
        INSERT INTO app.ekyc_demographics
        (
            ref_id,
            app_id,
            poi_name_cipher,
            poi_dob,
            poi_dob_type,
            poi_gender,
            poi_phone_cipher,
            poi_email_cipher,
            poa_cipher,
            pii_key_ref,
            pii_key_version,
            iv_name,
            iv_phone,
            iv_email,
            iv_poa
        )
        VALUES
        (
            @RefId,
            'app1',
            @Name,
            @Dob,
            'V',
            @Gender,
            @Phone,
            @Email,
            @Address,
            'key1',
            'v1',
            @IvName,
            @IvPhone,
            @IvEmail,
            @IvAddress
        )";

        await conn.ExecuteAsync(sql, new
        {
            RefId = refId,
            Name = nameCipher,
            Dob = dob,
            Gender = gender,

            Phone = phoneCipher,
            Email = emailCipher,
            Address = addressCipher,

            IvName = nameIv,
            IvPhone = phoneIv,
            IvEmail = emailIv,
            IvAddress = addressIv
        });
    }

    public async Task<DemographicsData> GetByRefIdAsync(Guid refId)
    {
        using var conn = _factory.CreateAppConnection();

        var sql = @"
            SELECT 
                poi_name_cipher AS NameCipher,
                iv_name AS IvName,

                poi_dob AS Dob,
                poi_dob_type AS DobType,
                poi_gender AS Gender,

                poi_phone_cipher AS PhoneCipher,
                iv_phone AS IvPhone,

                poi_email_cipher AS EmailCipher,
                iv_email AS IvEmail,

                poa_cipher AS AddressCipher,
                iv_poa AS IvPoa,

                pii_key_ref AS PiiKeyRef,
                pii_key_version AS PiiKeyVersion

            FROM app.ekyc_demographics
            WHERE ref_id = @RefId
            LIMIT 1";

        return await conn.QueryFirstOrDefaultAsync<DemographicsData>(sql, new { RefId = refId });
    }
}
