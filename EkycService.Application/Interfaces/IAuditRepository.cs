using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycService.Application.Interfaces;

public interface IAuditRepository
{
    Task LogAsync(Guid refId, string action, string status);
}
