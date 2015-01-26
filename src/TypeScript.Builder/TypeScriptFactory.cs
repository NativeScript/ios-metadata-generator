namespace TypeScript.Factory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Libclang.Core;
    using Libclang.Core.Meta.Utils;
    using TypeScript.Declarations;
    using MT = Libclang.Core.Meta;
    using TS = TypeScript.Declarations.Model;

    public partial class TypeScriptFactory
    {
        private static Regex pointerToObjCRegex = new Regex("^@\"(?<name>.*)\"$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private static Regex arrayRegex = new Regex("^\\[(?<type>.*)\\]$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private static Regex structRegex = new Regex("^\\{(?<name>.*)\\}$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        private Dictionary<MT.InterfaceMeta, TS.ClassDeclaration> classMap;
        private Dictionary<MT.ProtocolMeta, TS.InterfaceDeclaration> interfaceMap;
        private Dictionary<MT.StructMeta, TS.InterfaceDeclaration> structMap;
        private Dictionary<MT.EnumMeta, TS.EnumDeclaration> enumMap;
        private Dictionary<MT.VarMeta, TS.VariableStatement> varMap;
        private Dictionary<MT.FunctionMeta, TS.FunctionDeclaration> functionMap;

        /// <summary>
        /// A TypeScript ClassDeclaration lookup by InterfaceMeta name.
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

        public MetaContainer MetaContainer { get; set; }

        public TS.SourceUnit Global { get; private set; }

        public void Create()
        {
            if (this.MetaContainer == null)
            {
                throw new InvalidOperationException("Can not build definitions without meta data.");
            }

            this.Global = new TS.SourceUnit();
            this.typeTransformation = new MetaToTypeScriptTypeTransform(this);

            this.classMap = new Dictionary<MT.InterfaceMeta, TS.ClassDeclaration>();
            this.interfaceMap = new Dictionary<MT.ProtocolMeta, TS.InterfaceDeclaration>();
            this.structMap = new Dictionary<MT.StructMeta, TS.InterfaceDeclaration>();
            this.enumMap = new Dictionary<MT.EnumMeta, TS.EnumDeclaration>();
            this.varMap = new Dictionary<MT.VarMeta, TS.VariableStatement>();
            this.functionMap = new Dictionary<MT.FunctionMeta, TS.FunctionDeclaration>();

            this.interfaceLookup = new Dictionary<string, TS.ClassDeclaration>();
            this.protocolLookup = new Dictionary<string, TS.InterfaceDeclaration>();
            this.structLookup = new Dictionary<string, TS.InterfaceDeclaration>();

            var metaItems = this.MetaContainer.Select(kvp => kvp.Value);

            metaItems.OfType<MT.InterfaceMeta>().Apply(this.CreateClassForInterface);
            metaItems.OfType<MT.ProtocolMeta>().Apply(this.CreateInterfaceForProtocol);
            metaItems.OfType<MT.ProtocolMeta>().Apply(this.CreateTypeVarForProtocol);
            metaItems.OfType<MT.StructMeta>().Apply(this.CreateInterfaceForStruct);
            metaItems.OfType<MT.EnumMeta>().Apply(this.CreateEnum);
            metaItems.OfType<MT.VarMeta>().Apply(this.CreateVar);
            metaItems.OfType<MT.FunctionMeta>().Apply(this.CreateFunction);

            var nsportmessage = this.MetaContainer.Where(meta => meta.Value.JSName == "NSManagedObjectID");

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

        private void PropagateInstancetypeMethods(MT.InterfaceMeta interfaceMeta, TS.ClassDeclaration @class)
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

        private void CreateConstructors(MT.InterfaceMeta interfaceMeta, TS.ClassDeclaration @class)
        {
            var constructorsMeta = new Dictionary<string, MT.MethodMeta>();

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

        private void CreateInterfaceForProtocol(MT.ProtocolMeta protocolMeta)
        {
            var @interface = new TS.InterfaceDeclaration();
            @interface.Name = protocolMeta.JSName;
            this.interfaceMap.Add(protocolMeta, @interface);
            this.protocolLookup.Add(protocolMeta.JSName, @interface);
            this.TrackMeta(protocolMeta, @interface);
            this.AddDeclaration(@interface);
        }

        private void CreateTypeVarForProtocol(MT.ProtocolMeta protocolMeta)
        {
            var @var = new TS.VariableStatement();
            @var.Name = protocolMeta.JSName;
            this.TrackMeta(protocolMeta, @var);

            this.AddDeclaration(@var);
        }

        private void CreateClassForInterface(MT.InterfaceMeta interfaceMeta)
        {
            var @class = new TS.ClassDeclaration();
            @class.Name = interfaceMeta.JSName;
            this.classMap.Add(interfaceMeta, @class);
            this.interfaceLookup.Add(interfaceMeta.JSName, @class);
            this.TrackMeta(interfaceMeta, @class);
            this.AddDeclaration(@class);
        }

        private void CreateInterfaceForStruct(MT.StructMeta structMeta)
        {
            var @struct = new TS.InterfaceDeclaration();
            @struct.Name = structMeta.JSName;
            this.structMap.Add(structMeta, @struct);
            this.structLookup.Add(structMeta.JSName, @struct);
            this.TrackMeta(structMeta, @struct);
            this.AddDeclaration(@struct);
        }

        private void CreateEnum(MT.EnumMeta enumMeta)
        {
            var @enum = new TS.EnumDeclaration();
            @enum.Name = enumMeta.JSName;
            this.enumMap.Add(enumMeta, @enum);
            this.TrackMeta(enumMeta, @enum);
            this.AddDeclaration(@enum);
        }

        private void CreateVar(MT.VarMeta varMeta)
        {
            var @var = new TS.VariableStatement();
            @var.Name = varMeta.JSName;
            this.varMap.Add(varMeta, @var);
            this.TrackMeta(varMeta, @var);
            this.AddDeclaration(@var);
        }

        private void CreateFunction(MT.FunctionMeta functionMeta)
        {
            var @function = new TS.FunctionDeclaration();
            @function.Name = functionMeta.JSName;
            this.functionMap.Add(functionMeta, @function);
            this.TrackMeta(functionMeta, @function);
            this.AddDeclaration(@function);
        }

        private void CreateMembers(MT.ProtocolMeta protocolMeta, TS.InterfaceDeclaration @interface)
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

        private void CreateMembers(MT.InterfaceMeta interfaceMeta, TS.ClassDeclaration @class)
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

        private void BuildMembersFromCategory(MT.CategoryMeta category, TS.ClassDeclaration @class)
        {
            // Add protocols implemented in the category
            @class.Implements.AddRange(category.ImplementedProtocols.Select(this.GetInterfaceType));

            // category.Properties
            this.BuildMemberPropertiesFromCategory(category.Properties, @class.Properties, category);

            // category.InstanceMethods
            this.BuildMemberMethodsFromCategory(category.Methods, @class.Methods, category);
        }

        private void CreateMembers(MT.EnumMeta enumMeta, TS.EnumDeclaration @enum)
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

        private void BuildMemberProperties(HashSet<MT.PropertyMeta> from, IList<TS.PropertySignature> to)
        {
            foreach (var propertyMeta in from)
            {
                var property = this.CreateProperty(propertyMeta);
                to.Add(property);
            }
        }

        private void BuildMemberPropertiesFromCategory(HashSet<MT.PropertyMeta> from, IList<TS.PropertySignature> to, MT.CategoryMeta category)
        {
            foreach (var propertyMeta in from)
            {
                var property = this.CreateProperty(propertyMeta);
                property.Annotations.Add(category);
                to.Add(property);
            }
        }

        private TS.PropertySignature CreateProperty(MT.PropertyMeta propertyMeta)
        {
            var property = new TS.PropertySignature();
            property.Name = propertyMeta.JSName;
            property.TypeAnnotation = this.GetTypeFromEncoding(propertyMeta.ExtendedEncoding);
            property.IsOptional = propertyMeta.IsOptional;
            this.TrackMeta(propertyMeta, property);
            return property;
        }

        private void BuildMemberMethods(HashSet<MT.MethodMeta> from, IList<TS.MethodSignature> to)
        {
            foreach (var methodMeta in from)
            {
                var method = this.CreateMethod(methodMeta);
                to.Add(method);
            }
        }

        private void BuildMemberMethodsFromCategory(HashSet<MT.MethodMeta> from, IList<TS.MethodSignature> to, MT.CategoryMeta category)
        {
            foreach (var methodMeta in from)
            {
                var method = this.CreateMethod(methodMeta);
                method.Annotations.Add(category);
                to.Add(method);
            }
        }

        private TS.MethodSignature CreateMethod(MT.MethodMeta methodMeta)
        {
            var method = new TS.MethodSignature();
            method.Name = methodMeta.JSName;
            method.IsStatic = methodMeta.IsStatic;
            method.IsOptional = methodMeta.IsOptional;

            this.BuildParameters(methodMeta.Parameters, method.Parameters);

            if (methodMeta.ReturnTypeEncoding.IsInstancetype())
            {
                Flags.SetReturnsInstancetype(method);
            }

            method.ReturnType = this.GetTypeFromEncoding(methodMeta.ReturnTypeEncoding);

            this.TrackMeta(methodMeta, method);
            return method;
        }

        private void BuildParameters(IEnumerable<MT.ParameterMeta> metaParameters, ICollection<TS.Parameter> typeScriptParameters)
        {
            typeScriptParameters.AddRange(metaParameters.Select(this.BuildParameter));
        }

        private TS.Parameter BuildParameter(MT.ParameterMeta metaParameter)
        {
            var typeScriptParameter = new TS.Parameter();
            typeScriptParameter.Name = metaParameter.JSName;
            typeScriptParameter.TypeAnnotation = this.GetTypeFromEncoding(metaParameter.TypeEncoding);
            return typeScriptParameter;
        }

        private void CreateMembers(MT.StructMeta structMeta, TS.InterfaceDeclaration @interface)
        {
            // throw new NotImplementedException();
            foreach (var field in structMeta.Fields)
            {
                // field.JSName
                var property = new TS.PropertySignature();
                property.Name = field.JSName;
                property.TypeAnnotation = this.GetTypeFromEncoding(field.ExtendedEncoding);
                @interface.Properties.Add(property);
            }
        }

        private void CreateMembers(MT.VarMeta varMeta, TS.VariableStatement @var)
        {
            @var.TypeAnnotation = this.GetTypeFromEncoding(varMeta.ExtendedEncoding);
        }

        private void CreateMembers(MT.FunctionMeta functionMeta, TS.FunctionDeclaration @function)
        {
            // Add arguments. (parameters)
            this.BuildParameters(functionMeta.Parameters, @function.Parameters);

            // Add return type.
            @function.ReturnType = this.GetTypeFromEncoding(functionMeta.ReturnTypeEncoding);
        }

        private void TrackMeta(MT.Meta meta, TS.TypeScriptObject typeScriptObject)
        {
            typeScriptObject.Annotations.Add(meta);
        }

        private TS.IType GetClassType(MT.InterfaceMeta interfaceMeta)
        {
            return this.classMap[interfaceMeta];
        }

        private TS.IType GetInterfaceType(MT.ProtocolMeta protocolMeta)
        {
            return this.GetTypeForProtocolMetaName(protocolMeta.JSName);
        }

        private TS.IType GetStructType(MT.StructMeta structMeta)
        {
            return this.GetTypeForStructMetaName(structMeta.JSName);
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
