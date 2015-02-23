using System;
using YamlDotNet.RepresentationModel;
using System.Linq;
using System.Collections.Generic;

namespace MetadataGenerator.Core
{
    public class YamlTypeEncodingTransfomation : TypeEncodingTransfomation<YamlNode>
    {
        public YamlTypeEncodingTransfomation ()
        {
        }

        protected override internal YamlNode TransformUnknown()
        {
            return this.ConstructTypeEncodingNode("Unknown");
        }

        protected override internal YamlNode TransformVaList()
        {
            return this.ConstructTypeEncodingNode("VaList");
        }

        protected override internal YamlNode TransformProtocol()
        {
            return this.ConstructTypeEncodingNode("Protocol");
        }

        protected override internal YamlNode TransformVoid()
        {
            return this.ConstructTypeEncodingNode("Void");
        }

        protected override internal YamlNode TransformBool()
        {
            return this.ConstructTypeEncodingNode("Bool");
        }

        protected override internal YamlNode TransformShort()
        {
            return this.ConstructTypeEncodingNode("Short");
        }

        protected override internal YamlNode TransformUShort()
        {
            return this.ConstructTypeEncodingNode("Ushort");
        }

        protected override internal YamlNode TransformInt()
        {
            return this.ConstructTypeEncodingNode("Int");
        }

        protected override internal YamlNode TransformUInt()
        {
            return this.ConstructTypeEncodingNode("UInt");
        }

        protected override internal YamlNode TransformLong()
        {
            return this.ConstructTypeEncodingNode("Long");
        }

        protected override internal YamlNode TransformULong()
        {
            return this.ConstructTypeEncodingNode("ULong");
        }

        protected override internal YamlNode TransformLongLong()
        {
            return this.ConstructTypeEncodingNode("LongLong");
        }

        protected override internal YamlNode TransformULongLong()
        {
            return this.ConstructTypeEncodingNode("ULongLong");
        }

        protected override internal YamlNode TransformChar()
        {
            return this.ConstructTypeEncodingNode("Char");
        }

        protected override internal YamlNode TransformUChar()
        {
            return this.ConstructTypeEncodingNode("UChar");
        }

        protected override internal YamlNode TransformUnichar()
        {
            return this.ConstructTypeEncodingNode("Unichar");
        }

        protected override internal YamlNode TransformCharS()
        {
            return this.ConstructTypeEncodingNode("CharS");
        }

        protected override internal YamlNode TransformCString()
        {
            return this.ConstructTypeEncodingNode("CString");
        }

        protected override internal YamlNode TransformFloat()
        {
            return this.ConstructTypeEncodingNode("Float");
        }

        protected override internal YamlNode TransformDouble()
        {
            return this.ConstructTypeEncodingNode("Double");
        }

        protected override internal YamlNode TransformSelector()
        {
            return this.ConstructTypeEncodingNode("Selector");
        }

        protected override internal YamlNode TransformClass()
        {
            return this.ConstructTypeEncodingNode("Class");
        }

        protected override internal YamlNode TransformInstancetype()
        {
            return this.ConstructTypeEncodingNode("Instancetype");
        }

        protected override internal YamlNode TransformId(params Tuple<string, string>[] protocols)
        {
            return this.ConstructTypeEncodingNode("Id", new YamlScalarNode("WithProtocols"), new YamlSequenceNode(protocols.Select(t => 
            new YamlMappingNode( new YamlScalarNode("Module"), new YamlScalarNode(t.Item1), new YamlScalarNode("Name"), new YamlScalarNode(t.Item2) ) )));
        }

        protected override internal YamlNode TransformConstantArray(int size, TypeEncoding elementType)
        {
            return this.ConstructTypeEncodingNode("ConstantArray", new YamlScalarNode("ArrayType"), this.Transform(elementType), new YamlScalarNode("Size"), new YamlScalarNode(size.ToString()));
        }

        protected override internal YamlNode TransformIncompleteArray(TypeEncoding elementType)
        {
            return this.ConstructTypeEncodingNode("IncompleteArray", new YamlScalarNode("ArrayType"), this.Transform(elementType));
        }

        protected override internal YamlNode TransformInterface(string name, string moduleName)
        {
            return this.ConstructTypeEncodingNode("Interface", new YamlScalarNode("Module"), new YamlScalarNode(moduleName), new YamlScalarNode("Name"), new YamlScalarNode(name));
        }

        private IEnumerable<TypeEncoding> CombineParams(TypeEncoding returnType, IEnumerable<TypeEncoding> parameterTypes)
        {
            yield return returnType;
            foreach (TypeEncoding parameterType in parameterTypes)
            {
                yield return parameterType;
            }
        }

        protected override internal YamlNode TransformFunction(TypeEncoding returnType, IEnumerable<TypeEncoding> parameterTypes)
        {
            return this.ConstructTypeEncodingNode("FunctionPointer", new YamlScalarNode("Signature"), new YamlSequenceNode(CombineParams(returnType, parameterTypes).Select(p => this.Transform(p))));
        }

        protected override internal YamlNode TransformBlock(TypeEncoding returnType, IEnumerable<TypeEncoding> parameterTypes)
        {
            return this.ConstructTypeEncodingNode("Block", new YamlScalarNode("Signature"), new YamlSequenceNode(CombineParams(returnType, parameterTypes).Select(p => this.Transform(p))));
        }

        protected override internal YamlNode TransformPointer(TypeEncoding target)
        {
            return this.ConstructTypeEncodingNode("Pointer", new YamlScalarNode("PointerType"), this.Transform(target));
        }

        protected override internal YamlNode TransformStruct(string name, string moduleName)
        {
            return this.ConstructTypeEncodingNode("Struct", new YamlScalarNode("Module"), new YamlScalarNode(moduleName), new YamlScalarNode("Name"), new YamlScalarNode(name));
        }

        protected override internal YamlNode TransformUnion(string name, string moduleName)
        {
            return this.ConstructTypeEncodingNode("Union", new YamlScalarNode("Module"), new YamlScalarNode(moduleName), new YamlScalarNode("Name"), new YamlScalarNode(name));
        }

        protected override internal YamlNode TransformInterfaceDeclaration(string name, string moduleName)
        {
            return this.ConstructTypeEncodingNode("PureInterface", new YamlScalarNode("Module"), new YamlScalarNode(moduleName), new YamlScalarNode("Name"), new YamlScalarNode(name));
        }

        protected override internal YamlNode TransformAnonymousStruct(IEnumerable<RecordField> fields)
        {
            return this.ConstructTypeEncodingNode("AnonymousStruct", new YamlScalarNode("Fields"), SerializeFields(fields));
        }

        protected override internal YamlNode TransformAnonymousUnion(IEnumerable<RecordField> fields)
        {
            return this.ConstructTypeEncodingNode("AnonymousUnion", new YamlScalarNode("Fields"), SerializeFields(fields));
        }

        private YamlSequenceNode SerializeFields(IEnumerable<RecordField> fields)
        {
            YamlSequenceNode node = new YamlSequenceNode();
            foreach (RecordField field in fields)
            {
                YamlMappingNode fieldNode = new YamlMappingNode();
                fieldNode.Add("Name", field.Name);
                fieldNode.Add("Signature", this.Transform(field.TypeEncoding));
                node.Add(fieldNode);
            }
            return node;
        }

        private YamlMappingNode ConstructTypeEncodingNode (string type, params YamlNode[] keyAndValues)
        {
            if (keyAndValues.Length % 2 != 0)
            {
                throw new Exception("Keys and values must be even.");
            }
            YamlMappingNode node = new YamlMappingNode(new YamlScalarNode("Type"), new YamlScalarNode(type));
            for (int i = 0; i < keyAndValues.Length; i += 2) {
                node.Add(keyAndValues[i], keyAndValues[i+1]);
            }
            return node;
        }
    }
}

