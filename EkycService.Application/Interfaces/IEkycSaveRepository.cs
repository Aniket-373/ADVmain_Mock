using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycService.Application.Interfaces;

public interface IEkycSaveRepository
{
    //Task SaveAsync(
    //    Guid refId,
    //    string uidCipher,
    //    string uidIv,
    //    string uidHmac,

    //    string nameCipher,
    //    string nameIv,
    //    DateTime dob,
    //    string gender,
    //    string? phoneCipher,
    //    string? phoneIv,
    //    string? emailCipher,
    //    string? emailIv,
    //    string addressCipher,
    //    string addressIv,

    //    string xmlCipher,
    //    string xmlIv,
    //    string xmlTag,
    //    string wrappedDek,
    //    string uidVaultCipher,
    //    string uidVaultIv,
    //    string uidVaultTag
    //);

    Task SaveAsync(
    Guid refId,
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
    string addressIv
);
}
