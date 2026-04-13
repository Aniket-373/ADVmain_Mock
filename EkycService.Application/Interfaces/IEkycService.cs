using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EkycService.Application.DTOs.Request;
using EkycService.Application.DTOs.Response;

namespace EkycService.Application.Interfaces;

public interface IEkycService
{
    Task<SaveEkycResponse> SaveAsync(SaveEkycRequest request);

    Task<string> GetRawXmlAsync(Guid refId);
    Task<object> GetDemographicsAsync(Guid refId);
}