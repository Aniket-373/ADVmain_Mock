using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycService.Application.DTOs.Response;

public class VaultData
{
    public string XmlCipher { get; set; }
    public string IvXml { get; set; }
    public string TagXml { get; set; }
    public string WrappedDek { get; set; }
}
