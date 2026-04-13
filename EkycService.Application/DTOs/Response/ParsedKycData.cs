using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycService.Application.DTOs.Response;

public class ParsedKycData
{
    public string? Uid { get; set; }

    public string? Name { get; set; }
    public string? Dob { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public string? Address { get; set; }

    public byte[]? Photo { get; set; }
}