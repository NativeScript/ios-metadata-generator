using MetadataGenerator.Core;
using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Common;
using MetadataGenerator.Core.Generator;
using MetadataGenerator.Core.Meta.Utils;
using MetadataGenerator.Core.Meta.Visitors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;
using System.Diagnostics;

namespace MetadataGenerator.Core.Meta.Visitors
{
    internal class YamlSerializer : IMetaContainerDeclarationVisitor
    {
        public YamlSerializer()
        {
            this.stream = new YamlStream();
        }

        private YamlStream stream;
        private YamlMappingNode rootNode;
        private YamlSequenceNode itemsNode;
        private YamlTypeEncodingTransfomation typeEncodingTransformation = new YamlTypeEncodingTransfomation();

        public void Begin(Utils.ModuleDeclarationsContainer metaContainer)
        {
            this.rootNode = new YamlMappingNode();
            rootNode.Add("name", metaContainer.ModuleName);
            this.itemsNode = new YamlSequenceNode();
            rootNode.Add("items", this.itemsNode);
        }

        public void End(Utils.ModuleDeclarationsContainer metaContainer)
        {
            this.stream.Add(new YamlDocument(this.rootNode));
        }

        public void Save(string outputFolder, string filename)
        {
            string finalName = Path.Combine(outputFolder, Path.ChangeExtension(filename, ".yaml"));
            using (FileStream fs = File.Open(finalName, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    stream.Save(writer);
                }
            }
        }

        public void Visit(Ast.InterfaceDeclaration declaration)
        {
            YamlMappingNode node = CreateClassNode(declaration);
            //node.Anchor = declaration.GetJSName();
            if (declaration.Base != null)
            {
                node.Add("Base", new YamlMappingNode(
                    new YamlScalarNode("Module"),
                    new YamlScalarNode(declaration.Base.Module.FullName),
                    new YamlScalarNode("Name"),
                    new YamlScalarNode(declaration.Base.GetJSName())));
            }
            else
            {
                node.Add("Base", "");
            }
            node.Add("Type", "Interface");
            this.itemsNode.Add(node);
        }

        public void Visit(Ast.ProtocolDeclaration declaration)
        {
            YamlMappingNode node = CreateClassNode(declaration);
            //node.Anchor = declaration.GetJSName();
            node.Add("Type", "Protocol");
            this.itemsNode.Add(node);
        }

        public void Visit(Ast.CategoryDeclaration declaration)
        {
            YamlMappingNode node = CreateClassNode(declaration);
            node.Add("ExtendedInterface", new YamlMappingNode( 
                new YamlScalarNode("Module"), 
                new YamlScalarNode(declaration.ExtendedInterface.Module.FullName),
                new YamlScalarNode("Name"), 
                new YamlScalarNode(declaration.ExtendedInterface.GetJSName())) );
            node.Add("Type", "Category");
            this.itemsNode.Add(node);
        }

        public void Visit(StructDeclaration declaration)
        {
            YamlMappingNode node = CreateRecordNode(declaration);
            node.Add("Type", "Struct");
            this.itemsNode.Add(node);
        }

        public void Visit(UnionDeclaration declaration)
        {
            YamlMappingNode node = CreateRecordNode(declaration);
            node.Add("Type", "Union");
            this.itemsNode.Add(node);
        }

        public void Visit(Ast.FieldDeclaration declaration)
        {
        }

        public void Visit(Ast.EnumDeclaration declaration)
        {
            YamlMappingNode node = CreateBaseNode(declaration);

            string json = String.Format("{{{0}}}", String.Join(",", declaration.Fields.Select(f => String.Format("\"{0}\":{1}", f.Name, f.Value))));
            string jsCode = String.Format("__tsEnum({0})", json);

            node.Add("Type", "JsCode");
            node.Add("JsCode", jsCode);

            this.itemsNode.Add(node);
        }

        public void Visit(Ast.EnumMemberDeclaration declaration)
        {
        }

        public void Visit(Ast.FunctionDeclaration declaration)
        {
            YamlMappingNode node = CreateBaseNode(declaration);
            node.Add("Type", "Function");
            node.Add("Signature", new YamlSequenceNode(declaration.GetExtendedEncoding().Select(e => this.typeEncodingTransformation.Transform(e))));

            // set flags
            YamlSequenceNode flags = GetFlags(declaration);

            if (declaration.IsVariadic)
                flags.Add("FunctionIsVariadic");
            if (declaration.OwnsReturnedCocoaObject.GetValueOrDefault())
                flags.Add("FunctionOwnsReturnedCocoaObject");

            this.itemsNode.Add(node);
        }

        public void Visit(Ast.MethodDeclaration declaration)
        {
        }

        public void Visit(Ast.ParameterDeclaration declaration)
        {
        }

        public void Visit(Ast.PropertyDeclaration declaration)
        {
        }

        public void Visit(Ast.ModuleDeclaration declaration)
        {
        }

        public void Visit(Ast.VarDeclaration declaration)
        {
            YamlMappingNode node = CreateBaseNode(declaration);
            if (declaration.GetValue() != null)
            {
                node.Add("Type", "JsCode");
                node.Add("JsCode", declaration.GetValue().ToString());
            }
            else
            {
                node.Add("Type", "Var");
                node .Add("Signature", this.typeEncodingTransformation.Transform(declaration.GetExtendedEncoding()));
            }

            this.itemsNode.Add(node);
        }

        public void Visit(Ast.TypedefDeclaration declaration)
        {
        }

        protected static YamlSequenceNode GetFlags(BaseDeclaration declaration)
        {
            object flagsNode;
            if (!declaration.MetadataPropertyBag.TryGetValue("flags", out flagsNode))
            {
                flagsNode = new YamlSequenceNode();
                declaration.MetadataPropertyBag.Add("flags", flagsNode);
            }
            return flagsNode as YamlSequenceNode;
        }

        protected YamlMappingNode CreateBaseNode(BaseDeclaration declaration)
        {
            YamlMappingNode node = new YamlMappingNode();

            // Names
            node.Add("Name", SerializeString(declaration.Name));
            node.Add("JsName", SerializeString(declaration.GetJSName()));

            // Module
            if (declaration.Module != null)
            {
                node.Add("Module", SerializeString(declaration.Module.FullName));
            }

            // Availability
            if (declaration.IosAvailability != null)
            {
                node.Add("IntroducedIn", declaration.IosAvailability.Introduced.ToString());
            }

            // Flags
            YamlSequenceNode flagsNode = GetFlags(declaration);
            node.Add("Flags", flagsNode);

            if (!(declaration.IosAppExtensionAvailability != null && !declaration.IosAppExtensionAvailability.IsUnavailable))
            {
                flagsNode.Add("IsIosAppExtensionAvailable");
            }

            return node;
        }

        protected YamlMappingNode CreateClassNode(BaseClass declaration)
        {
            YamlMappingNode node = CreateBaseNode(declaration);

            IEnumerable<YamlMappingNode> instanceMethodsList = declaration.InstanceMethods()
                .OrderBy(m => m.GetJSName())
                .Select(m => CreateMethodNode(m));

            IEnumerable<YamlMappingNode> staticMethodsList = declaration.StaticMethods()
                .OrderBy(m => m.GetJSName())
                .Select(m => CreateMethodNode(m));

            IEnumerable<YamlMappingNode> propertiesList = declaration.Properties
                .OrderBy(p => p.GetJSName())
                .Select(p => CreatePropertyNode(p));

            IEnumerable<YamlMappingNode> protocolsList = declaration.ImplementedProtocols
                .OrderBy(p => p.GetJSName())
                .Distinct()
                .Select(p => new YamlMappingNode( 
                new YamlScalarNode("Module"), 
                new YamlScalarNode(p.Module.FullName),
                new YamlScalarNode("Name"), 
                new YamlScalarNode(p.GetJSName())) );

            node.Add("InstanceMethods", new YamlSequenceNode(instanceMethodsList));
            node.Add("StaticMethods", new YamlSequenceNode(staticMethodsList));
            node.Add("Properties", new YamlSequenceNode(propertiesList));
            node.Add("Protocols", new YamlSequenceNode(protocolsList));

            return node;
        }

        protected YamlMappingNode CreateMethodNode(MethodDeclaration declaration)
        {
            YamlMappingNode node = CreateBaseNode(declaration);
            //node.Anchor = declaration.GetJSName();

            node.Add("Selector", SerializeString(declaration.Selector));
            node.Add("Signature", new YamlSequenceNode(declaration.GetExtendedEncoding().Select(e => this.typeEncodingTransformation.Transform(e))) );
            node.Add("TypeEncoding", SerializeString(declaration.TypeEncoding));

            // set flags
            YamlSequenceNode flags = GetFlags(declaration);

            if (declaration.IsVariadic)
                flags.Add("MethodIsVariadic");
            if (declaration.IsNilTerminatedVariadic)
                flags.Add("MethodIsNullTerminatedVariadic");
            if (declaration.OwnsReturnedCocoaObject.GetValueOrDefault())
                flags.Add("MethodOwnsReturnedCocoaObject");

            return node;
        }

        protected YamlMappingNode CreatePropertyNode(PropertyDeclaration declaration)
        {
            YamlMappingNode node = CreateBaseNode(declaration);

            if (declaration.Getter != null)
            {
                node.Add("Getter", this.CreateMethodNode(declaration.Getter));
            }
            if (declaration.Setter != null)
            {
                node.Add("Setter", this.CreateMethodNode(declaration.Setter));
            }

            // set flags
            YamlSequenceNode flags = GetFlags(declaration);

            if (declaration.Getter != null)
                flags.Add("PropertyHasGetter");
            if (declaration.Setter != null)
                flags.Add("PropertyHasSetter");

            return node;
        }

        protected YamlMappingNode CreateRecordNode(BaseRecordDeclaration declaration)
        {
            YamlMappingNode node = CreateBaseNode(declaration);
            YamlSequenceNode fields = new YamlSequenceNode();

            foreach (FieldDeclaration field in declaration.Fields)
            {
                YamlMappingNode fieldNode = new YamlMappingNode();
                fieldNode.Add("Name", field.GetJSName());
                fieldNode.Add("Signature", field.GetExtendedEncoding().Transform(this.typeEncodingTransformation));
                fields.Add(fieldNode);
            }

            node.Add("Fields", fields);
            return node;
        }

        private YamlScalarNode SerializeString(string value)
        {
            return new YamlScalarNode(value) { Style = YamlDotNet.Core.ScalarStyle.SingleQuoted };
        }
    }
}
