using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycService.Application.DTOs.Response;

public class DemographicsData
{
    public string NameCipher { get; set; }
    public string IvName { get; set; }
    public DateOnly Dob { get; set; }
    public string DobType { get; set; }
    public string Gender { get; set; }
    public string? PhoneCipher { get; set; }
    public string? IvPhone { get; set; }
    public string? EmailCipher { get; set; }
    public string? IvEmail { get; set; }
    public string AddressCipher { get; set; }
    public string IvPoa { get; set; }
    public string PiiKeyRef { get; set; }
    public string PiiKeyVersion { get; set; }
}

