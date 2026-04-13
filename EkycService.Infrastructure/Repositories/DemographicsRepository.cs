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

    public async Task SaveAsync(Guid refId, string nameCipher, string nameIv)
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
                poa_cipher,
                pii_key_ref,
                pii_key_version,
                iv_name,
                iv_poa
            )
            VALUES
            (
                @RefId,
                'app1',
                @Name,
                CURRENT_DATE,
                'V',
                'M',
                'dummy-address',
                'key1',
                'v1',
                @Iv,
                'iv2'
            )";

        await conn.ExecuteAsync(sql, new
        {
            RefId = refId,
            Name = nameCipher,
            Iv = nameIv
        });
    }

    public async Task<DemographicsData> GetByRefIdAsync(Guid refId)
    {
        using var conn = _factory.CreateAppConnection();

        var sql = @"
        SELECT 
            poi_name_cipher AS NameCipher,
            iv_name AS IvName
        FROM app.ekyc_demographics
        WHERE ref_id = @RefId
        LIMIT 1";

        return await conn.QueryFirstOrDefaultAsync<DemographicsData>(sql, new { RefId = refId });
    }
}
