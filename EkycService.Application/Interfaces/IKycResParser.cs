using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EkycService.Application.DTOs.Response;

namespace EkycService.Application.Interfaces;

public interface IKycResParser
{
    ParsedKycData Parse(string decryptedXml);
}
