namespace TypeScript.Factory
{
    using System.Linq;
    using MetadataGenerator.Core.Ast;
    using MetadataGenerator.Core.Generator;
    using TypeScript.Declarations.Writers;
    using MT = MetadataGenerator.Core.Ast;
    using TS = TypeScript.Declarations.Model;

    public class SourceTrackingDocumentationProvider : DocumentationProvider
    {
        public override string GetDoc(TS.ClassDeclaration @class)
        {
            var doc = base.GetDoc(@class);
            doc = this.AddNativeType(doc, @class);
            doc = this.AddFramework(doc, @class);
            doc = this.AddAvailability(doc, @class);
            doc = this.AddLocation(doc, @class);
            return doc;
        }

        public override string GetDoc(TS.InterfaceDeclaration @interface)
        {
            var doc = base.GetDoc(@interface);
            doc = this.AddNativeType(doc, @interface);
            doc = this.AddFramework(doc, @interface);
            doc = this.AddAvailability(doc, @interface);
            doc = this.AddLocation(doc, @interface);
            return doc;
        }

        public override string GetDoc(TS.EnumDeclaration @enum)
        {
            var doc = base.GetDoc(@enum);
            doc = this.AddFramework(doc, @enum);
            doc = this.AddAvailability(doc, @enum);
            doc = this.AddLocation(doc, @enum);
            return doc;
        }

        public override string GetDoc(TS.FunctionDeclaration function)
        {
            var doc = base.GetDoc(function);
            doc = this.AddFramework(doc, function);
            doc = this.AddAvailability(doc, function);
            doc = this.AddLocation(doc, function);
            return doc;
        }

        public override string GetDoc(TS.VariableStatement @var)
        {
            var doc = base.GetDoc(@var);
            doc = this.AddFramework(doc, @var);
            doc = this.AddAvailability(doc, @var);
            doc = this.AddLocation(doc, @var);
            return doc;
        }

        public override string GetDoc(TS.MethodSignature method)
        {
            var doc = base.GetDoc(method);

            var methodMeta = method.Annotations.OfType<MT.MethodDeclaration>().FirstOrDefault();
            if (methodMeta != null)
            {
                doc = Append(doc, string.Format("@selector {0}", methodMeta.Selector));
            }

            doc = this.AddCategory(doc, method);
            doc = this.AddAvailability(doc, method);

            return doc;
        }

        public override string GetDoc(TS.PropertySignature property)
        {
            var doc = base.GetDoc(property);

            var propertyMeta = property.Annotations.OfType<MT.PropertyDeclaration>().FirstOrDefault();
            if (propertyMeta != null)
            {
                doc = Append(doc, string.Format("@property {0}", propertyMeta.Name));
            }

            doc = this.AddCategory(doc, property);
            doc = this.AddAvailability(doc, property);

            return doc;
        }

        public override string GetDoc(TS.ConstructSignature construct)
        {
            var doc = base.GetDoc(construct);

            doc = this.AddCategory(doc, construct);
            doc = this.AddAvailability(doc, construct);

            return doc;
        }

        private static string Append(string top, string bottom)
        {
            top = string.IsNullOrWhiteSpace(top) ? bottom : (top + "\n" + bottom);
            return top;
        }

        private string AddNativeType(string doc, TS.ClassDeclaration @class)
        {
            var interfaceMeta = @class.Annotations.OfType<MT.InterfaceDeclaration>().FirstOrDefault();
            if (interfaceMeta != null)
            {
                doc = Append(doc, string.Format("@interface {0}", interfaceMeta.Name));
            }

            return doc;
        }

        private string AddNativeType(string doc, TS.InterfaceDeclaration @interface)
        {
            var protocolMeta = @interface.Annotations.OfType<MT.ProtocolDeclaration>().FirstOrDefault();
            if (protocolMeta != null)
            {
                doc = Append(doc, string.Format("@protocol {0}", protocolMeta.Name));
            }

            var structMeta = @interface.Annotations.OfType<MT.StructDeclaration>().FirstOrDefault();
            if (structMeta != null)
            {
                if (!structMeta.IsAnonymous)
                {
                    doc = Append(doc, string.Format("@struct {0}", structMeta.Name));
                }
                else if (structMeta.IsAnonymousWithTypedef())
                {
                    doc = Append(doc, string.Format("@struct typedef {0}", structMeta.TypedefName));
                }
                else
                {
                    doc = Append(doc, "@struct <anonymous>");
                }
            }

            return doc;
        }

        private string AddCategory(string doc, TS.TypeScriptObject typeScriptObject)
        {
            var category = typeScriptObject.Annotations.OfType<MT.CategoryDeclaration>().FirstOrDefault();
            if (category != null)
            {
                doc = Append(doc, string.Format("@category {0}", category.Name));
            }

            return doc;
        }

        private string AddAvailability(string doc, TS.TypeScriptObject typeScriptObject)
        {
            var meta = typeScriptObject.Annotations.OfType<MT.BaseDeclaration>().FirstOrDefault();
            if (meta != null)
            {
                MetadataGenerator.Core.Common.PlatformAvailability availability = meta.IosAvailability;

                if (availability != null && MetadataGenerator.Core.Common.Version.IsSet(availability.Introduced))
                {
                    doc = Append(doc, string.Format("@introduced {0}", availability.Introduced));
                }

                if (availability != null && MetadataGenerator.Core.Common.Version.IsSet(availability.Obsoleted))
                {
                    doc = Append(doc, string.Format("@obsolete {0}", availability.Obsoleted));
                }

                if (availability != null && MetadataGenerator.Core.Common.Version.IsSet(availability.Deprecated))
                {
                    doc = Append(doc, string.Format("@deprecated {0}", availability.Deprecated));
                }
            }

            return doc;
        }

        private string AddFramework(string doc, TS.TypeScriptObject typeScriptObject)
        {
            var interfaceMeta = typeScriptObject.Annotations.OfType<MT.BaseDeclaration>().FirstOrDefault();
            if (interfaceMeta != null && !string.IsNullOrWhiteSpace(interfaceMeta.Module.FullName))
            {
                doc = Append(doc, string.Format("@framework {0}", interfaceMeta.Module.FullName));
            }

            return doc;
        }

        private string AddLocation(string doc, TS.TypeScriptObject typeScriptObject)
        {
            var meta = typeScriptObject.Annotations.OfType<MT.BaseDeclaration>().FirstOrDefault();
            if (meta != null && meta.Location != null)
            {
                var location = meta.Location;
                var source = string.Format("@source {0} ({1}, {2})", System.IO.Path.GetFileName(location.Filename), location.Line, location.Column);
                doc = Append(doc, source);
            }

            return doc;
        }
    }
}
