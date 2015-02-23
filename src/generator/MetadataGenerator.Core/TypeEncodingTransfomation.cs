using System.Collections.Generic;
using System;

namespace Libclang.Core
{
    public class TypeEncodingTransfomation<TReturn>
    {
        public TReturn Transform(TypeEncoding encoding)
        {
            if (encoding == null)
            {
                return this.Default();
            }
            else
            {
                return encoding.Transform(this);
            }
        }

        protected internal virtual TReturn Default()
        {
            return default(TReturn);
        }

        protected internal virtual TReturn TransformUnknown()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformVaList()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformProtocol()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformVoid()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformBool()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformShort()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformUShort()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformInt()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformUInt()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformLong()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformULong()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformLongLong()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformULongLong()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformChar()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformUChar()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformUnichar()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformCharS()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformCString()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformFloat()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformDouble()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformSelector()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformClass()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformInstancetype()
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformId(params Tuple<string, string>[] protocols)
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformConstantArray(int size, TypeEncoding elementType)
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformIncompleteArray(TypeEncoding elementType)
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformInterface(string name, string module)
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformFunction(TypeEncoding returnType, IEnumerable<TypeEncoding> parameterTypes)
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformBlock(TypeEncoding returnType, IEnumerable<TypeEncoding> parameterTypes)
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformPointer(TypeEncoding target)
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformStruct(string name, string module)
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformUnion(string name, string module)
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformInterfaceDeclaration(string name, string module)
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformAnonymousStruct(IEnumerable<RecordField> fields)
        {
            return this.Default();
        }

        protected internal virtual TReturn TransformAnonymousUnion(IEnumerable<RecordField> fields)
        {
            return this.Default();
        }
    }
}
