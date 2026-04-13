using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using EkycService.Application.Interfaces;
using EkycService.Infrastructure.Database;

namespace EkycService.Infrastructure.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly DbConnectionFactory _factory;

    public AuditRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task LogAsync(Guid refId, string action, string status)
    {
        using var conn = _factory.CreateAppConnection();

        var txnId = $"txn-{Guid.NewGuid()}";

        var sql = @"
        INSERT INTO app.ekyc_audit_log
        (
            ref_id,
            app_id,
            txn_id,
            operation,
            status,
            requester_id,
            source_ip,
            request_id,
            class_name,
            requested_at,
            responded_at,
            latency_ms
        )
        VALUES
        (
            @RefId,
            'app1',
            @TxnId,
            @Operation,
            @Status,
            'system',
            '127.0.0.1',
            gen_random_uuid(),
            'EkycService',
            NOW(),
            NOW(),
            10
        )";

        await conn.ExecuteAsync(sql, new
        {
            RefId = refId,
            AppId = "app1",
            TxnId = txnId,
            Operation = action,     // MUST be SAVE
            Status = status,
            RequesterId = "system",
            SourceIp = "127.0.0.1",
            RequestId = Guid.NewGuid(),
            ClassName = "EkycService",
            Latency = 10
        });
    }
}