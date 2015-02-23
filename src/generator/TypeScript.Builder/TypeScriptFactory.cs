namespace TypeScript.Factory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using MetadataGenerator.Core;
    using MetadataGenerator.Core.Types;
    using MetadataGenerator.Core.Meta.Utils;
    using TypeScript.Declarations;
    using MT = MetadataGenerator.Core.Ast;
    using TS = TypeScript.Declarations.Model;

    public partial class TypeScriptFactory
    {
        private static Regex pointerToObjCRegex = new Regex("^@\"(?<name>.*)\"$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private static Regex arrayRegex = new Regex("^\\[(?<type>.*)\\]$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private static Regex structRegex = new Regex("^\\{(?<name>.*)\\}$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        private Dictionary<MT.InterfaceDeclaration, TS.ClassDeclaration> classMap;
        private Dictionary<MT.ProtocolDeclaration, TS.InterfaceDeclaration> interfaceMap;
        private Dictionary<MT.StructDeclaration, TS.InterfaceDeclaration> structMap;
        private Dictionary<MT.EnumDeclaration, TS.EnumDeclaration> enumMap;
        private Dictionary<MT.VarDeclaration, TS.VariableStatement> varMap;
        private Dictionary<MT.FunctionDeclaration, TS.FunctionDeclaration> functionMap;

        /// <summary>
        /// A TypeScript ClassDeclaration lookup by InterfaceDeclaration name.
        /// </summary>
        private Dictionary<string, TS.ClassDeclaration> interfaceLookup;

        /// <summary>
        /// A TypeScript InterfaceDeclaration lookup by ProtocolMeta name.
        /// </summary>
        private Dictionary<string, TS.InterfaceDeclaration> protocolLookup;

        /// <summary>
        /// A TypeScript InterfaceDeclaration lookup by StructMeta name.
        /// </summary>
        private Dictionary<string, TS.InterfaceDeclaration> structLookup;

        private HashSet<string> unhandledEncodings = new HashSet<string>();

        private TypeEncodingTransfomation<TS.IType> typeTransformation;

        public ModuleDeclarationsContainer MetaContainer { get; set; }

        public TS.SourceUnit Global { get; private set; }

        public void Create()
        {
            if (this.MetaContainer == null)
            {
                throw new InvalidOperationException("Can not build definitions without meta data.");
            }

            this.Global = new TS.SourceUnit();
            this.typeTransformation = new MetaToTypeScriptTypeTransform(this);

            this.classMap = new Dictionary<MT.InterfaceDeclaration, TS.ClassDeclaration>();
            this.interfaceMap = new Dictionary<MT.ProtocolDeclaration, TS.InterfaceDeclaration>();
            this.structMap = new Dictionary<MT.StructDeclaration, TS.InterfaceDeclaration>();
            this.enumMap = new Dictionary<MT.EnumDeclaration, TS.EnumDeclaration>();
            this.varMap = new Dictionary<MT.VarDeclaration, TS.VariableStatement>();
            this.functionMap = new Dictionary<MT.FunctionDeclaration, TS.FunctionDeclaration>();

            this.interfaceLookup = new Dictionary<string, TS.ClassDeclaration>();
            this.protocolLookup = new Dictionary<string, TS.InterfaceDeclaration>();
            this.structLookup = new Dictionary<string, TS.InterfaceDeclaration>();

            this.MetaContainer.OfType<MT.InterfaceDeclaration>().Apply(this.CreateClassForInterface);
            this.MetaContainer.OfType<MT.ProtocolDeclaration>().Apply(this.CreateInterfaceForProtocol);
            this.MetaContainer.OfType<MT.ProtocolDeclaration>().Apply(this.CreateTypeVarForProtocol);
            this.MetaContainer.OfType<MT.StructDeclaration>().Apply(this.CreateInterfaceForStruct);
            this.MetaContainer.OfType<MT.EnumDeclaration>().Apply(this.CreateEnum);
            this.MetaContainer.OfType<MT.VarDeclaration>().Apply(this.CreateVar);
            this.MetaContainer.OfType<MT.FunctionDeclaration>().Apply(this.CreateFunction);

            var nsportmessage = this.MetaContainer.Cast<MT.BaseDeclaration>().Where(meta => meta.GetJSName() == "NSManagedObjectID");

            this.interfaceMap.Apply(this.CreateMembers);
            this.classMap.Apply(this.CreateMembers);
            this.enumMap.Apply(this.CreateMembers);
            this.structMap.Apply(this.CreateMembers);
            this.varMap.Apply(this.CreateMembers);
            this.functionMap.Apply(this.CreateMembers);

            this.classMap.Apply(this.PropagateInstancetypeMethods);
            this.CreateExtendMethod(this.classMap.Values);

            this.classMap.Apply(this.CreateConstructors);

            this.classMap.Values.Apply(this.AutoImplementInterfaces);

            this.classMap.Values.Apply(this.FilterDuplicateMethods);
            this.classMap.Values.Apply(this.FilterDuplicateProperties);
        }

        private void PropagateInstancetypeMethods(MT.InterfaceDeclaration interfaceMeta, TS.ClassDeclaration @class)
        {
            // Set the return type of instance methods
            @class.Methods.Where(Flags.ReturnsInstancetype).Apply(m => m.ReturnType = @class);

            // Pull instancemethods from base classes and update the return type
            this.PullBaseInstancetypeMethods(@class, @class.Extends);
        }

        private void CreateExtendMethod(IEnumerable<TS.ClassDeclaration> classes)
        {
            var extend = new TS.MethodSignature()
            {
                Name = "extend",
                IsStatic = true,
                ReturnType = TS.PrimitiveTypes.Any,
            };

            var methodsParameter = new TS.Parameter()
            {
                Name = "methods",
                TypeAnnotation = TS.PrimitiveTypes.Any
            };
            extend.Parameters.Add(methodsParameter);

            var exposedMethodsParameter = new TS.Parameter()
            {
                Name = "exposedMethods",
                IsOptional = true
            };
            var exposedMethodsParameterType = new TS.ObjectType();
            exposedMethodsParameterType.Properties.Add(new TS.PropertySignature()
            {
                Name = "name",
                IsOptional = true,
                TypeAnnotation = TS.PrimitiveTypes.String
            });

            var protocols = new TS.PropertySignature()
            {
                Name = "protocols",
                IsOptional = true
            };

            var protocolsType = new TS.ArrayType() { ComponentType = TS.PrimitiveTypes.Any };
            protocols.TypeAnnotation = protocolsType;
            exposedMethodsParameterType.Properties.Add(protocols);

            var exposedObjectProperty = new TS.PropertySignature()
            {
                Name = "exposedMethods",
                IsOptional = true
            };
            var exposedObjectPropertyType = new TS.ObjectType();
            exposedObjectPropertyType.Indexers.Add(new TS.IndexerSignature()
            {
                KeyName = "name",
                KeyType = TS.PrimitiveTypes.String,
                ComponentType = TS.PrimitiveTypes.String
            });
            exposedObjectProperty.TypeAnnotation = exposedObjectPropertyType;
            exposedMethodsParameterType.Properties.Add(exposedObjectProperty);

            exposedMethodsParameter.TypeAnnotation = exposedMethodsParameterType;
            extend.Parameters.Add(exposedMethodsParameter);

            classes.Apply(c => c.Methods.Add(extend));
        }

        private void CreateConstructors(MT.InterfaceDeclaration interfaceMeta, TS.ClassDeclaration @class)
        {
            var constructorsMeta = new Dictionary<string, MT.MethodDeclaration>();

            // We can use as constructors all init method from this class and its categories and base classes and all their categories...
            var current = interfaceMeta;
            while (current != null)
            {
                // class methods
                foreach (var method in current.Methods.Where(m => m.IsConstructor))
                {
                    if (!constructorsMeta.ContainsKey(method.Name))
                    {
                        constructorsMeta.Add(method.Name, method);
                    }
                }

                // category methods
                foreach (var method in current.Categories.SelectMany(c => c.Methods).Where(m => m.IsConstructor))
                {
                    if (!constructorsMeta.ContainsKey(method.Name))
                    {
                        constructorsMeta.Add(method.Name, method);
                    }
                }

                current = current.Base;
            }

            // constructors
            var allConstructs = new List<TS.ConstructSignature>();
            foreach (var methodMeta in constructorsMeta.Values)
            {
                var constructor = new TS.ConstructSignature();
                this.BuildParameters(methodMeta.Parameters, constructor.Parameters);
                this.TrackMeta(methodMeta, constructor);
                allConstructs.Add(constructor);
            }

            var uniqueConstructs = allConstructs
                .Where(construct => !allConstructs.Any(other => !object.ReferenceEquals(construct, other) && object.Equals(construct, other)))
                .ToList();

            @class.Constructors.AddRange(uniqueConstructs);
        }

        private void PullBaseInstancetypeMethods(TS.ClassDeclaration @class, TS.IType type)
        {
            if (type == null)
            {
                return;
            }

            var baseClass = type as TS.ClassDeclaration;
            if (baseClass != null)
            {
                foreach (var baseInstancetypeMethod in baseClass.Methods
                    .Where(Flags.ReturnsInstancetype)
                    .Where(inherited => !@class.Methods.Any(own => own.Name == inherited.Name)))
                {
                    var promotedInstancetypeMethod = baseInstancetypeMethod.Clone();
                    promotedInstancetypeMethod.ReturnType = @class;
                    @class.Methods.Add(promotedInstancetypeMethod);
                }

                this.PullBaseInstancetypeMethods(@class, baseClass.Extends);
            }

            //// NOTE: Else we hit classes with generic parameters,
            //// the IType here may be GenericType with underlying class and actual type arguments...
        }

        private void CreateInterfaceForProtocol(MT.ProtocolDeclaration protocolMeta)
        {
            var @interface = new TS.InterfaceDeclaration();
            @interface.Name = protocolMeta.GetJSName();
            this.interfaceMap.Add(protocolMeta, @interface);
            this.protocolLookup.Add(@interface.Name, @interface);
            this.TrackMeta(protocolMeta, @interface);
            this.AddDeclaration(@interface);
        }

        private void CreateTypeVarForProtocol(MT.ProtocolDeclaration protocolMeta)
        {
            var @var = new TS.VariableStatement();
            @var.Name = protocolMeta.GetJSName();
            this.TrackMeta(protocolMeta, @var);

            this.AddDeclaration(@var);
        }

        private void CreateClassForInterface(MT.InterfaceDeclaration interfaceMeta)
        {
            var @class = new TS.ClassDeclaration();
            @class.Name = interfaceMeta.GetJSName();
            this.classMap.Add(interfaceMeta, @class);
            this.interfaceLookup.Add(@class.Name, @class);
            this.TrackMeta(interfaceMeta, @class);
            this.AddDeclaration(@class);
        }

        private void CreateInterfaceForStruct(MT.StructDeclaration structMeta)
        {
            var @struct = new TS.InterfaceDeclaration();
            @struct.Name = structMeta.GetJSName();
            this.structMap.Add(structMeta, @struct);
            this.structLookup.Add(@struct.Name, @struct);
            this.TrackMeta(structMeta, @struct);
            this.AddDeclaration(@struct);
        }

        private void CreateEnum(MT.EnumDeclaration enumMeta)
        {
            var @enum = new TS.EnumDeclaration();
            @enum.Name = enumMeta.GetJSName();
            this.enumMap.Add(enumMeta, @enum);
            this.TrackMeta(enumMeta, @enum);
            this.AddDeclaration(@enum);
        }

        private void CreateVar(MT.VarDeclaration varMeta)
        {
            var @var = new TS.VariableStatement();
            @var.Name = varMeta.GetJSName();
            this.varMap.Add(varMeta, @var);
            this.TrackMeta(varMeta, @var);
            this.AddDeclaration(@var);
        }

        private void CreateFunction(MT.FunctionDeclaration functionMeta)
        {
            var @function = new TS.FunctionDeclaration();
            @function.Name = functionMeta.GetJSName();
            this.functionMap.Add(functionMeta, @function);
            this.TrackMeta(functionMeta, @function);
            this.AddDeclaration(@function);
        }

        private void CreateMembers(MT.ProtocolDeclaration protocolMeta, TS.InterfaceDeclaration @interface)
        {
            // Add implemented protocols.
            foreach (var extendedProtocolMeta in protocolMeta.ImplementedProtocols)
            {
                var extendedInterface = this.GetInterfaceType(extendedProtocolMeta);
                @interface.Extends.Add(extendedInterface);
            }

            this.BuildMemberProperties(protocolMeta.Properties, @interface.Properties);
            this.BuildMemberMethods(protocolMeta.Methods, @interface.Methods);
        }

        private void CreateMembers(MT.InterfaceDeclaration interfaceMeta, TS.ClassDeclaration @class)
        {
            // Add extended class.
            var baseInterfaceMeta = interfaceMeta.Base;
            @class.Extends = baseInterfaceMeta == null ? null : this.GetClassType(baseInterfaceMeta);

            // Add implemented protocols.
            @class.Implements.AddRange(interfaceMeta.ImplementedProtocols.Select(this.GetInterfaceType));

            // Add properties
            this.BuildMemberProperties(interfaceMeta.Properties, @class.Properties);

            // Add methods.
            this.BuildMemberMethods(interfaceMeta.Methods, @class.Methods);

            foreach (var category in interfaceMeta.Categories)
            {
                this.BuildMembersFromCategory(category, @class);
            }
        }

        private void BuildMembersFromCategory(MT.CategoryDeclaration category, TS.ClassDeclaration @class)
        {
            // Add protocols implemented in the category
            @class.Implements.AddRange(category.ImplementedProtocols.Select(this.GetInterfaceType));

            // category.Properties
            this.BuildMemberPropertiesFromCategory(category.Properties, @class.Properties, category);

            // category.InstanceMethods
            this.BuildMemberMethodsFromCategory(category.Methods, @class.Methods, category);
        }

        private void CreateMembers(MT.EnumDeclaration enumMeta, TS.EnumDeclaration @enum)
        {
            foreach (var field in enumMeta.Fields)
            {
                var enumElement = new TS.EnumElement();
                enumElement.Name = field.Name;

                // NOTE: In case of long long with really big value we will provide an object with custom implementation for value
                // so it is important to skip the value here, otherwise TypeScript compiler would inline the enum value. e.g. "Enum.X" compile to "4 /* Enum.X */"
                // enumElement.Value = field.Value;
                @enum.Members.Add(enumElement);
            }
        }

        private void BuildMemberProperties(IEnumerable<MT.PropertyDeclaration> from, IList<TS.PropertySignature> to)
        {
            foreach (var propertyMeta in from)
            {
                var property = this.CreateProperty(propertyMeta);
                to.Add(property);
            }
        }

        private void BuildMemberPropertiesFromCategory(IEnumerable<MT.PropertyDeclaration> from, IList<TS.PropertySignature> to, MT.CategoryDeclaration category)
        {
            foreach (var propertyMeta in from)
            {
                var property = this.CreateProperty(propertyMeta);
                property.Annotations.Add(category);
                to.Add(property);
            }
        }

        private TS.PropertySignature CreateProperty(MT.PropertyDeclaration propertyMeta)
        {
            var property = new TS.PropertySignature();
            property.Name = propertyMeta.GetJSName();
            property.TypeAnnotation = this.GetTypeFromEncoding(propertyMeta.GetExtendedEncoding());
            property.IsOptional = propertyMeta.IsOptional;
            this.TrackMeta(propertyMeta, property);
            return property;
        }

        private void BuildMemberMethods(IEnumerable<MT.MethodDeclaration> from, IList<TS.MethodSignature> to)
        {
            foreach (var methodMeta in from)
            {
                var method = this.CreateMethod(methodMeta);
                to.Add(method);
            }
        }

        private void BuildMemberMethodsFromCategory(IEnumerable<MT.MethodDeclaration> from, IList<TS.MethodSignature> to, MT.CategoryDeclaration category)
        {
            foreach (var methodMeta in from)
            {
                var method = this.CreateMethod(methodMeta);
                method.Annotations.Add(category);
                to.Add(method);
            }
        }

        private TS.MethodSignature CreateMethod(MT.MethodDeclaration methodMeta)
        {
            var method = new TS.MethodSignature();
            method.Name = methodMeta.GetJSName();
            method.IsStatic = methodMeta.IsStatic;
            method.IsOptional = methodMeta.IsOptional;

            this.BuildParameters(methodMeta.Parameters, method.Parameters);

            if (methodMeta.ReturnType is InstanceType)
            {
                Flags.SetReturnsInstancetype(method);
            }

            method.ReturnType = this.GetTypeFromEncoding(methodMeta.ReturnType.ToTypeEncoding());

            this.TrackMeta(methodMeta, method);
            return method;
        }

        private void BuildParameters(IEnumerable<MT.ParameterDeclaration> metaParameters, ICollection<TS.Parameter> typeScriptParameters)
        {
            typeScriptParameters.AddRange(metaParameters.Select(this.BuildParameter));
        }

        private TS.Parameter BuildParameter(MT.ParameterDeclaration metaParameter)
        {
            var typeScriptParameter = new TS.Parameter();
            typeScriptParameter.Name = metaParameter.GetJSName();
            typeScriptParameter.TypeAnnotation = this.GetTypeFromEncoding(metaParameter.Type.ToTypeEncoding());
            return typeScriptParameter;
        }

        private void CreateMembers(MT.StructDeclaration structMeta, TS.InterfaceDeclaration @interface)
        {
            // throw new NotImplementedException();
            foreach (var field in structMeta.Fields)
            {
                // field.JSName
                var property = new TS.PropertySignature();
                property.Name = field.GetJSName();
                property.TypeAnnotation = this.GetTypeFromEncoding(field.GetExtendedEncoding());
                @interface.Properties.Add(property);
            }
        }

        private void CreateMembers(MT.VarDeclaration varMeta, TS.VariableStatement @var)
        {
            @var.TypeAnnotation = this.GetTypeFromEncoding(varMeta.GetExtendedEncoding());
        }

        private void CreateMembers(MT.FunctionDeclaration functionMeta, TS.FunctionDeclaration @function)
        {
            // Add arguments. (parameters)
            this.BuildParameters(functionMeta.Parameters, @function.Parameters);

            // Add return type.
            @function.ReturnType = this.GetTypeFromEncoding(functionMeta.ReturnType.ToTypeEncoding());
        }

        private void TrackMeta(MT.BaseDeclaration meta, TS.TypeScriptObject typeScriptObject)
        {
            typeScriptObject.Annotations.Add(meta);
        }

        private TS.IType GetClassType(MT.InterfaceDeclaration interfaceMeta)
        {
            return this.classMap[interfaceMeta];
        }

        private TS.IType GetInterfaceType(MT.ProtocolDeclaration protocolMeta)
        {
            return this.GetTypeForProtocolMetaName(protocolMeta.GetJSName());
        }

        private TS.IType GetStructType(MT.StructDeclaration structMeta)
        {
            return this.GetTypeForStructMetaName(structMeta.GetJSName());
        }

        private TS.IType GetTypeForProtocolMetaName(string name)
        {
            TS.InterfaceDeclaration type;
            if (!this.protocolLookup.TryGetValue(name, out type))
            {
                // NOTE: We have filtered some frameworks but types from them appear in methods.
                return TS.PrimitiveTypes.Any;
            }

            return type;
        }

        private TS.IType GetTypeForInterfaceMetaName(string name)
        {
            TS.ClassDeclaration type;
            if (!this.interfaceLookup.TryGetValue(name, out type))
            {
                // NOTE: We have filtered some frameworks but types from them appear in methods.
                return TS.PrimitiveTypes.Any;
            }

            return type;
        }

        private TS.IType GetTypeForStructMetaName(string name)
        {
            TS.InterfaceDeclaration type;
            if (!this.structLookup.TryGetValue(name, out type))
            {
                Console.WriteLine("Not fonud: " + name);
                return TS.PrimitiveTypes.Any;
            }

            return type;
        }

        private TS.IType GetTypeForUnionMetaName(string name)
        {
            return TS.PrimitiveTypes.Any;
        }

        private void AddDeclaration(TS.Declaration node)
        {
            // If we support modules we may need to push modules on stack or something...
            this.Global.Children.Add(node);
        }

        private void AutoImplementInterfaces(TS.ClassDeclaration @class)
        {
            this.AutoImplementInterfaces(@class, @class.Implements.OfType<TS.InterfaceDeclaration>());
        }

        private void AutoImplementInterfaces(TS.ClassDeclaration @class, IEnumerable<TS.InterfaceDeclaration> interfaces)
        {
            interfaces.Apply(i => this.AutoImplementInterface(@class, i));
        }

        private void AutoImplementInterface(TS.ClassDeclaration @class, TS.InterfaceDeclaration @interface)
        {
            @class.Properties.AddRange(
                @interface.Properties
                    .Where(p => !p.IsOptional)
                    .Where(p => !@class.HasInstanceMember(p.Name))
                    .Select(ModelExtensions.Clone));

            @class.Methods.AddRange(
                @interface.Methods
                    .Where(m => !m.IsOptional)
                    .Where(m => !@class.HasInstanceMember(m.Name))
                    .Select(ModelExtensions.Clone));

            // Recursively autoimplement extended interfaces.
            this.AutoImplementInterfaces(@class, @interface.Extends.OfType<TS.InterfaceDeclaration>());
        }

        private void FilterDuplicateMethods(TS.ClassDeclaration @class)
        {
            HashSet<TS.MethodSignature> methods = new HashSet<TS.MethodSignature>();
            IList<TS.MethodSignature> toRemove = new List<TS.MethodSignature>();
            foreach (var method in @class.Methods)
            {
                if (methods.Contains(method))
                {
                    toRemove.Add(method);
                }
                else
                {
                    methods.Add(method);
                }
            }

            @class.Methods.RemoveRange(toRemove);
        }

        private void FilterDuplicateProperties(TS.ClassDeclaration @class)
        {
            HashSet<string> properties = new HashSet<string>();
            IList<TS.PropertySignature> toRemove = new List<TS.PropertySignature>();
            foreach (var property in @class.Properties.Where(p => !p.IsStatic))
            {
                if (properties.Contains(property.Name))
                {
                    toRemove.Add(property);
                }
                else
                {
                    properties.Add(property.Name);
                }
            }

            @class.Properties.RemoveRange(toRemove);
        }

        private TS.IType GetTypeFromEncoding(TypeEncoding encoding)
        {
            return encoding.Transform(this.typeTransformation);
        }
    }
}
