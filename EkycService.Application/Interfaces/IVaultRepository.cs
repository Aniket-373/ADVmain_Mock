using EkycService.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycService.Application.Interfaces;

public interface IVaultRepository
{
    Task SaveAsync(
        Guid refId,
        string encryptedXml,
        string iv,
        string tag,
        string wrappedDek,

        string uidCipher,
        string uidIv,
        string uidTag
    );

    Task<VaultData> GetByRefIdAsync(Guid refId);
}
