using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycService.Application.DTOs.Response;

public sealed record GcmResult(
    string CipherB64,
    string IvHex,
    string TagHex
);
