using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycService.Application.Interfaces;

public interface IEkycRecordRepository
{
    Task SaveAsync(Guid refId, string uidTokenCipher, string uidIv);
}
