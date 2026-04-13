using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycService.Domain.Entities;

public class EkycVaultRef
{
    public Guid RefId { get; set; }
    public string VaultToken { get; set; }
    public string UidCipher { get; set; }
    public string EkycXmlCipher { get; set; }
    public string WrappedDek { get; set; }
}