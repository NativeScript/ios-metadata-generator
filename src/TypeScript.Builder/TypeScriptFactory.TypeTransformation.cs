namespace TypeScript.Factory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Libclang.Core;
    using TS = TypeScript.Declarations.Model;

    public partial class TypeScriptFactory
    {
        private static TypeEncoding NSString = TypeEncoding.Interface("NSString");

        private class MetaToTypeScriptTypeTransform : TypeEncodingTransfomation<TS.IType>
        {
            private TypeScriptFactory builder;

            public MetaToTypeScriptTypeTransform(TypeScriptFactory builder)
            {
                this.builder = builder;
            }

            protected override TS.IType TransformUnknown()
            {
                return TS.PrimitiveTypes.Any;
            }

            protected override TS.IType TransformVaList()
            {
                throw new NotSupportedException();
            }

            protected override TS.IType TransformProtocol()
            {
                return TS.PrimitiveTypes.Any;
            }

            protected override TS.IType TransformVoid()
            {
                return TS.PrimitiveTypes.Void;
            }

            protected override TS.IType TransformBool()
            {
                return TS.PrimitiveTypes.Boolean;
            }

            protected override TS.IType TransformShort()
            {
                return TS.PrimitiveTypes.Number;
            }

            protected override TS.IType TransformUShort()
            {
                return TS.PrimitiveTypes.Number;
            }

            protected override TS.IType TransformInt()
            {
                return TS.PrimitiveTypes.Number;
            }

            protected override TS.IType TransformUInt()
            {
                return TS.PrimitiveTypes.Number;
            }

            protected override TS.IType TransformLong()
            {
                return TS.PrimitiveTypes.Number;
            }

            protected override TS.IType TransformULong()
            {
                return TS.PrimitiveTypes.Number;
            }

            protected override TS.IType TransformLongLong()
            {
                return TS.PrimitiveTypes.Number;
            }

            protected override TS.IType TransformULongLong()
            {
                return TS.PrimitiveTypes.Number;
            }

            protected override TS.IType TransformChar()
            {
                return TS.PrimitiveTypes.String;
            }

            protected override TS.IType TransformUChar()
            {
                return TS.PrimitiveTypes.Number;
            }

            protected override TS.IType TransformUnichar()
            {
                return TS.PrimitiveTypes.String;
            }

            protected override TS.IType TransformCharS()
            {
                return TS.PrimitiveTypes.Number;
            }

            protected override TS.IType TransformCString()
            {
                return TS.PrimitiveTypes.String;
            }

            protected override TS.IType TransformFloat()
            {
                return TS.PrimitiveTypes.Number;
            }

            protected override TS.IType TransformDouble()
            {
                return TS.PrimitiveTypes.Number;
            }

            protected override TS.IType TransformSelector()
            {
                return TS.PrimitiveTypes.Any;
            }

            protected override TS.IType TransformClass()
            {
                return TS.PrimitiveTypes.Any;
            }

            protected override TS.IType TransformInstancetype()
            {
                // NOTE: This will be handled by a custom mechanism that overrides the methods with return type of instancetype promoting their return type.
                return TS.PrimitiveTypes.Any;
            }

            protected override TS.IType TransformId(params string[] protocols)
            {
                if (protocols == null || protocols.Length != 1)
                {
                    // NOTE: We either have no protocols and are simple id, or have multiple protocols we can not handle.
                    return TS.PrimitiveTypes.Any;
                }
                else
                {
                    var name = protocols[0];
                    var protocol = this.builder.GetTypeForProtocolMetaName(name);
                    return protocol;
                }
            }

            protected override TS.IType TransformConstantArray(int size, TypeEncoding elementType)
            {
                return new TS.ArrayType() { ComponentType = this.Transform(elementType) };
            }

            protected override TS.IType TransformIncompleteArray(TypeEncoding elementType)
            {
                return TS.PrimitiveTypes.Any;
            }

            protected override TS.IType TransformInterface(string name)
            {
                if (name == "NSString")
                {
                    return TS.PrimitiveTypes.String;
                }
                else
                {
                    return this.builder.GetTypeForInterfaceMetaName(name);
                }
            }

            protected override TS.IType TransformFunction(TypeEncoding returnType, IEnumerable<TypeEncoding> parameterTypes)
            {
                var functionType = new TS.FunctionType();
                functionType.ReturnType = this.Transform(returnType);
                this.AddParams(functionType.Parameters, parameterTypes);
                return functionType;
            }

            protected override TS.IType TransformBlock(TypeEncoding returnType, IEnumerable<TypeEncoding> parameterTypes)
            {
                var functionType = new TS.FunctionType();
                functionType.ReturnType = this.Transform(returnType);
                this.AddParams(functionType.Parameters, parameterTypes);
                return functionType;
            }

            protected override TS.IType TransformPointer(TypeEncoding target)
            {
                if (target.IsVoid())
                {
                    return TS.PrimitiveTypes.Any;
                }
                else
                {
                    return TS.PrimitiveTypes.Any;
                }
            }

            protected override TS.IType TransformStruct(string name)
            {
                return this.builder.GetTypeForStructMetaName(name);
            }

            protected override TS.IType TransformUnion(string name)
            {
                return this.builder.GetTypeForUnionMetaName(name);
            }

            protected override TS.IType TransformInterfaceDeclaration(string name)
            {
                return TS.PrimitiveTypes.Any;
            }

            protected override TS.IType TransformAnonymousStruct(IEnumerable<RecordField> fields)
            {
                var str = new TS.ObjectType();
                str.Properties.AddRange(fields.Select(this.CreateProperty));
                return str;
            }

            protected override TS.IType TransformAnonymousUnion(IEnumerable<RecordField> fields)
            {
                return TS.PrimitiveTypes.Any;
            }

            private void AddParams(IList<TS.Parameter> parameters, IEnumerable<TypeEncoding> parameterTypes)
            {
                var index = 0;
                foreach (var param in parameterTypes)
                {
                    parameters.Add(new TS.Parameter()
                    {
                        Name = "arg" + ++index,
                        TypeAnnotation = this.Transform(param)
                    });
                }
            }

            private TS.PropertySignature CreateProperty(RecordField field)
            {
                return new TS.PropertySignature()
                { 
                    Name = field.Name,
                    TypeAnnotation = this.Transform(field.TypeEncoding)
                };
            }
        }
    }
}
