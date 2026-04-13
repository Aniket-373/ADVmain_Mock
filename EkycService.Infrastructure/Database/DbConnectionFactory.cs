using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

using Microsoft.Extensions.Configuration;

namespace EkycService.Infrastructure.Database;

public class DbConnectionFactory
{
    private readonly IConfiguration _config;

    public DbConnectionFactory(IConfiguration config)
    {
        _config = config;
    }

    public NpgsqlConnection CreateAppConnection()
        => new(_config.GetConnectionString("AppDb"));

    public NpgsqlConnection CreateVaultConnection()
        => new(_config.GetConnectionString("VaultDb"));
}
