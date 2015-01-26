using Libclang.Core.Meta.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Libclang.Core.Meta.Filters
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

        protected override Action<MetaContainer> ActionForContainer
        {
            get { return this.SerializeMeta; }
        }

        private void SerializeMeta(MetaContainer metaContainer)
        {
            var protocols = metaContainer.Where(c => c.Value is ProtocolMeta);
            var interfaces = metaContainer.Where(c => c.Value is InterfaceMeta);
            var structs = metaContainer.Where(c => c.Value is StructMeta);
            var unions = metaContainer.Where(c => c.Value is UnionMeta);
            var enums = metaContainer.Where(c => c.Value is EnumMeta);
            var functions = metaContainer.Where(c => c.Value is FunctionMeta);
            var vars = metaContainer.Where(c => c.Value is VarMeta);

            JObject meta = new JObject();
            meta.Add("protocols", SerializeProtocols(protocols.OrderBy(c => c.Key).ThenBy(c => c.Value.Framework)));
            meta.Add("interfaces", SerializeInterfaces(interfaces.OrderBy(c => c.Key).ThenBy(c => c.Value.Framework)));
            meta.Add("structs", SerializeRecords(structs.OrderBy(c => c.Key).ThenBy(c => c.Value.Framework)));
            meta.Add("unions", SerializeRecords(unions.OrderBy(c => c.Key).ThenBy(c => c.Value.Framework)));
            meta.Add("enums", SerializeEnums(enums.OrderBy(c => c.Key).ThenBy(c => c.Value.Framework)));
            meta.Add("functions", SerializeFunctions(functions.OrderBy(c => c.Key).ThenBy(c => c.Value.Framework)));
            meta.Add("vars", SerializeVars(vars.OrderBy(c => c.Key).ThenBy(c => c.Value.Framework)));

            using (StreamWriter file = File.CreateText(this.OutputFilePath))
            {
                using (JsonWriter writer = new JsonTextWriter(file) {Formatting = Formatting.Indented})
                {
                    meta.WriteTo(writer);
                }
            }
        }

        private void AddCommonProperties(JObject @object, Meta meta)
        {
            @object.Add("Name", meta.JSName);
            JObject availability = new JObject();
            if (meta.IntroducedIn != null)
            {
                availability.Add("IntroducedIn", meta.IntroducedIn.ToString());
            }
            if (meta.ObsoletedIn != null)
            {
                availability.Add("ObsoletedIn", meta.ObsoletedIn.ToString());
            }
            if (meta.DeprecatedIn != null)
            {
                availability.Add("DeprecatedIn", meta.DeprecatedIn.ToString());
            }
            if (availability.HasValues)
            {
                @object.Add("Availability", availability);
            }
        }

        private JArray SerializeProtocols(IEnumerable<KeyValuePair<string, Meta>> pairs)
        {
            JArray array = new JArray();
            foreach (var pair in pairs)
            {
                ProtocolMeta @protocol = pair.Value as ProtocolMeta;
                JObject jMeta = SerializeClass(@protocol);
                array.Add(jMeta);
            }
            return array;
        }

        private JArray SerializeInterfaces(IEnumerable<KeyValuePair<string, Meta>> pairs)
        {
            JArray array = new JArray();
            foreach (var pair in pairs)
            {
                InterfaceMeta @interface = pair.Value as InterfaceMeta;
                JObject jMeta = SerializeClass(@interface);
                if (!string.IsNullOrEmpty(@interface.BaseJsName))
                    jMeta.Add("Base", @interface.BaseJsName);
                if (@interface.Categories.Count() > 0)
                    jMeta.Add("Categories",
                        JToken.FromObject(@interface.Categories.OrderBy(c => c.JSName).Select(c => SerializeClass(c))));
                array.Add(jMeta);
            }
            return array;
        }

        private JArray SerializeRecords(IEnumerable<KeyValuePair<string, Meta>> pairs)
        {
            JArray array = new JArray();
            foreach (var pair in pairs)
            {
                RecordMeta @record = pair.Value as RecordMeta;
                JObject jMeta = new JObject();
                AddCommonProperties(jMeta, @record);
                jMeta.Add("ExtendedEncoding", @record.ExtendedEncoding);
                if (@record.Fields.Count > 0)
                    jMeta.Add("Fields", JToken.FromObject(@record.Fields.Select(c => SerializeField(c))));
                jMeta.Add("Framework", @record.Framework);
                array.Add(jMeta);
            }
            return array;
        }

        private JArray SerializeEnums(IEnumerable<KeyValuePair<string, Meta>> pairs)
        {
            JArray array = new JArray();
            foreach (var pair in pairs)
            {
                EnumMeta @enum = pair.Value as EnumMeta;
                JObject jMeta = new JObject();
                AddCommonProperties(jMeta, @enum);
                if (@enum.Fields.Count > 0)
                    jMeta.Add("Members", JToken.FromObject(@enum.Fields.Select(c => SerializeEnumField(c))));
                jMeta.Add("Framework", @enum.Framework);
                array.Add(jMeta);
            }
            return array;
        }

        private JArray SerializeFunctions(IEnumerable<KeyValuePair<string, Meta>> pairs)
        {
            JArray array = new JArray();
            foreach (var pair in pairs)
            {
                FunctionMeta @function = pair.Value as FunctionMeta;
                JObject jMeta = new JObject();
                AddCommonProperties(jMeta, @function);
                jMeta.Add("ExtendedEncoding", Convert.ToString(@function.ExtendedEncoding));
                jMeta.Add("IsVariadic", @function.IsVariadic);
                jMeta.Add("Framework", @function.Framework);
                array.Add(jMeta);
            }
            return array;
        }

        private JArray SerializeVars(IEnumerable<KeyValuePair<string, Meta>> pairs)
        {
            JArray array = new JArray();
            foreach (var pair in pairs)
            {
                VarMeta @var = pair.Value as VarMeta;
                JObject jMeta = new JObject();
                AddCommonProperties(jMeta, @var);
                jMeta.Add("ExtendedEncoding", Convert.ToString(@var.ExtendedEncoding));
                jMeta.Add("Framework", @var.Framework);
                array.Add(jMeta);
            }
            return array;
        }

        private JObject SerializeMethod(MethodMeta method)
        {
            JObject jMeta = new JObject();
            AddCommonProperties(jMeta, method);
            jMeta.Add("Selector", method.Selector);
            jMeta.Add("CompilerEncoding", method.CompilerEncoding);
            jMeta.Add("ExtendedEncoding", Convert.ToString(method.ExtendedEncoding));
            jMeta.Add("IsVariadic", method.IsVariadic);
            return jMeta;
        }

        private JObject SerializeProperty(PropertyMeta property)
        {
            JObject jMeta = new JObject();
            AddCommonProperties(jMeta, property);
            if (property.Getter != null)
                jMeta.Add("Getter", property.Getter.JSName);
            if (property.Setter != null)
                jMeta.Add("Setter", property.Setter.JSName);
            return jMeta;
        }

        private JObject SerializeField(RecordFieldMeta field)
        {
            JObject jMeta = new JObject();
            AddCommonProperties(jMeta, field);
            jMeta.Add("ExtendedEncoding", Convert.ToString(field.ExtendedEncoding));
            return jMeta;
        }

        private JObject SerializeEnumField(EnumFieldMeta field)
        {
            JObject jMeta = new JObject();
            AddCommonProperties(jMeta, field);
            jMeta.Add("Value", field.Value);
            return jMeta;
        }

        private JObject SerializeClass(BaseClassMeta @class)
        {
            JObject jMeta = new JObject();
            AddCommonProperties(jMeta, @class);
            jMeta.Add("Framework", @class.Framework);
            if (@class.ImplementedProtocolsJSNames.Any())
                jMeta.Add("ImplementedProtocols", JToken.FromObject(@class.ImplementedProtocolsJSNames));
            if (@class.Properties.Any())
                jMeta.Add("Properties",
                    JToken.FromObject(@class.Properties.OrderBy(c => c.JSName).Select(c => SerializeProperty(c))));
            if (@class.InstanceMethods.Any())
                jMeta.Add("InstanceMethods",
                    JToken.FromObject(@class.InstanceMethods.OrderBy(c => c.JSName).Select(c => SerializeMethod(c))));
            if (@class.StaticMethods.Any())
                jMeta.Add("StaticMethods",
                    JToken.FromObject(@class.StaticMethods.OrderBy(c => c.JSName).Select(c => SerializeMethod(c))));
            return jMeta;
        }
    }
}
