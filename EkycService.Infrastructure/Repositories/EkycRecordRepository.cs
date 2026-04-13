using Dapper;
using EkycService.Application.Interfaces;
using EkycService.Infrastructure.Database;

namespace EkycService.Infrastructure.Repositories;

public class EkycRecordRepository : IEkycRecordRepository
{
    private readonly DbConnectionFactory _factory;

    public EkycRecordRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task SaveAsync(Guid refId, string uidTokenCipher, string uidIv)
    {
        using var conn = _factory.CreateAppConnection();

        var txnId = $"txn-{Guid.NewGuid()}";

        var sql = @"
        INSERT INTO app.ekyc_record
        (
            ref_id,
            app_id,
            uid_token_cipher,
            iv_token,
            txn_id,
            uidai_code,
            uidai_ts,
            uidai_ttl_at,
            ac,
            sa,
            expires_at,
            rekyc_due_at,
            record_retain_until
        )
        VALUES
        (
            @RefId,
            'app1',
            @UidTokenCipher,
            @UidIv,
            @TxnId,
            'code1',
            NOW(),
            NOW(),
            'AC',
            'SA',
            NOW(),
            NOW(),
            NOW()
        )";

        await conn.ExecuteAsync(sql, new
        {
            RefId = refId,
            TxnId = txnId,
            UidTokenCipher = uidTokenCipher,
            UidIv = uidIv
        });
    }
}