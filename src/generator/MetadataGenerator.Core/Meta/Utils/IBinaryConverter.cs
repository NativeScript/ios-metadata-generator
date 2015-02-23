using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetadataGenerator.Core.Meta.Utils
{
    public interface IBinaryConverter
    {
        BytesList Convert(Object value);
    }

    public class DefaultBinaryConverter : IBinaryConverter
    {
        public byte OffsetSize { get; set; }

        public byte ArrayCountSize { get; set; }

        public byte ModuleIdSize { get; set; }

        public Encoding StringEncoding { get; set; }

        public Func<NotCalculatedOffset, CalculatedOffset> OffsetCalculator { get; set; }

        public Func<string, int> ModuleIdCalculator { get; set; }

        public BytesList Convert(Object value)
        {
            if (value is IBinarySerializable)
            {
                return this.Convert(((IBinarySerializable)value).GetBinaryStructure());
            }
            if (value is CalculatedOffset)
            {
                return this.ConvertCalculatedOffset((CalculatedOffset)value);
            }
            else if (value is NotCalculatedOffset)
            {
                return this.ConvertNotCalculatedOffset((NotCalculatedOffset)value);
            }
            else if (value is BytesList)
            {
                return this.ConvertBytesList((BytesList)value);
            }
            else if (value is string)
            {
                return this.ConvertString((string)value);
            }
            else if (value is IEnumerable)
            {
                return this.ConvertEnumberable((IEnumerable)value);
            }
            else if (value is BinaryCount)
            {
                return this.ConvertArrayCount((BinaryCount)value);
            }
            else if (value is ModuleId)
            {
                return this.ConvertModuleId((ModuleId)value);
            }
            else if (value is TypeEncoding)
            {
                return this.ConvertString(value.ToString());
            }
            else if (value is uint)
            {
                return this.ConvertNumber(System.Convert.ToInt64(value), 4);
            }
            else if (value is int)
            {
                return this.ConvertNumber(System.Convert.ToInt64(value), 4);
            }
            else if (value is short || value is ushort)
            {
                return this.ConvertNumber((short)value, 2);
            }
            else if (value is byte || value is sbyte)
            {
                return this.ConvertNumber((byte)value, 1);
            }

            throw new ArgumentException("Invalid object type.");
        }

        private BytesList ConvertCalculatedOffset(CalculatedOffset offset)
        {
            return this.ConvertNumber(offset.Value, this.OffsetSize);
        }

        private BytesList ConvertNotCalculatedOffset(NotCalculatedOffset notCalculatedOffset)
        {
            CalculatedOffset calculatedOffset = this.OffsetCalculator(notCalculatedOffset);
            return this.Convert(calculatedOffset);
        }

        private BytesList ConvertNumber(long number, int bytesCount)
        {
            byte[] numBytes = BitConverter.GetBytes(number);

            if (bytesCount >= numBytes.Length)
            {
                throw new ArgumentOutOfRangeException("number",
                    String.Format("The number must fit in {0} bytes.", bytesCount));
            }

            return new BytesList(numBytes.Take(bytesCount));
        }

        private BytesList ConvertBytesList(BytesList bytesList)
        {
            return (BytesList)bytesList.Clone();
        }

        private BytesList ConvertEnumberable(IEnumerable enumerable)
        {
            BytesList result = new BytesList();
            foreach (object element in enumerable)
            {
                result.Append(this.Convert(element));
            }

            return result;
        }

        private BytesList ConvertArrayCount(BinaryCount count)
        {
            return this.ConvertNumber(count.Value, this.ArrayCountSize);
        }

        private BytesList ConvertModuleId(ModuleId moduleId)
        {
            int id = this.ModuleIdCalculator(moduleId.ModuleName);
            return this.ConvertNumber(id, this.ModuleIdSize);
        }

        private BytesList ConvertString(string str)
        {
            BytesList bytes = new BytesList();
            if (str != null)
            {
                bytes = new BytesList(this.StringEncoding.GetBytes(str));
                bytes.Append(0); // null terminate the string
            }
            return bytes;
        }
    }
}
