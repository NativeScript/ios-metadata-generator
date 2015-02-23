using Libclang.Core.Ast;
using Libclang.Core.Common;
using Libclang.Core.Generator;
using Libclang.Core.Meta.Utils;
using Libclang.Core.Meta.Visitors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Libclang.Core.Meta.Utils
{
    internal class BinarySerializer : IMetaContainerDeclarationVisitor
    {
        private readonly MetaFile file;
        private readonly string outputFolder;
        private readonly string outputfileName;
        private readonly BinaryEncodingTransformation typeEncodingTransformation = new BinaryEncodingTransformation();

        public BinarySerializer(string outputFolder, string fileName)
        {
            this.file = new MetaFile();
            this.outputFolder = outputFolder;
            this.outputfileName = fileName;
        }

        public void Begin(ModuleDeclarationsContainer metaContainer)
        {
        }

        public void End(ModuleDeclarationsContainer metaContainer)
        {
            this.Save(outputFolder, outputfileName);
        }

        public void Save(string folderPath, string outputFileName)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            this.file.SaveAs(Path.Combine(folderPath, Path.ChangeExtension(outputFileName, "bin")));
        }

        public void Visit(InterfaceDeclaration declaration)
        {
            BinarySymbol structure = this.GetClassMetaStructure(declaration);
            structure.Type = BinarySymbol.SymbolType.Interface;
            this.file.AddSymbol(structure);
        }

        public void Visit(ProtocolDeclaration declaration)
        {
            BinarySymbol structure = this.GetClassMetaStructure(declaration);
            structure.Type = BinarySymbol.SymbolType.Protocol;
            this.file.AddSymbol(structure);
        }

        public void Visit(CategoryDeclaration declaration)
        {
            BinarySymbol structure = this.GetClassMetaStructure(declaration);
            structure.Type = BinarySymbol.SymbolType.Category;
            this.file.AddSymbol(structure);
        }

        public void Visit(StructDeclaration declaration)
        {
            BinarySymbol structure = this.GetRecordMetaStructure(declaration);
            structure.Type = BinarySymbol.SymbolType.Struct;
            this.file.AddSymbol(structure);
        }

        public void Visit(UnionDeclaration declaration)
        {
            BinarySymbol structure = this.GetRecordMetaStructure(declaration);
            structure.Type = BinarySymbol.SymbolType.Union;
            this.file.AddSymbol(structure);
        }

        public void Visit(FieldDeclaration declaration)
        {
        }

        public void Visit(EnumDeclaration declaration)
        {
            BinarySymbol structure = this.GetBinaryStructure(declaration);

            string json = String.Format("{{{0}}}", String.Join(",", declaration.Fields.Select(f => String.Format("\"{0}\":{1}", f.Name, f.Value))));
            string jsCode = String.Format("__tsEnum({0})", json);
            structure.ChangeToJsCode(jsCode);

            this.file.AddSymbol(structure);
        }

        public void Visit(EnumMemberDeclaration declaration)
        {
        }

        public void Visit(FunctionDeclaration declaration)
        {
            BinarySymbol structure = this.GetBinaryStructure(declaration);
            structure.Type = BinarySymbol.SymbolType.Function;
            structure.Info = new NotCalculatedOffset(declaration.GetExtendedEncoding().Select(te => te.Transform<BinaryTypeEncoding>(typeEncodingTransformation)));

            if (declaration.IsVariadic)
                structure.Flags |= BinarySymbol.MetaFlags.FunctionIsVariadic;
            if (declaration.OwnsReturnedCocoaObject.GetValueOrDefault())
                structure.Flags |= BinarySymbol.MetaFlags.FunctionOwnsReturnedCocoaObject;

            this.file.AddSymbol(structure);
        }

        public void Visit(MethodDeclaration declaration)
        {
        }

        public void Visit(ParameterDeclaration declaration)
        {
        }

        public void Visit(PropertyDeclaration declaration)
        {
        }

        public void Visit(ModuleDeclaration declaration)
        {
        }

        public void Visit(VarDeclaration declaration)
        {
            BinarySymbol structure = this.GetBinaryStructure(declaration);
            if (declaration.GetValue() != null)
            {
                structure = structure.ChangeToJsCode(declaration.GetValue().ToString());
            }
            else
            {
                structure.Type = BinarySymbol.SymbolType.Var;
                structure.Info = new NotCalculatedOffset(declaration.GetExtendedEncoding().Transform<BinaryTypeEncoding>(typeEncodingTransformation));
            }

            this.file.AddSymbol(structure);
        }

        public void Visit(TypedefDeclaration declaration)
        {
        }

        protected BinarySymbol GetBinaryStructure(BaseDeclaration declaration)
        {
            BinarySymbol structure = new BinarySymbol();
            // Names
            structure.Name = declaration.Name;
            structure.JsName = declaration.GetJSName();

            // Module
            if (declaration.Module != null)
            {
                structure.Module = declaration.Module.FullName;
            }

            // Availability
            if (declaration.IosAvailability != null)
            {
                structure.IntrducedIn = declaration.IosAvailability.Introduced;
            }
            
            if (!(declaration.IosAppExtensionAvailability != null && !declaration.IosAppExtensionAvailability.IsUnavailable))
                structure.Flags |= BinarySymbol.MetaFlags.IsIosAppExtensionAvailable;

            return structure;
        }

        protected BinarySymbol GetClassMetaStructure(BaseClass declaration)
        {
            BinarySymbol structure = GetBinaryStructure(declaration);
            StringAsciiComparer comparer = new StringAsciiComparer();

            List<MethodDeclaration> instanceMethodsList = declaration.InstanceMethods().OrderBy(m => m.GetJSName(), comparer).ToList();
            int firstInitializerIndex = -1;
            for (int i = 0; i < instanceMethodsList.Count; i++)
            {
                if (instanceMethodsList[i].Name.StartsWith("init"))
                {
                    firstInitializerIndex = i;
                    break;
                }
            }

            BinaryArray<NotCalculatedOffset> instanceMethodsStructuresList = new BinaryArray<NotCalculatedOffset>(instanceMethodsList
                .Select(m => new NotCalculatedOffset(GetMethodMetaStructure(m))));

            BinaryArray<NotCalculatedOffset> staticMethodsList = new BinaryArray<NotCalculatedOffset>(declaration
                .StaticMethods()
                .OrderBy(m => m.GetJSName(), comparer)
                .Select(m => new NotCalculatedOffset(GetMethodMetaStructure(m))));

            BinaryArray<NotCalculatedOffset> propertiesList = new BinaryArray<NotCalculatedOffset>(declaration.Properties
                .OrderBy(p => p.GetJSName(), comparer)
                .Select(p => new NotCalculatedOffset(GetPropertyMetaStructure(p))));

            BinaryArray<NotCalculatedOffset> protocolsList = new BinaryArray<NotCalculatedOffset>(declaration.ImplementedProtocols
                .Select(p => p.GetJSName())
                .OrderBy(p => p, comparer)
                .Distinct()
                .Select(p => new NotCalculatedOffset(p)));

            List<object> membersLists = new List<object>()
            {
                new NotCalculatedOffset(instanceMethodsStructuresList),
                new NotCalculatedOffset(staticMethodsList),
                new NotCalculatedOffset(propertiesList),
                new NotCalculatedOffset(protocolsList),
                (short) firstInitializerIndex
            };

            structure.Info = membersLists;
            return structure;
        }

        protected BinarySymbol GetMethodMetaStructure(MethodDeclaration declaration)
        {
            BinarySymbol structure = this.GetBinaryStructure(declaration);

            structure.Info = new BinaryArray<NotCalculatedOffset>(new NotCalculatedOffset[] {
                new NotCalculatedOffset(declaration.Selector),
                new NotCalculatedOffset(declaration.GetExtendedEncoding().Select(te => te.Transform<BinaryTypeEncoding>(typeEncodingTransformation))),
                new NotCalculatedOffset(declaration.TypeEncoding)
            }, false);

            // set flags
            if (declaration.IsVariadic)
                structure.Flags |= BinarySymbol.MetaFlags.MethodIsVariadic;
            if (declaration.IsNilTerminatedVariadic)
                structure.Flags |= BinarySymbol.MetaFlags.MethodIsNullTerminatedVariadic;
            if (declaration.OwnsReturnedCocoaObject.GetValueOrDefault())
                structure.Flags |= BinarySymbol.MetaFlags.MethodOwnsReturnedCocoaObject;

            return structure;
        }

        protected BinarySymbol GetPropertyMetaStructure(PropertyDeclaration declaration)
        {
            BinarySymbol structure = this.GetBinaryStructure(declaration);

            BinaryArray<NotCalculatedOffset> propertyInfo = new BinaryArray<NotCalculatedOffset>(false);
            if (declaration.Getter != null)
            {
                propertyInfo.Add(new NotCalculatedOffset(this.GetMethodMetaStructure(declaration.Getter)));
            }
            if (declaration.Setter != null)
            {
                propertyInfo.Add(new NotCalculatedOffset(this.GetMethodMetaStructure(declaration.Setter)));
            }

            structure.Info = propertyInfo;

            // set flags
            if (declaration.Getter != null)
                structure.Flags |= BinarySymbol.MetaFlags.PropertyHasGetter;
            if (declaration.Setter != null)
                structure.Flags |= BinarySymbol.MetaFlags.PropertyHasSetter;

            return structure;
        }

        protected BinarySymbol GetRecordMetaStructure(BaseRecordDeclaration declaration)
        {
            BinarySymbol structure = this.GetBinaryStructure(declaration);

            NotCalculatedOffset fieldsEncoding = new NotCalculatedOffset(declaration.GetExtendedEncoding().Select(te => te.Transform<BinaryTypeEncoding>(typeEncodingTransformation)));
            IEnumerable<NotCalculatedOffset> fieldsNames = declaration.Fields.Select(f => new NotCalculatedOffset(f.GetJSName()));

            List<object> array = new List<object>();
            array.Add(fieldsEncoding);
            array.Add(new BinaryArray<NotCalculatedOffset>(fieldsNames));

            structure.Info = array;
            return structure;
        }

        internal class StringAsciiComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return String.Compare(x, y, StringComparison.Ordinal);
            }
        }
    }
}
