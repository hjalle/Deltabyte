using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deltabyte.Models;
using System.IO;

namespace Deltabyte.Delta
{
    public class UnsafeDeltaGenerator : IDeltaGenerator
    {
        public unsafe ByteDelta ComputeDelta(byte[] left, byte[] right)
        {

            if (left.Length > right.Length)
            {
                throw new Exception("Can't shrink");
            }
            else if (left.Length % sizeof(long) != 0 || right.Length % sizeof(long) != 0)
            {
                throw new Exception("Unsafe delta handler only support arrays of length % sizeof(long), (mod 8)");
            }

            ByteDelta delta = new ByteDelta();
            MemoryStream ms = new MemoryStream();
            var lastStartIndex = -1;
            var bytesInStream = 0;
            var i = 0;
            fixed (byte* bptrRight = right)
            fixed (byte* bptrLeft = left)
            {
                long* ptrLeft = (long*)bptrLeft;
                long* ptrRight = (long*)bptrRight;
                for (i = 0; i < left.Length; i += sizeof(long))
                {
                    var ptrIndex = i / sizeof(long);
                    if (ptrLeft[ptrIndex] != ptrRight[ptrIndex])
                    {
                        ms.Write(right, i, sizeof(long));
                        bytesInStream += sizeof(long);
                        if (lastStartIndex == -1)
                        {
                            lastStartIndex = i;
                        }
                    }
                    else if (bytesInStream > 0)
                    {
                        bytesInStream = 0;
                        ms.Flush();
                        ByteDiff diff = new ByteDiff(ms.ToArray(), lastStartIndex);
                        lastStartIndex = -1;
                        delta.Add(diff);
                        ms.SetLength(0);
                    }
                }
            }
            if (right.Length > left.Length)
            {
                //Remaining of right needs to be added
                var offset = 0;
                if (bytesInStream > 0)
                {
                    offset = lastStartIndex + bytesInStream;
                }
                else
                {
                    offset = i;
                }
                ms.Write(right, offset, right.Length - offset);
                bytesInStream += right.Length - offset;
            }
            if (bytesInStream > 0)
            {
                ms.Flush();
                ByteDiff diff = new ByteDiff(ms.ToArray(), lastStartIndex);
                delta.Add(diff);
            }
            ms.Dispose();
            return delta;
        }
    }
}
