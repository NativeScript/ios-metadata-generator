using MetadataGenerator.Core.Meta.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Meta;
using MetadataGenerator.Core.Generator;

namespace MetadataGenerator.Core.Meta.Filters
{
    internal class DebugSerializer : BaseMetaFilter
    {
        public DebugSerializer(string outputFilePath)
            : base(null)
        {
            this.OutputFilePath = outputFilePath;
        }

        public string FolderPath { get; private set; }

        public string OutputFilePath { get; private set; }

        protected override Action<ModuleDeclarationsContainer> ActionForContainer
        {
            get { return this.SerializeMeta; }
        }

        private void SerializeMeta(ModuleDeclarationsContainer metaContainer)
        {
            var protocols = metaContainer.OfType<ProtocolDeclaration>();
            var interfaces = metaContainer.OfType<InterfaceDeclaration>();
            var structs = metaContainer.OfType<StructDeclaration>();
            var unions = metaContainer.OfType<UnionDeclaration>();
            var enums = metaContainer.OfType<EnumDeclaration>();
            var functions = metaContainer.OfType<FunctionDeclaration>();
            var vars = metaContainer.OfType<VarDeclaration>();

            JObject meta = new JObject();
            meta.Add("protocols", SerializeProtocols(protocols.OrderBy(c => c.GetJSName())));
            meta.Add("interfaces", SerializeInterfaces(interfaces.OrderBy(c => c.GetJSName())));
            meta.Add("structs", SerializeRecords(structs.OrderBy(c => c.GetJSName())));
            meta.Add("unions", SerializeRecords(unions.OrderBy(c => c.GetJSName())));
            meta.Add("enums", SerializeEnums(enums.OrderBy(c => c.GetJSName())));
            meta.Add("functions", SerializeFunctions(functions.OrderBy(c => c.GetJSName())));
            meta.Add("vars", SerializeVars(vars.OrderBy(c => c.GetJSName())));

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            using (StreamWriter file = File.CreateText(this.OutputFilePath))
            {
                using (JsonWriter writer = new JsonTextWriter(file) {Formatting = Formatting.Indented})
                {
                    meta.WriteTo(writer);
                }
            }
        }

        private void AddCommonProperties(JObject @object, BaseDeclaration meta)
        {
            @object.Add("Name", meta.GetJSName());
            JObject availability = new JObject();
            if (meta.IosAvailability != null)
            {
                if (meta.IosAvailability.Introduced != null)
                {
                    availability.Add("IntroducedIn", meta.IosAvailability.Introduced.ToString());
                }
                if (meta.IosAvailability.Obsoleted != null)
                {
                    availability.Add("ObsoletedIn", meta.IosAvailability.Obsoleted.ToString());
                }
                if (meta.IosAvailability.Deprecated != null)
                {
                    availability.Add("DeprecatedIn", meta.IosAvailability.Deprecated.ToString());
                }
            }
            if (availability.HasValues)
            {
                @object.Add("Availability", availability);
            }
        }

        private JArray SerializeProtocols(IEnumerable<IDeclaration> declarations)
        {
            JArray array = new JArray();
            foreach (ProtocolDeclaration @protocol in declarations)
            {
                JObject jMeta = SerializeClass(@protocol);
                array.Add(jMeta);
            }
            return array;
        }

        private JArray SerializeInterfaces(IEnumerable<IDeclaration> declarations)
        {
            JArray array = new JArray();
            foreach (InterfaceDeclaration @interface in declarations)
            {
                JObject jMeta = SerializeClass(@interface);
                if (@interface.Base != null)
                    jMeta.Add("Base", @interface.Base.GetJSName());
                if (@interface.Categories.Count() > 0)
                    jMeta.Add("Categories",
                        JToken.FromObject(@interface.Categories.OrderBy(c => c.GetJSName()).Select(c => SerializeClass(c))));
                // TODO: Remove categories from dump. Only keep their names
                array.Add(jMeta);
            }
            return array;
        }

        private JArray SerializeRecords(IEnumerable<IDeclaration> declarations)
        {
            JArray array = new JArray();
            foreach (BaseRecordDeclaration @record in declarations)
            {
                JObject jMeta = new JObject();
                AddCommonProperties(jMeta, @record);
                jMeta.Add("ExtendedEncoding", String.Join(String.Empty, @record.GetExtendedEncoding().Select(e => e.ToString())));
                if (@record.Fields.Count > 0)
                    jMeta.Add("Fields", JToken.FromObject(@record.Fields.Select(c => SerializeField(c))));
                jMeta.Add("Framework", @record.Module.Name);
                array.Add(jMeta);
            }
            return array;
        }

        private JArray SerializeEnums(IEnumerable<IDeclaration> declarations)
        {
            JArray array = new JArray();
            foreach (EnumDeclaration @enum in declarations)
            {
                JObject jMeta = new JObject();
                AddCommonProperties(jMeta, @enum);
                if (@enum.Fields.Count > 0)
                    jMeta.Add("Members", JToken.FromObject(@enum.Fields.Select(c => SerializeEnumField(c))));
                jMeta.Add("Framework", @enum.Module.Name);
                array.Add(jMeta);
            }
            return array;
        }

        private JArray SerializeFunctions(IEnumerable<IDeclaration> declarations)
        {
            JArray array = new JArray();
            foreach (FunctionDeclaration @function in declarations)
            {
                JObject jMeta = new JObject();
                AddCommonProperties(jMeta, @function);
                jMeta.Add("ExtendedEncoding", Convert.ToString(@function.GetExtendedEncoding()));
                jMeta.Add("IsVariadic", @function.IsVariadic);
                jMeta.Add("Framework", @function.Module.Name);
                array.Add(jMeta);
            }
            return array;
        }

        private JArray SerializeVars(IEnumerable<IDeclaration> declarations)
        {
            JArray array = new JArray();
            foreach (VarDeclaration @var in declarations)
            {
                JObject jMeta = new JObject();
                AddCommonProperties(jMeta, @var);
                jMeta.Add("ExtendedEncoding", Convert.ToString(@var.GetExtendedEncoding()));
                jMeta.Add("Framework", @var.Module.Name);
                array.Add(jMeta);
            }
            return array;
        }

        private JObject SerializeMethod(MethodDeclaration method)
        {
            JObject jMeta = new JObject();
            AddCommonProperties(jMeta, method);
            jMeta.Add("Selector", method.Selector);
            jMeta.Add("CompilerEncoding", method.TypeEncoding);
            jMeta.Add("ExtendedEncoding", Convert.ToString(method.GetExtendedEncoding()));
            jMeta.Add("IsVariadic", method.IsVariadic);
            return jMeta;
        }

        private JObject SerializeProperty(PropertyDeclaration property)
        {
            JObject jMeta = new JObject();
            AddCommonProperties(jMeta, property);
            if (property.Getter != null)
                jMeta.Add("Getter", property.Getter.GetJSName());
            if (property.Setter != null)
                jMeta.Add("Setter", property.Setter.GetJSName());
            return jMeta;
        }

        private JObject SerializeField(FieldDeclaration field)
        {
            JObject jMeta = new JObject();
            AddCommonProperties(jMeta, field);
            jMeta.Add("ExtendedEncoding", Convert.ToString(field.GetExtendedEncoding()));
            return jMeta;
        }

        private JObject SerializeEnumField(EnumMemberDeclaration field)
        {
            JObject jMeta = new JObject();
            AddCommonProperties(jMeta, field);
            jMeta.Add("Value", field.Value);
            return jMeta;
        }

        private JObject SerializeClass(BaseClass @class)
        {
            JObject jMeta = new JObject();
            AddCommonProperties(jMeta, @class);
            jMeta.Add("Module", @class.Module.Name);

            if (@class.ImplementedProtocols.Any())
                jMeta.Add("ImplementedProtocols", JToken.FromObject(@class.ImplementedProtocols.Select(c => c.GetJSName()).ToArray()));

            if (@class.Properties.Any())
                jMeta.Add("Properties",
                    JToken.FromObject(@class.Properties.OrderBy(c => c.GetJSName()).Select(c => SerializeProperty(c))));

            if (@class.InstanceMethods().Any())
                jMeta.Add("InstanceMethods",
                    JToken.FromObject(@class.InstanceMethods().OrderBy(c => c.GetJSName()).Select(c => SerializeMethod(c))));

            if (@class.StaticMethods().Any())
                jMeta.Add("StaticMethods",
                    JToken.FromObject(@class.StaticMethods().OrderBy(c => c.GetJSName()).Select(c => SerializeMethod(c))));
            return jMeta;
        }
    }
}
