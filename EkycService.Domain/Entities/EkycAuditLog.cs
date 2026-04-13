using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycService.Domain.Entities;

public class EkycAuditLog
{
    public Guid RefId { get; set; }
    public string Operation { get; set; }
    public string Status { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
}
