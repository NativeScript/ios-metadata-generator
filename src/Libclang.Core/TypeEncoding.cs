using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Libclang.Core
{
    public abstract class TypeEncoding
    {
        public static readonly TypeEncoding Unknown = new UnknownEncoding();
        public static readonly TypeEncoding VaList = new VaListEncoding();
        public static readonly TypeEncoding Protocol = new ProtocolEncoding();
        public static readonly TypeEncoding Void = new VoidEncoding();
        public static readonly TypeEncoding Bool = new BoolEncoding();
        public static readonly TypeEncoding Short = new ShortEncoding();
        public static readonly TypeEncoding UShort = new UShortEncoding();
        public static readonly TypeEncoding Int = new IntEncoding();
        public static readonly TypeEncoding UInt = new UIntEncoding();
        public static readonly TypeEncoding Long = new LongEncoding();
        public static readonly TypeEncoding ULong = new ULongEncoding();
        public static readonly TypeEncoding LongLong = new LongLongEncoding();
        public static readonly TypeEncoding ULongLong = new ULongLongEncoding();
        public static readonly TypeEncoding SignedChar = new SignedCharEncoding();
        public static readonly TypeEncoding UnsignedChar = new UnsignedCharEncoding();
        public static readonly TypeEncoding Unichar = new UnicharEncoding();
        public static readonly TypeEncoding CString = new CStringEncoding();
        public static readonly TypeEncoding Float = new FloatEncoding();
        public static readonly TypeEncoding Double = new DoubleEncoding();
        public static readonly TypeEncoding Selector = new SelectorEncoding();
        public static readonly TypeEncoding Class = new ClassEncoding();
        public static readonly TypeEncoding Instancetype = new InstancetypeEncoding();

        private static readonly TypeEncoding id = new IdEncoding();

        public static TypeEncoding ConstantArray(int size, TypeEncoding elementType)
        {
            return new ConstantArrayEncoding()
            {
                Size = size,
                ElementType = elementType
            };
        }

        public static TypeEncoding IncompleteArray(TypeEncoding elementType)
        {
            return new IncompleteArrayEncoding()
            {
                ElementType = elementType
            };
        }

        public static TypeEncoding Interface(string name)
        {
            return new InterfaceEncoding()
            {
                Name = name
            };
        }


        public static TypeEncoding Id()
        {
            return id;
        }

        public static TypeEncoding Id(IEnumerable<string> protocols)
        {
            if (protocols == null && !protocols.Any())
            {
                return id;
            }
            else
            {
                return new IdEncoding(protocols);
            }
        }

        public static TypeEncoding Call(TypeEncoding returnType, IEnumerable<TypeEncoding> parameterTypes)
        {
            return TypeEncoding.CallPrivate(returnType, parameterTypes);
        }

        private static CallEncoding CallPrivate(TypeEncoding returnType, IEnumerable<TypeEncoding> parameterTypes)
        {
            return new CallEncoding()
            {
                ReturnType = returnType,
                Types = parameterTypes.ToList()
            };
        }

        public static TypeEncoding Function(TypeEncoding returnType, IEnumerable<TypeEncoding> parameterTypes)
        {
            return new FunctionEncoding()
            {
                FunctionCall = TypeEncoding.CallPrivate(returnType, parameterTypes)
            };
        }

        public static TypeEncoding Block(TypeEncoding returnType, IEnumerable<TypeEncoding> parameterTypes)
        {
            return new BlockEncoding()
            {
                BlockCall = TypeEncoding.CallPrivate(returnType, parameterTypes)
            };
        }

        public static TypeEncoding Pointer(TypeEncoding target)
        {
            return new PointerEncoding()
            {
                Target = target
            };
        }

        public static TypeEncoding Struct(string name)
        {
            return new StructEncoding()
            {
                Name = name
            };
        }

        public static TypeEncoding Union(string name)
        {
            return new UnionEncoding()
            {
                Name = name
            };
        }

        public static TypeEncoding InterfaceDeclaration(string name)
        {
            return new InterfaceDeclarationEncoding()
            {
                Name = name
            };
        }

        private static FieldsEncoding FieldsPrivate(IEnumerable<RecordField> fields)
        {
            return new FieldsEncoding()
            {
                Fields = fields.ToList()
            };
        }

        public static TypeEncoding AnonymousStruct(IEnumerable<RecordField> fields)
        {
            return new AnonymousStructEncoding()
            {
                Fields = TypeEncoding.FieldsPrivate(fields)
            };
        }

        public static TypeEncoding AnonymousUnion(IEnumerable<RecordField> fields)
        {
            return new AnonymousUnionEncoding()
            {
                Fields = TypeEncoding.FieldsPrivate(fields)
            };
        }

        public static bool operator ==(TypeEncoding l, TypeEncoding r)
        {
            return Object.Equals(l, r);
        }

        public static bool operator !=(TypeEncoding l, TypeEncoding r)
        {
            return !Object.Equals(l, r);
        }

        private TypeEncoding()
        {
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is TypeEncoding && this.ToString().Equals(obj.ToString());
        }

        public bool IsVoid()
        {
            return this == TypeEncoding.Void;
        }

        public bool IsInstancetype()
        {
            return this == TypeEncoding.Instancetype;
        }

        public abstract T Transform<T>(TypeEncodingTransfomation<T> visitor);

        private class InterfaceEncoding : TypeEncoding
        {
            public string Name { get; set; }

            public override string ToString()
            {
                return string.Format("@\"{0}\"", this.Name);
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformInterface(this.Name);
            }
        }

        private class PointerEncoding : TypeEncoding
        {
            public TypeEncoding Target { get; set; }

            public override string ToString()
            {
                return string.Format("^{0}", this.Target);
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformPointer(this.Target);
            }
        }

        private class ConstantArrayEncoding : TypeEncoding
        {
            public int Size { get; set; }
            public TypeEncoding ElementType { get; set; }

            public override string ToString()
            {
                return string.Format("[{0}{1}]", this.Size, this.ElementType);
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformConstantArray(this.Size, this.ElementType);
            }
        }

        private class IncompleteArrayEncoding : TypeEncoding
        {
            public TypeEncoding ElementType { get; set; }

            public override string ToString()
            {
                return string.Format("[{0}]", this.ElementType);
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformIncompleteArray(this.ElementType);
            }
        }

        private class CallEncoding : TypeEncoding
        {
            public TypeEncoding ReturnType { get; set; }
            public IEnumerable<TypeEncoding> Types { get; set; }

            public override string ToString()
            {
                var typesEncoding = string.Join(null, this.Types.Select(t => t.ToString()));
                var functionEncoding = string.Format("{0}{1}", this.ReturnType, typesEncoding);
                return functionEncoding;
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                throw new NotSupportedException();
            }
        }

        private class BlockEncoding : TypeEncoding
        {
            public CallEncoding BlockCall { get; set; }

            public override string ToString()
            {
                return string.Format("%{0}|", this.BlockCall);
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformBlock(this.BlockCall.ReturnType, this.BlockCall.Types);
            }
        }

        private class FunctionEncoding : TypeEncoding
        {
            public CallEncoding FunctionCall { get; set; }

            public override string ToString()
            {
                return string.Format("/{0}|", this.FunctionCall);
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformFunction(this.FunctionCall.ReturnType, this.FunctionCall.Types);
            }
        }

        private class StructEncoding : TypeEncoding
        {
            public string Name { get; set; }

            public override string ToString()
            {
                return string.Format("{{{0}}}", this.Name);
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformStruct(this.Name);
            }
        }

        private class UnionEncoding : TypeEncoding
        {
            public string Name { get; set; }

            public override string ToString()
            {
                return string.Format("({0})", this.Name);
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformUnion(this.Name);
            }
        }

        private class UnknownEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "?";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformUnknown();
            }
        }

        private class VaListEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "~";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformVaList();
            }
        }

        private class ProtocolEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "P";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformProtocol();
            }
        }

        private class VoidEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "v";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformVoid();
            }
        }

        private class BoolEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "B";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformBool();
            }
        }

        private class ShortEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "s";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformShort();
            }
        }

        private class UShortEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "S";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformUShort();
            }
        }

        private class IntEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "i";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformInt();
            }
        }

        private class UIntEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "I";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformUInt();
            }
        }

        private class LongEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "l";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformLong();
            }
        }

        private class ULongEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "L";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformULong();
            }
        }

        private class LongLongEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "q";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformLongLong();
            }
        }

        private class ULongLongEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "Q";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformULongLong();
            }
        }

        private class SignedCharEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "c";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformChar();
            }
        }

        private class UnsignedCharEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "C";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformUChar();
            }
        }

        private class UnicharEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "U";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformUnichar();
            }
        }

        private class CStringEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "*";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformCString();
            }
        }

        private class FloatEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "f";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformFloat();
            }
        }

        private class DoubleEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "d";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformDouble();
            }
        }

        private class SelectorEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return ":";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformSelector();
            }
        }

        private class ClassEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "#";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformClass();
            }
        }

        private class InstancetypeEncoding : TypeEncoding
        {
            public override string ToString()
            {
                return "&";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformInstancetype();
            }
        }

        private class IdEncoding : TypeEncoding
        {
            private string[] protocols;

            public IdEncoding()
            {
            }

            public IdEncoding(IEnumerable<string> protocols)
            {
                this.protocols = protocols.ToArray();
            }

            public override string ToString()
            {
                // NOTE: At runtime we do not use the implemented protocols.
                return "@";
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformId(this.protocols);
            }
        }

        private class InterfaceDeclarationEncoding : TypeEncoding
        {
            public string Name { get; set; }

            public override string ToString()
            {
                return string.Format("{{{0}=#}}", this.Name);
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformInterfaceDeclaration(this.Name);
            }
        }

        private class FieldsEncoding : TypeEncoding
        {
            public IEnumerable<RecordField> Fields { get; set; }

            public override string ToString()
            {
                StringBuilder recordEncoding = new StringBuilder("?=");
                foreach (RecordField field in this.Fields)
                {
                    recordEncoding.Append(field.TypeEncoding);
                }
                recordEncoding.Append(',');
                recordEncoding.Append(string.Join(",", this.Fields.Select(f => f.Name)));
                recordEncoding.Append(',');
                var encoding = recordEncoding.ToString();
                return encoding;
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                throw new NotSupportedException();
            }
        }

        private class AnonymousStructEncoding : TypeEncoding
        {
            public FieldsEncoding Fields { get; set; }

            public override string ToString()
            {
                return string.Format("{{{0}}}", this.Fields);
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformAnonymousStruct(this.Fields.Fields);
            }
        }

        private class AnonymousUnionEncoding : TypeEncoding
        {
            public FieldsEncoding Fields { get; set; }

            public override string ToString()
            {
                return string.Format("({0})", this.Fields);
            }

            public override T Transform<T>(TypeEncodingTransfomation<T> visitor)
            {
                return visitor.TransformAnonymousUnion(this.Fields.Fields);
            }
        }
    }
}
