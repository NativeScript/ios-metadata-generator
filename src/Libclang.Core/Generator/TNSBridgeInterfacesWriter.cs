using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Libclang.Core.Ast;
using Libclang.Core.Types;
using Libclang.Core.Common;

namespace Libclang.Core.Generator
{
    public class TNSBridgeInterfacesWriter : BaseTNSBridgeWriter
    {
        private readonly string path;
        protected readonly Dictionary<InterfaceDeclaration, List<MethodDeclaration>> interfacesToMethodsMap;
        private readonly MultiDictionary<string, PropertyDeclaration> interfacesToPropertiesMap;

        protected readonly Dictionary<InterfaceDeclaration, List<MethodDeclaration>>
            interfacesToInheritedCategoriesMethodsMap;

        private readonly ICollection<string> collectionNames = new HashSet<string>
        {
            "NSArray",
            "NSDictionary",
            "NSMutableArray",
            "NSMutableDictionary",
            "NSMutableOrderedSet",
            "NSMutableSet",
            "NSOrderedSet",
            "NSSet"
        };

        protected override string JSContext
        {
            get { return "[JSContext contextWithJSGlobalContextRef:" + JSContextRef + "]"; }
        }

        protected override string JSContextRef
        {
            get { return "(JSGlobalContextRef)[__instance tns_jscontext]"; }
        }

        public TNSBridgeInterfacesWriter(string path,
            Dictionary<InterfaceDeclaration, List<MethodDeclaration>> interfacesToMethodsMap,
            MultiDictionary<string, PropertyDeclaration> interfacesToPropertiesMap,
            Dictionary<InterfaceDeclaration, List<MethodDeclaration>> interfacesToInheritedCategoriesMethodsMap,
            MultiDictionary<FunctionDeclaration, BaseRecordDeclaration> functionToRecords)
            : base(functionToRecords)
        {
            this.path = path;
            this.interfacesToMethodsMap = interfacesToMethodsMap;
            this.interfacesToPropertiesMap = interfacesToPropertiesMap;
            this.interfacesToInheritedCategoriesMethodsMap = interfacesToInheritedCategoriesMethodsMap;
        }

        public void Generate()
        {
            if (!Directory.Exists(this.path))
            {
                Directory.CreateDirectory(this.path);
            }

            interfacesToMethodsMap.Keys.ToList().ForEach(interfaceDecl => { GenerateBindingsForClass(interfaceDecl); });
        }

        protected virtual void GenerateBindingsForClass(InterfaceDeclaration interfaceDecl)
        {
            GenerateJSExportHeader(interfaceDecl);

            GenerateJSDerivedHeader(interfaceDecl);
            GenerateJSDerivedImplementation(interfaceDecl);

            GenerateJSExposedHeader(interfaceDecl);
            GenerateJSExposedImplementation(interfaceDecl);
        }

        protected StreamWriter WriterForDeclaration(InterfaceDeclaration interfaceDecl, string suffix)
        {
            return new StreamWriter(Path.Combine(this.path, interfaceDecl.Name + suffix));
        }

        #region Protocol

        private const string ExportHeaderTemplate = @"#import <JavaScriptCore/JavaScriptCore.h>
{0}
{1}
{5}

@protocol {2}JSExposedExport <JSExport>
{4}
@end

@protocol {2}JSExport <{2}JSExposedExport, {3}JSExport>
@end
";

        private void GenerateJSExportHeader(InterfaceDeclaration interfaceDecl)
        {
            using (StreamWriter writer = this.WriterForDeclaration(interfaceDecl, "JSExport.h"))
            {
                var baseName = interfaceDecl.Base != null ? (interfaceDecl.Base.Name + "JSExport") : "";
                var forwardProtocol = interfaceDecl.Base != null ? ("@protocol " + baseName + ";") : "";
                var baseProtocolName = interfaceDecl.Base != null ? (baseName + ", ") : "";
                writer.Write(string.Format(ExportHeaderTemplate, GetImports(interfacesToMethodsMap[interfaceDecl]),
                    forwardProtocol, interfaceDecl.Name, baseProtocolName, string.Empty,
                    GenerateProtocolList(interfaceDecl)));
            }
        }

        private string GenerateProtocolList(InterfaceDeclaration interfaceDecl)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("@protocol {0}JSProperties", interfaceDecl.Name);
            foreach (var property in interfacesToPropertiesMap[interfaceDecl.Name])
            {
                var baseInterface = interfaceDecl.Base;
                var hasOverride = false;
                while (baseInterface != null)
                {
                    if (interfacesToPropertiesMap[baseInterface.Name].Any(x => x.Name == property.Name))
                    {
                        hasOverride = true;
                        break;
                    }

                    baseInterface = baseInterface.Base;
                }

                if (hasOverride)
                {
                    continue;
                }
                var attributes = new List<string>() {"assign"};

                if (property.HasCustomGetter)
                {
                    attributes.Add("getter=" + property.Getter.Name);
                }

                if (property.IsReadonly)
                {
                    attributes.Add("readonly");
                }
                else if (property.HasCustomSetter)
                {
                    attributes.Add("setter=" + property.Setter.Name);
                }
                var attributesString = attributes.Any()
                    ? string.Format(" ({0})", string.Join(", ", attributes))
                    : string.Empty;

                sb.AppendLine("@property{0} {1};", attributesString, property.Type.ToString(property.Name));
            }
            sb.AppendLine("@end");

            return sb.ToString();
        }

        private string GenerateJSExportHeaderMethodSignatures(InterfaceDeclaration interfaceDecl)
        {
            StringBuilder sb = new StringBuilder();

            if (interfaceDecl.Name == "NSObject")
            {
                sb.AppendLine("-(NSString *)dispatch_class;");
                sb.AppendLine("-(BOOL)dispatch_isKindOfClass:(NSString *)cls;");
                sb.AppendLine("-(NSUInteger)dispatch_hash;");
            }

            if (interfaceDecl.Name == "NSString")
            {
                sb.AppendLine("-(NSString *)dispatch_toString;");
            }

            if (collectionNames.Contains(interfaceDecl.Name))
            {
                sb.AppendLine("-(NSUInteger)dispatch_count;");
            }

            sb.AppendLine();

            foreach (
                MethodDeclaration method in
                    GetDerivedMethods(interfaceDecl, withCategories: true, withOptional: true, withImplicit: true))
            {
                string methodSignature = GenerateJSExportHeaderMethodSignature(GetDispatchMethod(method), "dispatch_");
                sb.AppendFormat("{0};\n", methodSignature);
            }

            return sb.ToString().TrimEnd();
        }

        #endregion

        #region Derived

        protected virtual string DerivedHeaderTemplate
        {
            get { return @"#import ""{0}JSExport.h""
#import <TNSBridgeInfrastructure/JSDerivedProtocol.h>
#import <TNSBridgeInfrastructure/JSExposedProtocol.h>
#import <TNSBridgeInfrastructure/Variadics.h>

@interface {0}JSDerived : {0} <{0}JSExport, JSDerivedProtocol>
{1}
@end"; }
        }

        protected virtual void GenerateJSDerivedHeader(InterfaceDeclaration interfaceDecl)
        {
            using (StreamWriter writer = this.WriterForDeclaration(interfaceDecl, "JSDerived.h"))
            {
                var methods = new StringBuilder();

                foreach (
                    var method in
                        GetDerivedMethods(interfaceDecl, withCategories: true, withOptional: true, withImplicit: true))
                {
                    methods.AppendLine("+ {0};",
                        GetMethodSignature(GetMarshallMethod(method), true, "marshall_")
                            .Replace("(JSValue*)__instance", "(id)__instance"));
                }

                writer.Write(string.Format(DerivedHeaderTemplate, interfaceDecl.Name, methods.ToString().TrimEnd()));
            }
        }

        protected virtual string DerivedImplementationTemplate
        {
            get { return @"#import <JavaScriptCore/JavaScriptCore.h>
#import ""{0}JSDerived.h""
#import <TNSBridgeInfrastructure/ObjectWrapper.h>
#import <TNSBridgeInfrastructure/MarshallingService.h>
#import <TNSBridgeInfrastructure/ObjCInheritance.h>
#import <TNSBridgeInfrastructure/ObjCInheritance+IsValidOverride.h>
#import <TNSBridgeInfrastructure/TNSRefValue.h>
#import <TNSBridgeInfrastructure/TNSBuffer.h>
#import <TNSBridgeInfrastructure/BigIntWrapper.h>
#import <TNSBridgeInfrastructure/JSValue+Extensions.h>
{3}
{7}

@implementation {0}JSDerived{{
    BOOL _unprotectedOnce;
    BOOL _isDeallocating;
}}

@synthesize tns_overridenMethods = _tns_overridenMethods, tns_jscontext = _tns_jscontext, tns_object = _tns_object, tns_lock = _tns_lock;

{4}

-(id)retain{{
    if (_unprotectedOnce && [self retainCount] == 1) {{
        JSValueProtect(self.tns_jscontext, self.tns_object);
    }}
    //return [super retain];
    return [NSObject instanceMethodForSelector:@selector(retain)](self, _cmd);
}}
-(oneway void)release{{
    if (self.tns_jscontext) {{
        if ([self retainCount] == 2) {{
            JSValueUnprotect(self.tns_jscontext, self.tns_object);
            _unprotectedOnce = YES;
        }}
    }}
    //[super release];
    [NSObject instanceMethodForSelector:@selector(release)](self, _cmd);
}}
-(BOOL)isDeallocating {{
    return _isDeallocating;
}}
-(void)dealloc{{
    [self.tns_overridenMethods release];
    [self.tns_lock release];
    _isDeallocating = YES;
    [super dealloc];
}}

{6}

{1}
@end
"; }
        }

        protected const string NativeMethodOverride = @"
// {method_location}
- {normalSignature} {
    {debug_log}
    [self.tns_lock lock];

    JSObjectRef __jsMethod = NULL;
    if (![self isDeallocating]) {{
        __jsMethod = [ObjCInheritance updateOverride:self forSelector:_cmd andNativeProperty:{property_name}];
    }} else if ([ObjCInheritance isValidOverrideForClass:NSStringFromClass([self class]) andMethod:@""{method_name}""]) {{
        NSLog(@""Method '%@-%@' called during deallocation. Please report."", [self class], NSStringFromSelector(_cmd));
    }}

    {declare_return_var}
    if (__jsMethod) {
        @autoreleasepool {
            JSContext *__jsContext = [JSContext contextWithJSGlobalContextRef:(JSGlobalContextRef)self.tns_jscontext];
            {marshall_arguments_to_js}
            {call_init_if_constructor}
            JSValueRef __exception = NULL;
            {jsvalueref_result_declaration}JSObjectCallAsFunction(self.tns_jscontext, __jsMethod, self.tns_object, {args_count}, __args , &__exception);
            if (__exception) __jsContext.exceptionHandler(__jsContext, [JSValue valueWithJSValueRef:__exception inContext:__jsContext]);
            {assign_return_prefix}{marshall_result_from_js}
        }
    } else {
        {comment_if_variadic}{assign_return_prefix}{call_super}
        {normal_variadic_selector_assigment}{normal_variadic_funcPtr_assigment}{normal_make_variadic_call}
    }
    [self.tns_lock unlock];
    {return_value}
}";

        protected const string DispatchMethods = @"
";

        protected virtual void GenerateJSDerivedImplementation(InterfaceDeclaration interfaceDecl)
        {
            using (StreamWriter writer = this.WriterForDeclaration(interfaceDecl, "JSDerived.m"))
            {
                string classMethods = string.Empty;

                if (interfaceDecl.Name == "NSObject")
                {
                    classMethods = @"
-(NSString *)dispatch_class { return NSStringFromClass([self class]); }
-(BOOL)dispatch_isKindOfClass:(NSString *)cls { return [self isKindOfClass:NSClassFromString(cls)]; }
-(NSUInteger)dispatch_hash { return [self hash]; }";
                }

                if (collectionNames.Contains(interfaceDecl.Name))
                {
                    classMethods = @"-(NSUInteger)dispatch_count { return [self count]; }";
                }

                IEnumerable<MethodDeclaration> methods =
                    interfacesToMethodsMap[interfaceDecl].Union(interfacesToInheritedCategoriesMethodsMap[interfaceDecl]);

                writer.Write(
                    DerivedImplementationTemplate,
                    interfaceDecl.Name,
                    GenerateJSDerivedMethodsImplementations(interfaceDecl),
                    string.Empty,
                    GetImports(methods),
                    GenerateStaticConstructor(interfaceDecl, x => x.Name + "JSDerived", true),
                    "[ObjCInheritance getJSContextFromJSContextRef:(JSGlobalContextRef)self.tns_jscontext]",
                    classMethods,
                    GetFunctionPointerBindings(methods));

                writer.Write("Protocol *TNSRegister{0}Properties() {{ return @protocol({0}JSProperties); }}",
                    interfaceDecl.Name);
            }
        }

        protected virtual string GenerateJSDerivedMethodsImplementations(InterfaceDeclaration interfaceDecl)
        {
            List<MethodDeclaration> methods = GetDerivedMethods(interfaceDecl, withCategories: true, withOptional: true,
                withImplicit: true);

            StringBuilder sb = new StringBuilder();
            foreach (MethodDeclaration method in methods)
            {
                sb.AppendLine(GenerateJSDerivedMethodImplementation(interfaceDecl, method, isCategory: false));
            }
            return sb.ToString();
        }

        private List<MethodDeclaration> GetDerivedMethods(InterfaceDeclaration interfaceDecl, bool withCategories,
            bool withOptional, bool withImplicit)
        {
            List<MethodDeclaration> methods = new List<MethodDeclaration>(interfacesToMethodsMap[interfaceDecl]);
            if (withCategories && interfacesToInheritedCategoriesMethodsMap.ContainsKey(interfaceDecl))
            {
                methods.AddRange(interfacesToInheritedCategoriesMethodsMap[interfaceDecl]);
            }
            if (!withOptional)
            {
                methods = methods.FindAll(x => !x.IsOptional);
            }
            if (!withImplicit)
            {
                methods = methods.FindAll(x => !x.IsImplicit);
            }
            return methods;
        }

        protected string GenerateJSDerivedMethodImplementation(InterfaceDeclaration interfaceDecl,
            MethodDeclaration method, bool isCategory)
        {
            TypeDefinition resolvedReturnType = method.ReturnType.Resolve();
            bool isVoid = (resolvedReturnType is PrimitiveType) &&
                          (resolvedReturnType as PrimitiveType).Type == PrimitiveTypeType.Void;

            string declare_return_var = string.Empty;
            string return_value = string.Empty;
            string assign_return_prefix = string.Empty;
            string result_declaration = "// No result";
            string result_assignment = string.Empty;
            string marshall_result_to_js = "// No return";
            string marshall_result_from_js = string.Empty;
            string call_init_if_constructor = string.Empty;
            string jsvalueref_result_declaration = string.Empty;

            int args_count = method.Parameters.Count;

            string call_super = GetSuperMessage(method, interfaceDecl, "self").EscapeBraces();
            string call_super_instance =
                GetSuperMessage(method, interfaceDecl, "__instance", "marshalled_").EscapeBraces();

            if (!method.ReturnType.IsVoid())
            {
                TypeDefinition actualReturnType = GetActualMethodReturnType(method, interfaceDecl);
                declare_return_var = string.Format("{0} = {1};", actualReturnType.ToString("__return_value"),
                    method.ReturnType.GetDefaultValue());
                assign_return_prefix = "__return_value = ";
                result_declaration = actualReturnType.ToString("__result") + ";";
                return_value = "return __return_value;";
                result_assignment = "__result = ";
                marshall_result_from_js = MarshallResultFromJSValueRef(method.ReturnType);

                marshall_result_to_js = MarshallResultToJSValue(method, interfaceDecl);
                jsvalueref_result_declaration = "JSValueRef __result = ";
            }

            string marshalled_message = GetMessage(method,
                method.Parameters.Select(x => "marshalled_" + x.Name).ToArray());
            string marshall_parameters_from_js = GenerateParametersMarshallingFromJS(method.Parameters);
            string marshall_arguments_to_js = GenerateArgumentsMarshallingToJS(method.Parameters);
            string retain_refvalues = GenerateRetainRefvalues(method.Parameters);

            if (method.IsConstructor)
            {
                marshall_result_from_js = "self;";
                jsvalueref_result_declaration = string.Empty;
                call_init_if_constructor = string.Format("self = {0};", call_super);
            }

            // variadic variables
            string comment_if_variadic = string.Empty;
            string normal_variadic_selector_assigment = string.Empty;
            string normal_variadic_funcPtr_assigment = string.Empty;
            string normal_make_variadic_call = string.Empty;
            string marshall_variadic_selector_assigment = string.Empty;
            string marshall_variadic_funcPtr_assigment = string.Empty;
            string marshall_make_variadic_call = string.Empty;

            if (method.IsVariadic)
            {
                comment_if_variadic = "//";

                normal_variadic_selector_assigment = String.Format("SEL __variadic_selector = @selector({0});",
                    GetVariadicSelectorForMethod(method));
                normal_variadic_funcPtr_assigment = String.Format("\nIMP __functionPtr = {0};",
                    GetSuperMessage(method, interfaceDecl, "self", string.Empty, GetVariadicSelectorForMethod(method),
                        false).EscapeBraces());
                normal_make_variadic_call = GenerateVariadicJSDerivedCall(method);

                marshall_variadic_selector_assigment = String.Format("SEL __variadic_selector = @selector({0});",
                    GetVariadicSelectorForMethod(method));
                marshall_variadic_funcPtr_assigment = String.Format("\nIMP __functionPtr = {0};",
                    GetSuperMessage(method, interfaceDecl, "__instance", "marshalled_",
                        GetVariadicSelectorForMethod(method), false).EscapeBraces());
                marshall_make_variadic_call = GenerateVariadicJSDerivedCall(method, true);
            }

            string self_message;
            {
                MethodDeclaration marshalledMethod = GetMarshallMethod(method);
                var arguments = marshalledMethod.Parameters.Select(x => x.Name).ToArray();
                arguments[arguments.Length - 2] = "self";
                arguments[arguments.Length - 1] = "__callType";
                self_message = GetMessage(marshalledMethod, arguments, "marshall_");
            }

            string normalSignature = GetMethodSignature(method, false);
            string marshallSignature =
                GetMethodSignature(GetMarshallMethod(method), true, "marshall_")
                    .Replace("(JSValue*)__instance", "(id)__instance");
            string dispatchSignature = GetMethodSignature(GetDispatchMethod(method), true, "dispatch_");
            string baseDispatchSignature = GetMethodSignature(method, true, "base_dispatch_");

            string template;
            if (interfacesToInheritedCategoriesMethodsMap[interfaceDecl].Contains(method))
            {
                if (isCategory)
                {
                    template = NativeMethodOverride;
                }
                else
                {
                    template = DispatchMethods;
                }
            }
            else
            {
                if (method.IsOptional)
                {
                    template = DispatchMethods;
                }
                else
                {
                    template = NativeMethodOverride + DispatchMethods;
                }
            }

            string methodLocation = string.Format("{0}", method.Parent.FullName, method.Parent.Location.ToString());

            StringBuilder sb = new StringBuilder(template)
                .Replace("{declare_return_var}", declare_return_var)
                .Replace("{return_value}", return_value)
                .Replace("{assign_return_prefix}", assign_return_prefix)
                .Replace("{result_declaration}", result_declaration)
                .Replace("{result_assignment}", result_assignment)
                .Replace("{marshall_result_to_js}", marshall_result_to_js)
                .Replace("{call_init_if_constructor}", call_init_if_constructor)
                .Replace("{marshall_result_from_js}", marshall_result_from_js)
                .Replace("{args_count}", args_count.ToString())
                .Replace("{self_message}", self_message)
                .Replace("{interface_name}", interfaceDecl.Name)
                .Replace("{method_name}", method.Name)
                .Replace("{marshalled_message}", marshalled_message)
                .Replace("{marshall_parameters_from_js}", marshall_parameters_from_js.EscapeBraces())
                .Replace("{marshall_arguments_to_js}", marshall_arguments_to_js)
                .Replace("{retain_refvalues}", retain_refvalues)
                .Replace("{jsvalueref_result_declaration}", jsvalueref_result_declaration)
                .Replace("{call_super}", call_super)
                .Replace("{call_super_instance}", call_super_instance)
                .Replace("{normalSignature}", normalSignature)
                .Replace("{marshallSignature}", marshallSignature)
                .Replace("{dispatchSignature}", dispatchSignature)
                .Replace("{baseDispatchSignature}", baseDispatchSignature)
                .Replace("{debug_log}", DebugLog)
                .Replace("{method_location}", methodLocation)
                .Replace("{comment_if_variadic}", comment_if_variadic)
                .Replace("{normal_variadic_selector_assigment}", normal_variadic_selector_assigment)
                .Replace("{normal_variadic_funcPtr_assigment}", normal_variadic_funcPtr_assigment)
                .Replace("{normal_make_variadic_call}", normal_make_variadic_call)
                .Replace("{marshall_variadic_selector_assigment}", marshall_variadic_selector_assigment)
                .Replace("{marshall_variadic_funcPtr_assigment}", marshall_variadic_funcPtr_assigment)
                .Replace("{marshall_make_variadic_call}", marshall_make_variadic_call)
                .Replace("{property_name}", GetPropertyName(method))
                ;

            return sb.ToString().Replace("{{", "{").Replace("}}", "}");
        }

        #endregion

        #region Exposed

        private const string ExposedHeaderTemplate = @"#import ""{0}JSExport.h""
#import <TNSBridgeInfrastructure/JSExposedProtocol.h>
{2}
@interface {0}JSExposed : {1} <{0}JSExposedExport, JSExposedProtocol>
@end";

        private void GenerateJSExposedHeader(InterfaceDeclaration interfaceDecl)
        {
            using (StreamWriter writer = this.WriterForDeclaration(interfaceDecl, "JSExposed.h"))
            {
                string baseClass = "NSObject";
                string baseImport = string.Empty;

                if (interfaceDecl.Base != null)
                {
                    baseClass = interfaceDecl.Base.Name + "JSExposed";
                    baseImport = string.Format("#import \"{0}JSExposed.h\"", interfaceDecl.Base.Name);
                }

                writer.Write(string.Format(ExposedHeaderTemplate, interfaceDecl.Name, baseClass, baseImport));
            }
        }

        private const string ExposedImplementationTemplate = @"#import <JavaScriptCore/JavaScriptCore.h>
#import ""{0}JSExposed.h""
#import ""{0}JSDerived.h""
#import <TNSBridgeInfrastructure/ObjectWrapper.h>
#import <TNSBridgeInfrastructure/MarshallingService.h>
#import <TNSBridgeInfrastructure/ObjCInheritance.h>
#import <TNSBridgeInfrastructure/TNSRefValue.h>
#import <TNSBridgeInfrastructure/TNSBuffer.h>
#import <TNSBridgeInfrastructure/BigIntWrapper.h>
#import <TNSBridgeInfrastructure/Variadics.h>
{3}
{6}

void TNSGet{0}Constructor(JSContext *);

@implementation {0}JSExposed{{
    BOOL _unprotectedOnce;
}}

@synthesize tns_instance = _tns_instance, tns_overridenMethods = _tns_overridenMethods, tns_jscontext = _tns_jscontext, tns_object = _tns_object, tns_lock = _tns_lock;

{4}

-(void)dealloc{{
    [self.tns_instance release];
    object_dispose(self);
}}

-(id)TNS_initWithObject:(id)nativeInstance withContext:(JSContext *)jsContext;
{{
    TNSGet{0}Constructor(jsContext);
    self.tns_instance = nativeInstance;
    self.tns_jscontext = [jsContext JSGlobalContextRef];
    self.tns_object = (JSObjectRef)[[JSValue valueWithNewObjectInContext:jsContext] JSValueRef];
    JSValueProtect(self.tns_jscontext, self.tns_object);
    return self;
}}

{5}
{1}
@end
";

        private bool HasBaseClass(InterfaceDeclaration interfaceDecl, string baseClassName)
        {
            while (interfaceDecl != null)
            {
                if (interfaceDecl.Name == baseClassName)
                {
                    return true;
                }

                interfaceDecl = interfaceDecl.Base;
            }

            return false;
        }

        private void GenerateJSExposedImplementation(InterfaceDeclaration interfaceDecl)
        {
            using (StreamWriter writer = this.WriterForDeclaration(interfaceDecl, "JSExposed.m"))
            {
                string classMethods = string.Empty;

                if (HasBaseClass(interfaceDecl, "NSObject"))
                {
                    classMethods = @"
-(NSString *)dispatch_class { return NSStringFromClass([self.tns_instance class]); }
-(BOOL)dispatch_isKindOfClass:(NSString *)cls { return [self.tns_instance isKindOfClass:NSClassFromString(cls)]; }
-(NSUInteger)dispatch_hash { return [self hash]; }
-(NSUInteger)hash { return [self.tns_instance hash]; }
";
                }

                if (HasBaseClass(interfaceDecl, "NSString"))
                {
                    classMethods = @"
-(NSString *)dispatch_toString { return self.description; }
";
                }

                if (collectionNames.Any(x => HasBaseClass(interfaceDecl, x)))
                {
                    classMethods = @"
-(NSUInteger)count { return [self.tns_instance count]; }
-(NSUInteger)dispatch_count { return [self count]; }";
                }

                IEnumerable<MethodDeclaration> methods =
                    interfacesToMethodsMap[interfaceDecl].Union(interfacesToInheritedCategoriesMethodsMap[interfaceDecl]);
                writer.Write(
                    ExposedImplementationTemplate,
                    interfaceDecl.Name,
                    string.Empty, // GenerateJSExposedMethodsImplementations(interfaceDecl)
                    string.Empty,
                    GetImports(methods),
                    string.Empty, // GenerateStaticConstructor(interfaceDecl, x => x.Name + "JSExposed", false),
                    classMethods,
                    GetFunctionPointerBindings(methods));
            }
        }

        private string GenerateJSExposedMethodsImplementations(InterfaceDeclaration interfaceDecl)
        {
            List<MethodDeclaration> methods = new List<MethodDeclaration>(interfacesToMethodsMap[interfaceDecl]);
            if (interfacesToInheritedCategoriesMethodsMap.ContainsKey(interfaceDecl))
            {
                methods.AddRange(interfacesToInheritedCategoriesMethodsMap[interfaceDecl]);
            }

            StringBuilder sb = new StringBuilder();
            foreach (MethodDeclaration method in methods)
            {
                if (!method.IsConstructor)
                {
                    sb.AppendLine(GenerateJSExposedMethodImplementation(interfaceDecl, method));
                }
            }
            return sb.ToString();
        }

        protected const string ExposedMethodImplementationTemplate = @"
-{normal_signature} {
    [self.tns_lock lock];
    {comment_if_variadic}{declare_return_value}[(({interface_name} *)self.tns_instance) {normal_message}];
{varaidic_result_declaration}
{variadic_selector_assigment}
{variadic_funcPtr_assigment}
{make_variadic_call}
    [self.tns_lock unlock];
    {return_value}
}
-{dispatch_signature} {
    {debug_log}
    {return}[{interface_name}JSDerived {marshall_message}];
}";

        private string GenerateJSExposedMethodImplementation(InterfaceDeclaration interfaceDecl,
            MethodDeclaration method)
        {
            string @return = method.ReturnType.IsVoid() ? string.Empty : "return ";
            string normal_signature = GetMethodSignature(method, false);
            string dispatch_signature = GetMethodSignature(GetDispatchMethod(method), true, "dispatch_");

            string marshall_message;
            {
                MethodDeclaration marshalledMethod = GetMarshallMethod(method);
                var arguments = marshalledMethod.Parameters.Select(x => x.Name).ToArray();
                arguments[arguments.Length - 2] = "self";
                arguments[arguments.Length - 1] = "kTNSCallTypeInstance";
                marshall_message = GetMessage(marshalledMethod, arguments, "marshall_");
            }
            string normal_message = GetMessage(method, method.Parameters.Select(x => x.Name).ToArray());

            string declare_return_value = string.Empty;
            string return_value = string.Empty;

            if (!method.ReturnType.IsVoid())
            {
                declare_return_value = string.Format("{0} = ", method.ReturnType.ToString("__return_value"));
                return_value = string.Format("return __return_value;");
            }

            // Variadic variables
            string comment_if_variadic = string.Empty;
            string make_variadic_call = string.Empty;
            string variadic_selector_assigment = string.Empty;
            string variadic_funcPtr_assigment = string.Empty;
            string varaidic_result_declaration = string.Empty;

            if (method.IsVariadic)
            {
                comment_if_variadic = "//";
                if (method.ReturnType.IsVoid())
                {
                    varaidic_result_declaration = "void *__return_value = NULL;";
                }
                else
                {
                    varaidic_result_declaration = String.Format("{0} = ({1})0;",
                        method.ReturnType.ToStringInternal("__return_value"),
                        method.ReturnType.ToStringInternal(string.Empty));
                }
                variadic_selector_assigment = String.Format("SEL __variadic_selector = @selector({0});",
                    GetVariadicSelectorForMethod(method));
                variadic_funcPtr_assigment =
                    String.Format(
                        "\nIMP __functionPtr = (IMP)class_getMethodImplementation(object_getClass((({0} *)self.tns_instance)), __variadic_selector);",
                        interfaceDecl.Name);
                make_variadic_call = GenerateVariadicJSExposedCall(method);
            }

            StringBuilder sb = new StringBuilder(ExposedMethodImplementationTemplate)
                .Replace("{interface_name}", interfaceDecl.Name)
                .Replace("{return}", @return)
                .Replace("{normal_signature}", normal_signature)
                .Replace("{dispatch_signature}", dispatch_signature)
                .Replace("{debug_log}", DebugLog)
                .Replace("{marshall_message}", marshall_message)
                .Replace("{normal_message}", normal_message)
                .Replace("{declare_return_value}", declare_return_value)
                .Replace("{return_value}", return_value)
                .Replace("{comment_if_variadic}", comment_if_variadic)
                .Replace("{varaidic_result_declaration}", varaidic_result_declaration)
                .Replace("{variadic_selector_assigment}", variadic_selector_assigment)
                .Replace("{variadic_funcPtr_assigment}", variadic_funcPtr_assigment)
                .Replace("{make_variadic_call}", make_variadic_call)
                ;

            return System.Text.RegularExpressions.Regex.Replace(sb.ToString(), "(\r\n)+", "\r\n");
        }

        #endregion

        private MethodDeclaration GetMarshallMethod(MethodDeclaration method)
        {
            MethodDeclaration marshalledMethod = new MethodDeclaration(null, method.Name + "withReciever:CallType:",
                method.ReturnType, method.TypeEncoding);
            marshalledMethod.Parameters.AddRange(method.Parameters);
            marshalledMethod.Parameters.AddRange(new[]
            {
                new ParameterDeclaration("__instance", new IdType()),
                new ParameterDeclaration("__callType", new PrimitiveType(PrimitiveTypeType.Int))
            });
            return marshalledMethod;
        }

        private MethodDeclaration GetDispatchMethod(MethodDeclaration method)
        {
            MethodDeclaration marshalledMethod = new MethodDeclaration(null, method.Name + "CallType:",
                method.ReturnType, method.TypeEncoding);
            marshalledMethod.Parameters.AddRange(method.Parameters);
            marshalledMethod.Parameters.AddRange(new[]
            {
                new ParameterDeclaration("__callType", new PrimitiveType(PrimitiveTypeType.Int))
            });
            return marshalledMethod;
        }

        private static string GenerateRetainRefvalues(IEnumerable<ParameterDeclaration> parameters)
        {
            var sb = new StringBuilder();

            foreach (
                var param in
                    parameters.Where(
                        p =>
                            p.Type is PointerType && ((PointerType) p.Type).Target.IsObjCType() &&
                            !(((PointerType) p.Type).Target is IdType)))
            {
                sb.AppendLine("[refvalue_{0} retainPtr];", param.Name);
            }

            return sb.ToString();
        }

        protected string GenerateStaticConstructor(InterfaceDeclaration interfaceDecl,
            Func<InterfaceDeclaration, string> interfaceFormatter, bool addInitialize)
        {
            if (interfaceDecl.Base == null)
            {
                return string.Empty;
            }

            string format = "@class {1};";
            if (addInitialize)
            {
                format += @"
+ (void)initialize {{
    if (self != [{0} class]) return;
    [ObjCInheritance addMethodsFromClass:[{1} class] toClass:[self class]];
}}";
            }

            return string.Format(format, interfaceFormatter(interfaceDecl), interfaceFormatter(interfaceDecl.Base));
        }
    }
}
