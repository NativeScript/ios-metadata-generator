using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MetadataGenerator.Core.Meta.Utils
{
    public interface IBinarySerializable
    {
        Object GetBinaryStructure();
    }

    public class BinarySymbol : IBinarySerializable
    {
        [Flags]
        public enum MetaFlags : byte
        {
            HasName = 1 << 7,
            IsIosAppExtensionAvailable = 1 << 6,

            FunctionIsVariadic = 1 << 5,
            FunctionOwnsReturnedCocoaObject = 1 << 4,

            MemberIsLocalJsNameDuplicate = 1 << 0,
            MemberHasJsNameDuplicateInHierarchy = 1 << 1,

            MethodIsVariadic = 1 << 2,
            MethodIsNullTerminatedVariadic = 1 << 3,
            MethodOwnsReturnedCocoaObject = 1 << 4,

            PropertyHasGetter = 1 << 2,
            PropertyHasSetter = 1 << 3
        }

        public enum SymbolType : byte
        {
            Undefined = 0,
            Struct,
            Union,
            Function,
            JsCode,
            Var,
            Interface,
            Protocol,
            Category
        }

        public BinarySymbol()
        {
        }

        public string Name { get; set; }

        public string JsName { get; set; }

        public SymbolType Type
        {
            get
            {
                return (SymbolType)((byte)this.Flags & 7);
            }
            set
            {
                this.Flags |= (MetaFlags)value;
            }
        }

        public MetaFlags Flags { get; set; }

        public string Module { get; set; }

        public Common.Version IntrducedIn { get; set; }

        public Object Info { get; set; }

        public object GetBinaryStructure()
        {
            List<object> result = new List<object>();

            // Name
            bool hasName = (this.Name != this.JsName);
            object namePointerValue = (hasName) ? new List<object>() { new NotCalculatedOffset(this.JsName), new NotCalculatedOffset(this.Name) } : (object)this.JsName;
            result.Add(new NotCalculatedOffset(namePointerValue));

            // Flags
            MetaFlags flags = this.Flags;
            if (hasName)
            {
                flags |= BinarySymbol.MetaFlags.HasName;
            }
            result.Add((byte)flags);

            // Module
            result.Add(new ModuleId(this.Module));

            // Introduced In
            result.Add(this.IntrducedIn.ToByte());

            // Info
            result.Add(this.Info);

            return result;
        }
    }

    public static class BinarySymbolExtensions
    {
        public static byte ToByte(this Common.Version version)
        {
            byte result = 0;
            if (version != null && version.Major != -1)
            {
                Debug.Assert(version.Major >= 1 && version.Major <= 31);
                result |= (byte)(version.Major << 3);
                if (version.Minor != -1)
                {
                    Debug.Assert(version.Minor >= 0 && version.Minor <= 7);
                    result |= (byte)version.Minor;
                }
            }
            return result;
        }

        public static BinarySymbol ChangeToJsCode(this BinarySymbol structure, string jsCode)
        {
            Debug.Assert(structure.Info == null, "The JS Code will override other information.");
            structure.Type = BinarySymbol.SymbolType.JsCode;
            structure.Info = new NotCalculatedOffset(jsCode);
            return structure;
        }

        public static string GetTopLevelModule(this BinarySymbol structure)
        {
            int firstDotIndex = structure.Module.IndexOf('.');
            return (firstDotIndex > 0) ? structure.Module.Substring(0, firstDotIndex) : structure.Module;
        }
    }

    public class BinaryArray<T> : IBinarySerializable
    {
        private readonly List<T> elements;

        public bool HasCountAtBegining { get; private set; }

        public int Count
        {
            get
            {
                return this.elements.Count;
            }
        }

        public BinaryArray(bool hasCountAtBeginning = true)
        {
            this.elements = new List<T>();
            this.HasCountAtBegining = hasCountAtBeginning;
        }

        public BinaryArray(IEnumerable<T> elements, bool hasCountAtBeginning = true)
        {
            this.elements = new List<T>(elements);
            this.HasCountAtBegining = hasCountAtBeginning;
        }

        public void Add(params T[] elements)
        {
            this.elements.AddRange(elements);
        }

        public T this[int i]
        {
            get
            {
                return this.elements[i];
            }
            set
            {
                this.elements[i] = value;
            }
        }

        public Object GetBinaryStructure()
        {
            List<object> result = this.elements.Cast<object>().ToList();
            if (this.HasCountAtBegining)
            {
                result.Insert(0, new BinaryCount((uint)result.Count));
            }
            return result;
        }
    }

    public class BinaryHashTable : IBinarySerializable
    {
        private readonly List<Object> elements;
        private readonly Func<Object, uint> hasher;

        public BinaryHashTable(Func<Object, uint> hasher)
        {
            this.elements = elements = new List<Object>();
            this.hasher = hasher;
        }

        public int Count
        {
            get { return this.elements.Count; }
        }

        public void Add(Object element)
        {
            Debug.Assert(element != null);
            this.elements.Add(element);
        }

        public void AddRange(IEnumerable<object> elements)
        {
            foreach (object element in elements)
            {
                this.Add(element);
            }
        }

        public Object GetBinaryStructure()
        {
            int hashTableLength = (int)(this.Count * 1.25);
            BinaryArray<NotCalculatedOffset> hashTable = new BinaryArray<NotCalculatedOffset>();
            for (int i = 0; i < hashTableLength; i++)
            {
                hashTable.Add(new NotCalculatedOffset(null));
            }

            // construct hash table buckets
            foreach (Object obj in this.elements)
            {
                int tableIndex = (int)(this.hasher(obj) % hashTableLength);
                if (hashTable[tableIndex].Value == null)
                {
                    hashTable[tableIndex].Value = new BinaryArray<NotCalculatedOffset>();
                }
                BinaryArray<NotCalculatedOffset> bucket = (BinaryArray<NotCalculatedOffset>)hashTable[tableIndex].Value;
                bucket.Add(new NotCalculatedOffset(obj));
            }

            return hashTable;
        }
    }

    public class CalculatedOffset
    {
        public uint Value { get; set; }

        public CalculatedOffset(uint value)
        {
            this.Value = value;
        }
    }

    public class NotCalculatedOffset
    {
        public NotCalculatedOffset(object value = null)
        {
            this.Value = value;
        }

        public Object Value { get; set; }
    }

    public class BinaryCount
    {
        public BinaryCount(uint value = 0)
        {
            this.Value = value;
        }

        public uint Value { get; set; }
    }

    public class ModuleId
    {
        public string ModuleName { get; set; }

        public ModuleId(string fullName)
        {
            this.ModuleName = fullName;
        }
    }

    public class BinaryTypeEncoding : IBinarySerializable
    {
        public enum BinaryTypeType : byte
        {
            Unknown,
            Void,
            Bool,
            Short,
            UShort,
            Int,
            UInt,
            Long,
            ULong,
            LongLong,
            ULongLong,
            Char,
            UChar,
            Unichar,
            CharS,
            CString,
            Float,
            Double,
            DeclarationReference,
            InterfaceDeclaration, // NSString* -> DeclarationReference, NSString -> InterfaceDeclaration
            Pointer,
            VaList,
            Selector,
            Class,
            Protocol,
            InstanceType,
            Id,
            ConstantArray,
            IncompleteArray,
            FunctionPointer,
            Block,
            AnonymousStruct,
            AnonymousUnion
        }

        public BinaryTypeType Type { get; set; }

        public object Payload { get; set; }

        public BinaryTypeEncoding(BinaryTypeType type, object payload = null)
        {
            this.Type = type;
            this.Payload = payload;
        }

        public object GetBinaryStructure()
        {
            List<object> result = new List<object> { (byte)this.Type };
            if (this.Payload != null)
            {
                result.Add(this.Payload);
            }
            return result;
        }
    }
}
