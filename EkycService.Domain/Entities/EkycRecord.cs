using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycService.Domain.Entities;

public class EkycRecord
{
    public Guid RefId { get; set; }
    public string AppId { get; set; }
    public string UidTokenCipher { get; set; }
    public string IvToken { get; set; }
    public string UidTokenHmac { get; set; }
    public string TxnId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
