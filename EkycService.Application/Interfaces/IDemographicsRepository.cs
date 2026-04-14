using EkycService.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycService.Application.Interfaces;

public interface IDemographicsRepository
{
    Task SaveAsync(
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
        string addressIv
    );

    Task<DemographicsData> GetByRefIdAsync(Guid refId);
}