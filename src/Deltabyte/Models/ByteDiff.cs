using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deltabyte.Models
{
    public class ByteDiff
    {
        private byte[] _data;
        private int _offset;

        public ByteDiff(byte[] data, int offset)
        {

            this._data = data;
            this._offset = offset;
        }
        public int Offset
        {
            get { return _offset; }
        }
        public byte[] Data
        {
            get
            {
                return _data;
            }
        }
    }
}
