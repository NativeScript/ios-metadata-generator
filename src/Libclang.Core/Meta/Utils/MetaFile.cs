using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Libclang.Core.Meta.Utils
{
    public class MetaFile
    {
        public MetaFile(int globalTableSize)
        {
            this.PointerSize = 4;
            this.ArrayCountSize = 4;
            this.StringEncoding = Encoding.ASCII;

            this.GlobalTable = new GlobalTableSection(this, globalTableSize);
            this.Heap = new HeapSection(this);
        }

        public byte PointerSize { get; private set; }

        public byte ArrayCountSize { get; private set; }

        public GlobalTableSection GlobalTable { get; private set; }

        public HeapSection Heap { get; private set; }

        public Encoding StringEncoding { get; private set; }


        public void SaveAs(string filePath)
        {
            using (FileStream writer = new FileStream(filePath, FileMode.Create))
            {
                this.WriteTo(writer, this.GlobalTable.Unzip(this.Heap)) // global table section
                    .WriteTo(writer, this.Heap.Data); // heap section
            }
        }

        private MetaFile WriteTo(FileStream writer, IEnumerable<byte> bytes)
        {
            byte[] bytesArray = bytes.ToArray();
            writer.Write(bytesArray, 0, bytesArray.Length);
            return this;
        }


        public class GlobalTableSection
        {
            private readonly MetaFile file;

            private readonly List<object>[] data;

            public GlobalTableSection(MetaFile file, int count)
            {
                this.file = file;
                this.data = new List<object>[count];
            }

            public int Count
            {
                get { return this.data.Length; }
            }

            public void AddMeta(string key, BinaryMetaStructure metaStructure)
            {
                int tableIndex = (int) (this.Hash(key)%this.Count);
                if (this.data[tableIndex] == null)
                {
                    this.data[tableIndex] = new List<object>();
                }
                this.data[tableIndex].Add(new Pointer(metaStructure));
            }

            public uint Hash(string key)
            {
                return JscStringHasher.Hash(key);
            }

            public List<byte> Unzip(HeapSection heap)
            {
                List<byte> inlinedGlobalTable = new List<byte>(this.Count*this.file.PointerSize);
                List<byte> globalTableSize = heap.ConvertArrayCount(new ArrayCount((uint) this.Count));
                inlinedGlobalTable.AddRange(globalTableSize);

                foreach (List<object> list in this.data)
                {
                    List<byte> pointer = null;
                    if (list == null)
                    {
                        pointer = heap.ConvertOffset(0);
                    }
                    else
                    {
                        list.Insert(0, new ArrayCount((uint) list.Count));
                        pointer = heap.ConvertPointer(new Pointer(list));
                    }
                    inlinedGlobalTable.AddRange(pointer);
                }
                return inlinedGlobalTable;
            }
        }

        public class HeapSection
        {
            private readonly MetaFile file;

            private readonly List<byte> data;

            private readonly Dictionary<string, uint> strings;

            public HeapSection(MetaFile file)
            {
                this.file = file;
                this.data = new List<byte>();
                this.strings = new Dictionary<string, uint>();
                this.AddByte(0); // the first byte of the section is set to empty byte (has a function of null pointer)
            }

            public uint Length
            {
                get { return (uint) this.data.Count; }
            }

            public List<byte> Data
            {
                get { return data; }
            }


            public uint AddInternString(string str)
            {
                if (this.strings.ContainsKey(str))
                {
                    return this.strings[str];
                }

                List<byte> stringBytes = this.ConvertString(str);
                uint offset = this.AddBytes(stringBytes);
                this.strings.Add(str, offset);
                return offset;
            }

            public uint AddObject(object value)
            {
                return this.AddBytes(this.Convert(value));
            }

            public List<byte> Convert(object value)
            {
                if (value is Pointer)
                {
                    return this.ConvertPointer((Pointer) value);
                }
                else if (value is IEnumerable)
                {
                    return ConvertEnumberable((IEnumerable) value);
                }
                else if (value is ArrayCount)
                {
                    return this.ConvertArrayCount((ArrayCount) value);
                }
                else if (value is BinaryMetaStructure)
                {
                    return this.ConvertMetaStructure((BinaryMetaStructure) value);
                }
                else if (value is TypeEncoding)
                {
                    return this.ConvertString(value.ToString());
                }
                else if (value is string)
                {
                    return this.ConvertString((string)value);
                }
                else if (value is uint)
                {
                    return this.ConvertOffset((uint)value);
                }
                else if (value is int)
                {
                    return this.ConvertNumber((long)value, 4);
                }
                else if (value is short || value is ushort)
                {
                    return this.ConvertNumber((long)(short)value, 2);
                }
                else if (value is byte || value is sbyte)
                {
                    return this.ConvertNumber((byte)value, 1);
                }

                throw new ArgumentException("Invalid object type.");
            }

            public List<byte> ConvertNumber(long number, int bytesCount)
            {
                if (number >= Math.Pow(2, this.file.PointerSize*8))
                {
                    throw new ArgumentOutOfRangeException("number",
                        String.Format("The number must fit in {0} bytes.", bytesCount));
                }

                List<byte> bytes = new List<byte>();
                for (int i = 0; i < bytesCount; i++)
                {
                    int pad = 8*i;
                    byte current = (byte) ((number & (255 << pad)) >> pad);
                    bytes.Add(current);
                }
                return bytes;
            }

            public List<byte> ConvertString(string str)
            {
                List<byte> bytes = new List<byte>();
                if (str != null)
                {
                    bytes = this.file.StringEncoding.GetBytes(str).ToList();
                    bytes.Add(0); // null terminate the string
                }
                return bytes;
            }

            public List<byte> ConvertPointer(Pointer pointer)
            {
                uint offset = 0;
                if (pointer.Value != null)
                {
                    if (pointer.Value is string)
                    {
                        offset = this.AddInternString((string) pointer.Value);
                    }

                    if (offset == 0)
                    {
                        List<byte> valueBytes = this.Convert(pointer.Value);
                        offset = this.AddBytes(valueBytes);
                    }
                }

                return this.ConvertOffset(offset);
            }

            public List<byte> ConvertOffset(uint offset)
            {
                return this.ConvertNumber(offset, this.file.PointerSize);
            }

            public List<byte> ConvertArrayCount(ArrayCount count)
            {
                return this.ConvertNumber(count.Value, this.file.ArrayCountSize);
            }

            public List<byte> ConvertEnumberable(IEnumerable enumerable)
            {
                List<byte> bytes = new List<byte>();
                foreach (object element in enumerable)
                {
                    bytes.AddRange(this.Convert(element));
                }

                return bytes;
            }

            public List<byte> ConvertMetaStructure(BinaryMetaStructure structure)
            {
                List<byte> bytes = new List<byte>();

                // Name
                bool hasName = (structure.Name != structure.JsName);
                object namePointerValue = (hasName) ? new List<object>() { new Pointer(structure.JsName), new Pointer(structure.Name) } : (object)structure.JsName;
                structure.Flags[BinaryMetaStructure.HasName] = hasName;
                bytes.AddRange(this.Convert(new Pointer(namePointerValue)));

                // Flags
                bytes.AddRange(this.Convert(structure.Flags.ToByte()));

                // Framework
                bytes.AddRange(this.Convert(structure.Framework.ToByte()));

                // Introduced In
                bytes.AddRange(this.Convert(structure.IntrducedIn.ToByte()));

                // Info
                bytes.AddRange(this.Convert(structure.Info));

                return bytes;
            }


            public uint AddByte(byte singleByte)
            {
                uint valueOffset = this.Length;
                this.data.Add(singleByte);
                return valueOffset;
            }

            public uint AddBytes(IEnumerable<byte> bytes)
            {
                if (bytes == null)
                {
                    throw new ArgumentNullException("bytes");
                }
                uint valueOffset = this.Length;
                this.data.AddRange(bytes);
                return valueOffset;
            }
        }
    }
}
