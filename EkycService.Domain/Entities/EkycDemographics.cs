using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycService.Domain.Entities;

public class EkycDemographics
{
    public Guid RefId { get; set; }
    public string PoiNameCipher { get; set; }
    public DateOnly PoiDob { get; set; }
    public char PoiGender { get; set; }
    public string PoaCipher { get; set; }
}