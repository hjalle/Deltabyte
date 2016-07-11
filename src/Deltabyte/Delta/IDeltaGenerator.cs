using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deltabyte.Models;

namespace Deltabyte.Delta
{
    public interface IDeltaGenerator
    {
        ByteDelta ComputeDelta(byte[] left, byte[] right);
    }
}
