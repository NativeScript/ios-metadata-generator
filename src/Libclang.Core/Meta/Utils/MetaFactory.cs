using System;
using System.Linq;
using Libclang.Core.Ast;
using Libclang.Core.Generator;
using System.Collections.Generic;
using Libclang.Core.Common;
using System.Diagnostics;

namespace Libclang.Core.Meta.Utils
{
    internal class MetaFactory
    {
        private static readonly IDictionary<string, string> ReservedKeywordParameterNameResolution = new Dictionary<string, string>()
        {
            { "break", "breakArg" },
            { "case", "caseArg" },
            { "catch", "catchArg" },
            { "continue", "continueArg" },
            { "debugger", "debuggerArg" },
            { "default", "defaultArg" },
            { "delete", "deleteArg" },
            { "do", "doArg" },
            { "else", "elseArg" },
            { "finally", "finallyArg" },
            { "for", "forArg" },
            { "function", "funcArg" },
            { "if", "ifarg" },
            { "in", "input" },
            { "instanceof", "instanceOfArg" },
            { "new", "newArg" },
            { "return", "returnArg" },
            { "switch", "switchArg" },
            { "this", "thisArg" },
            { "throw", "throwArg" },
            { "try", "tryArg" },
            { "typeof", "typeofArg" },
            { "var", "arg" },
            { "void", "voidArg" },
            { "while", "whileArg" },
            { "with", "withArg" }
        };

        public static Meta CreateMeta(Meta meta, BaseDeclaration declaration, MetaContainer container)
        {
            meta.Container = container;
            meta.Name = declaration.Name;
            meta.JSName = container.CalculateJsName(declaration);
            meta.Framework = declaration.GetFrameworkName();
            meta.Location = declaration.Location;
            meta.IsIosAppExtensionAvailable = (declaration.IosAppExtensionAvailability != null) ? !declaration.IosAppExtensionAvailability.IsUnavailable : true;
            if (declaration.IosAppExtensionAvailability != null) {
                // In metadata we store only IosAppExtensionAvailability.IsUnavailable value because there is no symbol in iOS 8 SDK which has IosAppExtensionAvailability and has
                // valid version for Introduced, Deprecated and Obsoleted properties. If in future SDKs there is such symbol we have to save in metadata introduced and obsoleted values.
                Debug.Assert(declaration.IosAppExtensionAvailability.Introduced == null || declaration.IosAppExtensionAvailability.Introduced.Major == -1);
                Debug.Assert(declaration.IosAppExtensionAvailability.Deprecated == null || declaration.IosAppExtensionAvailability.Deprecated.Major == -1);
                Debug.Assert(declaration.IosAppExtensionAvailability.Obsoleted == null || declaration.IosAppExtensionAvailability.Obsoleted.Major == -1);
            }

            if (declaration.IosAvailability != null)
            {
                meta.IntroducedIn = declaration.IosAvailability.Introduced;
                meta.ObsoletedIn = declaration.IosAvailability.Obsoleted;
                meta.DeprecatedIn = declaration.IosAvailability.Deprecated;
            }
            return meta;
        }

        public static RecordMeta CreateRecord(RecordMeta meta, BaseRecordDeclaration declaration,
            MetaContainer container)
        {
            meta.TypedefName = declaration.TypedefName;
            meta.IsAnonymous = declaration.IsAnonymous;
            MetaFactory.CreateMeta(meta, declaration, container);

            meta.Fields = new List<RecordFieldMeta>(declaration.Fields.Count);
            foreach (FieldDeclaration field in declaration.Fields)
            {
                meta.Fields.Add(MetaFactory.CreateRecordField(new RecordFieldMeta(), field, container));
            }
            return meta;
        }

        public static StructMeta CreateStruct(StructMeta meta, StructDeclaration declaration, MetaContainer container)
        {
            MetaFactory.CreateRecord(meta, declaration, container);
            return meta;
        }

        public static UnionMeta CreateUnion(UnionMeta meta, UnionDeclaration declaration, MetaContainer container)
        {
            MetaFactory.CreateRecord(meta, declaration, container);
            return meta;
        }

        public static RecordFieldMeta CreateRecordField(RecordFieldMeta meta, FieldDeclaration declaration, MetaContainer container)
        {
            MetaFactory.CreateMeta(meta, declaration, container);
            meta.ExtendedEncoding = declaration.Type.ToTypeEncoding(container.CalculateJsName);
            return meta;
        }

        public static FunctionMeta CreateFunction(FunctionMeta meta, FunctionDeclaration declaration, MetaContainer container)
        {
            MetaFactory.CreateMeta(meta, declaration, container);
            meta.IsVariadic = declaration.IsVariadic;
            meta.HasVaListParameter = declaration.HasVaListParameter();
            meta.IsDefinedInHeaders = declaration.IsDefinition;
            meta.OwnsReturnedCocoaObject = declaration.OwnsReturnedCocoaObject.HasValue && declaration.OwnsReturnedCocoaObject.Value;
            meta.ReturnTypeEncoding = declaration.GetReturnTypeEncoding(container.CalculateJsName);
            meta.Parameters.AddRange(CreateParameters(declaration.Parameters, container));
            return meta;
        }

        private static IEnumerable<ParameterMeta> CreateParameters(IEnumerable<ParameterDeclaration> declarations, MetaContainer container)
        {
            var parameters = declarations.Select(d => CreateParameter(d, container)).ToList();
            ResolveNameCollisions(parameters);
            return parameters;
        }

        private static ParameterMeta CreateParameter(ParameterDeclaration declaration, MetaContainer container)
        {
            string name = declaration.Name;
            string jsName = null;
            if (!ReservedKeywordParameterNameResolution.TryGetValue(name, out jsName))
            {
                jsName = name;
            }

            if (string.IsNullOrWhiteSpace(jsName))
            {
                jsName = "arg";
            }

            var parameterMeta = new ParameterMeta()
            {
                Name = name,
                JSName = jsName,
                TypeEncoding = declaration.Type.ToTypeEncoding(container.CalculateJsName)
            };

            return parameterMeta;
        }

        private static void ResolveNameCollisions(List<ParameterMeta> parameters)
        {
            var names = new HashSet<string>();
            foreach (var p in parameters)
            {
                var suffix = 0;
                var suffixedName = p.JSName;
                while (names.Contains(suffixedName))
                {
                    suffix++;
                    suffixedName = p.JSName + suffix;
                }
                p.JSName = suffixedName;
                names.Add(suffixedName);
            }
        }

        public static VarMeta CreateVar(VarMeta meta, VarDeclaration declaration, MetaContainer container)
        {
            MetaFactory.CreateMeta(meta, declaration, container);
            meta.ExtendedEncoding = declaration.Type.ToTypeEncoding(container.CalculateJsName);
            meta.Value = null; // we support only run-time constants
            return meta;
        }

        public static EnumMeta CreateEnum(EnumMeta meta, EnumDeclaration declaration, MetaContainer container)
        {
            meta.TypedefName = declaration.TypedefName;
            meta.IsAnonymous = declaration.IsAnonymous;
            MetaFactory.CreateMeta(meta, declaration, container);
            meta.UnderlyingType = declaration.UnderlyingType;

            meta.Fields = new List<EnumFieldMeta>(declaration.Fields.Count);
            foreach (EnumMemberDeclaration field in declaration.Fields)
            {
                meta.Fields.Add(MetaFactory.CreateEnumField(new EnumFieldMeta(), field, container));
            }
            return meta;
        }

        public static EnumFieldMeta CreateEnumField(EnumFieldMeta meta, EnumMemberDeclaration declaration,
            MetaContainer container)
        {
            MetaFactory.CreateMeta(meta, declaration, container);
            meta.Value = declaration.Value;
            return meta;
        }

        public static BaseClassMeta CreateBaseClass(BaseClassMeta meta, BaseClass declaration, MetaContainer container)
        {
            MetaFactory.CreateMeta(meta, declaration, container);
            meta.Methods =
                new HashSet<MethodMeta>(
                    declaration.Methods.Where(m => !m.IsImplicit && !declaration.Properties.Any(p => p.Getter == m || p.Setter == m))
                        .Select(m => MetaFactory.CreateMethod(new MethodMeta(), m, container, declaration)));
            meta.Properties =
                new HashSet<PropertyMeta>(
                    declaration.Properties.Select(
                        p => MetaFactory.CreateProperty(new PropertyMeta(), p, container, declaration)));
            meta.ImplementedProtocolsJSNames = declaration.ImplementedProtocols.Select(p => container.CalculateJsName(p));
            return meta;
        }

        public static ProtocolMeta CreateProtocol(ProtocolMeta meta, ProtocolDeclaration declaration,
            MetaContainer container)
        {
            MetaFactory.CreateBaseClass(meta, declaration, container);
            return meta;
        }

        public static InterfaceMeta CreateInterface(InterfaceMeta meta, InterfaceDeclaration declaration,
            MetaContainer container)
        {
            MetaFactory.CreateBaseClass(meta, declaration, container);
            meta.Categories =
                new List<CategoryMeta>(
                    declaration.Categories.Select(c => MetaFactory.CreateCategory(new CategoryMeta(), c, container)));
            meta.BaseJsName = (declaration.Base == null) ? null : container.CalculateJsName(declaration.Base);
            return meta;
        }

        public static CategoryMeta CreateCategory(CategoryMeta meta, CategoryDeclaration declaration,
            MetaContainer container)
        {
            MetaFactory.CreateBaseClass(meta, declaration, container);
            return meta;
        }

        public static MemberMeta CreateMember(MemberMeta meta, BaseDeclaration declaration, MetaContainer container,
            BaseClass parent)
        {
            MetaFactory.CreateMeta(meta, declaration, container);
            BaseClass methodParentInMetadata = (parent is CategoryDeclaration) ? ((CategoryDeclaration)parent).ExtendedInterface : parent;
            meta.ParentJsName = container.CalculateJsName(methodParentInMetadata);
            meta.Framework = parent.GetFrameworkName();
            return meta;
        }

        public static PropertyMeta CreateProperty(PropertyMeta meta, PropertyDeclaration declaration,
            MetaContainer container, BaseClass parent)
        {
            MetaFactory.CreateMember((MemberMeta) meta, declaration, container, parent);
            meta.Getter = (declaration.Getter == null)
                ? null
                : MetaFactory.CreateMethod(new MethodMeta(), declaration.Getter, container, parent);
            meta.Setter = (declaration.Setter == null)
                ? null
                : MetaFactory.CreateMethod(new MethodMeta(), declaration.Setter, container, parent);
            meta.IsOptional = declaration.IsOptional;
            return meta;
        }

        public static MethodMeta CreateMethod(MethodMeta meta, MethodDeclaration declaration, MetaContainer container, BaseClass parent)
        {
            MetaFactory.CreateMember(meta, declaration, container, parent);
            meta.IsVariadic = declaration.IsVariadic;
            meta.IsNilTerminatedVariadic = declaration.IsNilTerminatedVariadic;
            meta.HasVaListParameter = declaration.HasVaListParameter();
            meta.IsStatic = declaration.IsStatic;
            meta.Selector = declaration.Name;
            meta.CompilerEncoding = declaration.TypeEncoding;
            meta.ReturnTypeEncoding = declaration.GetReturnTypeEncoding(container.CalculateJsName);
            meta.IsConstructor = declaration.IsConstructor;
            meta.IsOptional = declaration.IsOptional;
            meta.OwnsReturnedCocoaObject = declaration.OwnsReturnedCocoaObject.HasValue && declaration.OwnsReturnedCocoaObject.Value;
            meta.Parameters.AddRange(declaration.Parameters.Select(d => CreateParameter(d, container)));
            return meta;
        }
    }
}
