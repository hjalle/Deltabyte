using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Deltabyte.Models
{
    public class ByteDelta
    {
        private List<ByteDiff> _diffs;

        public ByteDelta()
        {
            this._diffs = new List<ByteDiff>();
        }
        public void Add(ByteDiff diff)
        {
            this._diffs.Add(diff);
        }
        public long ApplyDelta(Stream stream)
        {
            var bytesWritten = 0;
            foreach (var point in this._diffs)
            {
                stream.Position = point.Offset;
                stream.Write(point.Data, 0, point.Data.Length);
                bytesWritten += point.Data.Length;
            }
            return bytesWritten;
        }
    }
}
