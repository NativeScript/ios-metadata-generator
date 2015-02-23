using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetadataGenerator.Core.Meta.Utils
{
    class BinaryEncodingTransformation : TypeEncodingTransfomation<BinaryTypeEncoding>
    {
        protected override internal BinaryTypeEncoding TransformUnknown()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Unknown);
        }

        protected override internal BinaryTypeEncoding TransformVaList()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.VaList);
        }

        protected override internal BinaryTypeEncoding TransformProtocol()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Protocol);
        }

        protected override internal BinaryTypeEncoding TransformVoid()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Void);
        }

        protected override internal BinaryTypeEncoding TransformBool()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Bool);
        }

        protected override internal BinaryTypeEncoding TransformShort()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Short);
        }

        protected override internal BinaryTypeEncoding TransformUShort()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.UShort);
        }

        protected override internal BinaryTypeEncoding TransformInt()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Int);
        }

        protected override internal BinaryTypeEncoding TransformUInt()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.UInt);
        }

        protected override internal BinaryTypeEncoding TransformLong()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Long);
        }

        protected override internal BinaryTypeEncoding TransformULong()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.ULong);
        }

        protected override internal BinaryTypeEncoding TransformLongLong()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.LongLong);
        }

        protected override internal BinaryTypeEncoding TransformULongLong()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.ULongLong);
        }

        protected override internal BinaryTypeEncoding TransformChar()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Char);
        }

        protected override internal BinaryTypeEncoding TransformUChar()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.UChar);
        }

        protected override internal BinaryTypeEncoding TransformUnichar()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Unichar);
        }

        protected override internal BinaryTypeEncoding TransformCharS()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.CharS);
        }

        protected override internal BinaryTypeEncoding TransformCString()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.CString);
        }

        protected override internal BinaryTypeEncoding TransformFloat()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Float);
        }

        protected override internal BinaryTypeEncoding TransformDouble()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Double);
        }

        protected override internal BinaryTypeEncoding TransformSelector()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Selector);
        }

        protected override internal BinaryTypeEncoding TransformClass()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Class);
        }

        protected override internal BinaryTypeEncoding TransformInstancetype()
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.InstanceType);
        }

        protected override internal BinaryTypeEncoding TransformId(params Tuple<string, string>[] protocols)
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Id);
        }

        protected override internal BinaryTypeEncoding TransformConstantArray(int size, TypeEncoding elementType)
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.ConstantArray, new object[] { size, this.Transform(elementType) });
        }

        protected override internal BinaryTypeEncoding TransformIncompleteArray(TypeEncoding elementType)
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.IncompleteArray, this.Transform(elementType));
        }

        protected override internal BinaryTypeEncoding TransformInterface(string name, string moduleName)
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.DeclarationReference, new object[] { new ModuleId(moduleName), new NotCalculatedOffset(name) });
        }

        protected override internal BinaryTypeEncoding TransformFunction(TypeEncoding returnType, IEnumerable<TypeEncoding> parameterTypes)
        {
            List<object> payload = new List<object>();
            Debug.Assert(parameterTypes.Count() <= 255);
            payload.Add((byte)parameterTypes.Count());
            payload.AddRange(parameterTypes.Select(t => this.Transform(t)));

            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.FunctionPointer, payload);
        }

        protected override internal BinaryTypeEncoding TransformBlock(TypeEncoding returnType, IEnumerable<TypeEncoding> parameterTypes)
        {
            List<object> payload = new List<object>();
            Debug.Assert(parameterTypes.Count() <= 255);
            payload.Add((byte)parameterTypes.Count());
            payload.AddRange(parameterTypes.Select(t => this.Transform(t)));

            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Block, payload);
        }

        protected override internal BinaryTypeEncoding TransformPointer(TypeEncoding target)
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.Pointer, this.Transform(target));
        }

        protected override internal BinaryTypeEncoding TransformStruct(string name, string moduleName)
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.DeclarationReference, new object[] { new NotCalculatedOffset(moduleName), new NotCalculatedOffset(name) });
        }

        protected override internal BinaryTypeEncoding TransformUnion(string name, string moduleName)
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.DeclarationReference, new object[] { new NotCalculatedOffset(moduleName), new NotCalculatedOffset(name) });
        }

        protected override internal BinaryTypeEncoding TransformInterfaceDeclaration(string name, string moduleName)
        {
            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.InterfaceDeclaration, new object[] { new NotCalculatedOffset(moduleName), new NotCalculatedOffset(name) });
        }

        protected override internal BinaryTypeEncoding TransformAnonymousStruct(IEnumerable<RecordField> fields)
        {
            List<object> payload = new List<object>();
            Debug.Assert(fields.Count() <= 255);
            payload.Add((byte)fields.Count());
            payload.AddRange(fields.Select(f => this.Transform(f.TypeEncoding)));

            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.AnonymousStruct, payload);
        }

        protected override internal BinaryTypeEncoding TransformAnonymousUnion(IEnumerable<RecordField> fields)
        {
            List<object> payload = new List<object>();
            Debug.Assert(fields.Count() <= 255);
            payload.Add((byte)fields.Count());
            payload.AddRange(fields.Select(f => this.Transform(f.TypeEncoding)));

            return new BinaryTypeEncoding(BinaryTypeEncoding.BinaryTypeType.AnonymousUnion, payload);
        }
    }
}
