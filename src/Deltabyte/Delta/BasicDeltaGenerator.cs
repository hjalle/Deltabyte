using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deltabyte.Models;
using System.IO;

namespace Deltabyte.Delta
{
    public class BasicDeltaGenerator : IDeltaGenerator
    {
        public ByteDelta ComputeDelta(byte[] left, byte[] right)
        {
            if (left.Length > right.Length)
            {
                throw new Exception("Can't shrink");
            }

            ByteDelta delta = new ByteDelta();
            MemoryStream ms = new MemoryStream();
            var lastStartIndex = -1;
            var bytesInStream = 0;
            var i = 0;
            for (i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    ms.WriteByte(right[i]);
                    bytesInStream++;
                    if (lastStartIndex == -1)
                    {
                        lastStartIndex = i;
                    }
                }
                else if(bytesInStream > 0)
                {
                    bytesInStream = 0;
                    ms.Flush();
                    ByteDiff diff = new ByteDiff(ms.ToArray(), lastStartIndex);
                    delta.Add(diff);
                    ms.Dispose();
                    ms = new MemoryStream();
                }
            }
            if (right.Length > left.Length)
            {
                //Remaining of right needs to be added
                var offset = 0;
                if (bytesInStream > 0)
                {
                    offset = lastStartIndex + bytesInStream;
                } else
                {
                    offset = i;
                }
                ms.Write(right, offset, right.Length - offset);
                bytesInStream += right.Length - offset;
            }
            if(bytesInStream > 0)
            {
                ms.Flush();
                ByteDiff diff = new ByteDiff(ms.ToArray(), i);
                delta.Add(diff);
            }
            ms.Dispose();
            return delta;
        }
    }
}
