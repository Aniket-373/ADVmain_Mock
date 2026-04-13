using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycService.Application.Interfaces;

public interface IHsmProvider
{
    Task<byte[]> GenerateDekAsync();
    Task<string> WrapDekAsync(byte[] dek);
    Task<byte[]> UnwrapDekAsync(string wrappedDek);
}
