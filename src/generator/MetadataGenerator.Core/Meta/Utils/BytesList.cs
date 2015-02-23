using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataGenerator.Core.Meta.Utils
{
    public class BytesList : IEnumerable<byte>, ICloneable
    {
        private readonly List<byte> data;

        public BytesList()
            : this(new List<byte>())
        {
        }

        public BytesList(IEnumerable<byte> bytes)
        {
            this.data = new List<byte>(bytes);
        }

        public uint Length
        {
            get { return (uint)this.data.Count; }
        }

        public uint Append(BytesList list)
        {
            return this.Append(list.data.ToArray());
        }

        public uint Append(params byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            uint valueOffset = this.Length;
            this.data.AddRange(bytes);
            return valueOffset;
        }

        public object Clone()
        {
            return new BytesList(this.data);
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
