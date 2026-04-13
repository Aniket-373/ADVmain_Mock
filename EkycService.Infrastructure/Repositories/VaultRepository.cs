using Dapper;
using EkycService.Application.DTOs.Response;
using EkycService.Application.Interfaces;
using EkycService.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EkycService.Infrastructure.Repositories;

public class VaultRepository : IVaultRepository
{
    private readonly DbConnectionFactory _factory;

    public VaultRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task SaveAsync(Guid refId,string encryptedXml,string iv,string tag,string wrappedDek,string uidCipher,string uidIv,string uidTag)
    {
        using var conn = _factory.CreateVaultConnection();

        var sql = @"
        INSERT INTO vault.ekyc_vault_ref
        (
            ref_id,
            vault_token,
            app_id,
            uid_cipher,
            iv_uid,
            gcm_tag_uid,
            ekyc_xml_cipher,
            iv_xml,
            gcm_tag_xml,
            rar_cipher,
            iv_rar,
            gcm_tag_rar,
            wrapped_dek,
            kek_version,
            kek_ref
        )
        VALUES
        (
            @RefId,
            gen_random_uuid()::text,
            'app1',
            @UidCipher,
            @UidIv,
            @UidTag,
            @Xml,
            @Iv,
            @Tag,
            'rar',
            'iv2',
            'tag2',
            @Dek,
            'v1',
            'kek1'
        )";

        await conn.ExecuteAsync(sql, new
        {
            RefId = refId,
            Xml = encryptedXml,
            Iv = iv,
            Tag = tag,
            Dek = wrappedDek,

            UidCipher = uidCipher,
            UidIv = uidIv,
            UidTag = uidTag
        });
    }

    public async Task<VaultData> GetByRefIdAsync(Guid refId)
    {
        using var conn = _factory.CreateVaultConnection();

        var sql = @"
        SELECT 
            ekyc_xml_cipher AS XmlCipher,
            iv_xml AS IvXml,
            gcm_tag_xml AS TagXml,
            wrapped_dek AS WrappedDek
        FROM vault.ekyc_vault_ref
        WHERE ref_id = @RefId
        LIMIT 1";

        return await conn.QueryFirstOrDefaultAsync<VaultData>(sql, new { RefId = refId });
    }
}
