using System.Diagnostics;
using System.Text.RegularExpressions;
using Libclang.Core.Ast;
using Libclang.Core.Common;
using Libclang.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Libclang.Core.Generator
{
    public class TNSBridgeStaticWriter : BaseTNSBridgeWriter
    {
        private readonly MultiDictionary<InterfaceDeclaration, MethodDeclaration> staticInterfaceToMethods;

        private readonly MultiDictionary<string, MethodDeclaration> interfacesToMethods;

        private readonly MultiDictionary<string, PropertyDeclaration> interfacesToPropertiesMap;

        private readonly MultiDictionary<InterfaceDeclaration, Tuple<MethodDeclaration, bool>> mergedInterfaceToMethods
            =
            new MultiDictionary<InterfaceDeclaration, Tuple<MethodDeclaration, bool>>();

        private readonly MultiDictionary<InterfaceDeclaration, MethodDeclaration> interfaceToConstructors =
            new MultiDictionary<InterfaceDeclaration, MethodDeclaration>();

        private readonly string[] typesThatSkipMarshalling = {"NSString", "NSMutableString"};

        protected override string JSContext
        {
            get { return "[ObjCInheritance getJSContextFromJSContextRef: " + JSContextRef + "]"; }
        }

        protected override string JSContextRef
        {
            get { return "ctx"; }
        }

        public TNSBridgeStaticWriter(MultiDictionary<InterfaceDeclaration, MethodDeclaration> staticInterfaceToMethods,
            MultiDictionary<string, MethodDeclaration> interfacesToMethods,
            MultiDictionary<string, PropertyDeclaration> interfacesToPropertiesMap,
            MultiDictionary<FunctionDeclaration, BaseRecordDeclaration> functionToRecords)
            : base(functionToRecords)
        {
            this.staticInterfaceToMethods = staticInterfaceToMethods;
            this.interfacesToMethods = interfacesToMethods;
            this.interfacesToPropertiesMap = interfacesToPropertiesMap;

            foreach (var @interface in staticInterfaceToMethods.Keys)
            {
                MergeMethods(@interface);
            }
            if (mergedInterfaceToMethods.Count() != staticInterfaceToMethods.Count())
            {
                throw new InvalidOperationException("Statics");
            }

            foreach (var @interface in staticInterfaceToMethods.Keys)
            {
                interfaceToConstructors.AddMany(@interface, GetConstructors(@interface).ToList().DistinctBy(x => x.Name));
            }
        }

        private bool ShouldProcessMethod(MethodDeclaration method)
        {
            if (method.Name.IsEqualToAny("copyWithZone:", "mutableCopyWithZone:", "authorizationStatus"))
            {
                return false;
            }

            return true;
        }

        private ICollection<Tuple<MethodDeclaration, bool>> MergeMethods(InterfaceDeclaration @interface)
        {
            var current = mergedInterfaceToMethods.Any(x => x.Key.Name == @interface.Name);
            if (current)
            {
                return mergedInterfaceToMethods.Single(x => x.Key.Name == @interface.Name).Value;
            }

            var result =
                staticInterfaceToMethods.Single(x => x.Key.Name == @interface.Name)
                    .Value.Where(ShouldProcessMethod)
                    .Select(x => Tuple.Create(x, true))
                    .ToList();

            if (HasBaseInterface(@interface))
            {
                foreach (var tuple in MergeMethods(@interface.Base))
                {
                    // TODO: Some static methods are repeated in the interfaces. Remove them from the declarations.
                    if (result.Any(x => x.Item1.Name == tuple.Item1.Name))
                    {
                        continue;
                    }

                    result.Add(Tuple.Create(tuple.Item1, false));
                }
            }

            mergedInterfaceToMethods[@interface] = result;
            return result;
        }

        private IEnumerable<MethodDeclaration> GetConstructors(InterfaceDeclaration @interface)
        {
            while (@interface != null)
            {
                foreach (
                    var ctor in interfacesToMethods[@interface.Name].Where(x => x.IsConstructor /*&& !x.IsVariadic*/))
                {
                    yield return ctor;
                }

                @interface = @interface.Base;
            }
        }

        private string GetFileName(InterfaceDeclaration @interface)
        {
            return @interface.Name + "Static";
        }

        private bool HasBaseInterface(InterfaceDeclaration @interface)
        {
            return @interface.Base != null;
        }

        public void Generate(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            else
            {
                new DirectoryInfo(directory).Clear();
            }

            foreach (var kvp in mergedInterfaceToMethods)
            {
                InterfaceDeclaration @interface = kvp.Key;
                var fileName = GetFileName(@interface);

                using (var headerWriter = new StreamWriter(Path.Combine(directory, fileName + ".h")))
                using (var implementationWriter = new StreamWriter(Path.Combine(directory, fileName + ".m")))
                {
                    {
                        headerWriter.WriteLine(GetImports(kvp.Value.Select(x => x.Item1)));
                        headerWriter.WriteLine(GetImports(interfaceToConstructors[kvp.Key]));
                        headerWriter.WriteLine("#import <TNSBridgeInfrastructure/MarshallingService.h>");
                        headerWriter.WriteLine("#import <TNSBridgeInfrastructure/ObjCInheritance.h>");
                        headerWriter.WriteLine("#import <TNSBridgeInfrastructure/Context.h>");
                        headerWriter.WriteLine("#import <TNSBridgeInfrastructure/TNSRefValue.h>");
                        headerWriter.WriteLine("#import <TNSBridgeInfrastructure/TNSBuffer.h>");
                        headerWriter.WriteLine("#import <TNSBridgeInfrastructure/BigIntWrapper.h>");
                        headerWriter.WriteLine("#import <TNSBridgeInfrastructure/Variadics.h>");
                        headerWriter.WriteLine("#import <TNSBridgeInfrastructure/Metadata.h>");
                        headerWriter.WriteLine("#import <TNSBridgeInfrastructure/ProxyPrototypes.h>");
                        if (@interface.Base != null)
                        {
                            headerWriter.WriteLine("#import \"{0}.h\"", GetFileName(@interface.Base));
                        }
                        headerWriter.WriteLine();

                        //if (HasBaseInterface(kvp.Key))
                        //{
                        //    headerWriter.WriteLine("#import \"{0}.h\"", GetFileName(kvp.Key.Base));
                        //}

                        headerWriter.WriteLine(WriteHeader(kvp.Key));
                    }

                    {
                        implementationWriter.WriteLine("#import \"{0}\"", fileName + ".h");
                        implementationWriter.WriteLine();

                        implementationWriter.WriteLine(GetFunctionPointerBindings(kvp.Key.Methods));
                        implementationWriter.WriteLine();

                        implementationWriter.Write(WriteImplementation(kvp.Key, kvp.Value));
                    }
                }
            }

            GenerateRegisterMethod(directory);
        }

        private void GenerateRegisterMethod(string directory)
        {
            var fileName = "TNSRegisterStatic";

            using (var writer = new StreamWriter(Path.Combine(directory, fileName) + ".h"))
            {
                writer.WriteLine("void TNSRegisterStaticFunctions();");
            }

            using (var writer = new StreamWriter(Path.Combine(directory, fileName) + ".m"))
            {
                foreach (var @interface in mergedInterfaceToMethods.Keys)
                {
                    writer.WriteLine("#import \"{0}\"", GetFileName(@interface) + ".h");
                }

                writer.WriteLine();

                writer.WriteLine("void TNSRegisterStaticFunctions() {");

                foreach (var @interface in mergedInterfaceToMethods.Keys)
                {
                    writer.WriteLine(
                        "    [TNSGetStaticClassMap() setObject:(__bridge id)(void *)&TNSGet{0}Constructor forKey:@\"{0}\"];",
                        @interface.Name);
                }

                writer.WriteLine("}");
            }
        }


        private string WriteHeader(InterfaceDeclaration @interface)
        {
            var result = new StringBuilder();

            result.AppendLine("@protocol TNS{0}Static", @interface.Name);

            foreach (var method in interfaceToConstructors[@interface])
            {
                result.AppendLine("- {0};", GenerateProtocolHeaderMethodSignature(method));
            }

            result.AppendLine("@end");
            result.AppendLine();

            result.AppendLine("JSValue *TNSGet{0}Constructor(JSContext *ctx);", @interface.Name);

            return result.ToString();
        }

        private string WriteImplementation(InterfaceDeclaration @interface,
            ICollection<Tuple<MethodDeclaration, bool>> methods)
        {
            using (var result = new StringWriter())
            {
                IFormatter formatter = new Formatter(result);

                var constructors = interfaceToConstructors[@interface];

                List<MethodDeclaration> allMethods;

                if (typesThatSkipMarshalling.Contains(@interface.Name))
                {
                    allMethods = Enumerable.Concat(constructors, methods.Select(x => x.Item1)).ToList();
                }
                else
                {
                    allMethods =
                        Enumerable.Concat(constructors, methods.Where(x => x.Item2).Select(x => x.Item1)).ToList();
                }

                WriteInvokeConstructorFunction(formatter, @interface);

                WriteClassCreation(formatter, @interface);
                formatter.WriteLine();

                WriteClassRegistration(formatter, @interface);

                return result.ToString();
            }
        }

        private void WriteStaticFunctions(IFormatter formatter, InterfaceDeclaration @interface,
            IList<MethodDeclaration> methods)
        {
            int i = 0;
            foreach (var method in methods)
            {
                formatter.WriteLine(
                    "static JSValueRef {0}(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {{",
                    GenerateCName(@interface, method));
                formatter.Indent();

                formatter.WriteLine(DebugCLog);

                formatter.WriteLine("JSContext *__jsContext = [ObjCInheritance getJSContextFromJSContextRef:ctx];");

                formatter.WriteLine("@try {");
                formatter.Indent();

                formatter.WriteLine(MarshallParametersFromJSValueRef(method.Parameters));
                formatter.WriteLine();


                string makeVariadicCall = string.Empty;

                if (method.IsVariadic)
                {
                    var reciever = @interface.Name;
                    string variadicSelectorAssigment = String.Format("SEL __variadic_selector = @selector({0});",
                        GetVariadicSelectorForMethod(method));
                    string variadicFuncPtrAssigment =
                        String.Format(
                            "IMP __functionPtr = (IMP)class_getMethodImplementation([{0} class] , __variadic_selector);",
                            reciever);
                    makeVariadicCall = String.Format("{0}\n{1}\n{2}", variadicSelectorAssigment,
                        variadicFuncPtrAssigment, GenerateVariadicStaticCall(method, reciever));
                }
                else
                {
                    formatter.WriteLine(
                        "NSString *__className = [JSValue valueWithJSValueRef:thisObject inContext:__jsContext][@\"name\"].toString;");
                    formatter.WriteLine("Class __cls = NSClassFromString(__className);");
                    formatter.WriteLine("SEL __sel = sel_registerName(\"{0}\");", method.Name);
                }

                if (method.ReturnType.IsVoid())
                {
                    formatter.WriteLine((method.IsVariadic)
                        ? "void *__result = NULL;"
                        : GetMethodCall(method, @interface) + ";");
                    formatter.WriteLine(makeVariadicCall);
                    formatter.WriteLine("return JSValueMakeUndefined(ctx);");
                }
                else
                {
                    var returnType = GetActualMethodReturnType(method, @interface);

                    string assigment = (method.IsVariadic)
                        ? String.Format("({0})0", returnType.ToStringInternal(string.Empty))
                        : GetMethodCall(method, @interface);
                    formatter.WriteLine("{0} = {1};", returnType.ToStringInternal("__result"), assigment);
                    formatter.WriteLine(makeVariadicCall);

                    if (method.Name == "appearance")
                    {
                        formatter.WriteLine(
                            string.Format("return {0};",
                                string.Format(MarshallObjectToJSValueFormat, "__result",
                                    string.Format("@\"{0}\"", @interface.Name))).TrimEnd(';') + ".JSValueRef;");
                    }
                    else
                    {
                        bool usePrimitiveTypeMarshalling =
                            !(typesThatSkipMarshalling.Contains(@interface.Name) &&
                              (method.IsConstructor || method.Name.StartsWith("string")));
                        formatter.WriteLine(MarshallResultToJSValueRef(method, @interface, usePrimitiveTypeMarshalling));
                    }
                }

                formatter.Outdent();
                formatter.WriteLine("}");

                formatter.WriteLine("@catch (NSException *__exception) {");
                formatter.Indent();
                formatter.WriteLine(
                    "*exception = [ObjCInheritance handleException:__exception inContext:__jsContext].JSValueRef;");
                formatter.WriteLine("return JSValueMakeUndefined(ctx);");
                formatter.Outdent();
                formatter.WriteLine("}");

                formatter.Outdent();
                formatter.WriteLine("}");

                if (i++ != methods.Count - 1)
                {
                    formatter.WriteLine();
                }
            }
        }

        private string GetMethodCall(MethodDeclaration method, InterfaceDeclaration @interface,
            string argumentPrefix = "marshalled_")
        {
            if (method.IsConstructor)
            {
                var message = string.Format("(__cls = [__cls alloc], {0})",
                    GetObjcMessage(method, @interface, "__cls", argumentPrefix));
                return string.Format("[{0} autorelease]", message);
            }
            else
            {
                return GetObjcMessage(method, @interface, "__cls", argumentPrefix);
            }
        }

        private void WriteStaticFunctionArray(IFormatter formatter, InterfaceDeclaration @interface,
            IEnumerable<MethodDeclaration> methods)
        {
            formatter.WriteLine("static JSStaticFunction TNS_{0}_StaticFunctionArray [] = {{", @interface.Name);
            formatter.Indent();

            foreach (var method in methods)
            {
                formatter.WriteLine(
                    "{{ \"{0}\", {1}, kJSPropertyAttributeReadOnly | kJSPropertyAttributeDontDelete }},",
                    GenerateJavaScriptMethodName(method.Name), GenerateCName(@interface, method));
            }

            formatter.WriteLine("{ 0, 0, 0 }");

            formatter.Outdent();
            formatter.WriteLine("};");
        }

        private object GenerateCName(InterfaceDeclaration @interface, MethodDeclaration method)
        {
            return string.Format("TNS_{0}_{1}_Static", @interface.Name,
                method.Name.TrimEnd(':').Replace(':', '_').CapitalizeFirstLetter());
        }

        private void WriteInvokeConstructorFunction(IFormatter formatter, InterfaceDeclaration @interface)
        {
            formatter.WriteLine(
                "static JSValueRef TNS_{0}_MakeInstance(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {{",
                @interface.Name);
            formatter.Indent();

            formatter.WriteLine("@autoreleasepool {");
            formatter.Indent();

            formatter.WriteLine("JSContext *__jsContext = [ObjCInheritance getJSContextFromJSContextRef:ctx];");

            formatter.WriteLine(
                "id __result = [ObjCInheritance makeInstanceForClass:[{0} class] inContext:{1} withArguments:arguments count:argumentCount exception:exception];",
                @interface.Name, JSContext);

            var type = new PointerType(new DeclarationReferenceType(@interface));
            var method = new FunctionDeclaration("init", type);
            bool usePrimitiveTypeMarshalling = !typesThatSkipMarshalling.Contains(@interface.Name);
            formatter.WriteLine(MarshallResultToJSValueRef(method, @interface, usePrimitiveTypeMarshalling));

            formatter.Outdent();
            formatter.WriteLine("}");

            formatter.Outdent();
            formatter.WriteLine("}");
        }

        private void WriteClassCreation(IFormatter formatter, InterfaceDeclaration @interface)
        {
            string superPrototypeSet = "JSValue *superConstructor = nil;";
            if (@interface.Base != null)
            {
                superPrototypeSet = string.Format(@"JSValue *superConstructor = TNSGet{0}Constructor(ctx);",
                    @interface.Base.Name);
            }

            formatter.WriteLine(@"JSValue *TNSGet{0}Constructor(JSContext *ctx) {{
    // try to get the constructor (if exists)
    NSMutableDictionary *constructors = [ObjCInheritance getConstructorsForContext: ctx];
    JSValue *constructor = (JSValue *)[constructors objectForKey:@""{0}""];

    // if the constructor does not exist - create it
    if (constructor == nil) {{

        // Initialize the constructor function
        JSStringRef createCallbackName = JSStringCreateWithUTF8CString(""tnsCreateNewInstance"");
        constructor = [ctx evaluateScript:@""(function {0}() {{ return {0}.tnsCreateNewInstance.apply(this, arguments); }});""];
        //JSValueProtect(ctx.JSGlobalContextRef, constructor.JSValueRef); // protect the constructor because it is stored in a global variable
        JSObjectRef createCallback = JSObjectMakeFunctionWithCallback(ctx.JSGlobalContextRef, createCallbackName, TNS_{0}_MakeInstance);
        JSObjectSetProperty(ctx.JSGlobalContextRef, (JSObjectRef)constructor.JSValueRef, createCallbackName, createCallback, kJSPropertyAttributeDontDelete | kJSPropertyAttributeDontEnum | kJSPropertyAttributeReadOnly, NULL);
        JSStringRelease(createCallbackName);

        InterfaceMetaInfo *interfaceMeta = [Metadata metadataForIdentifier: @""{0}""];

        {1}
        TNSCreateProxyInstancePrototype(constructor[@""prototype""], superConstructor[@""prototype""].JSValueRef, interfaceMeta);
        TNSCreateProxyClassPrototype(constructor, superConstructor.JSValueRef, interfaceMeta);

        // Set the extend function
        NSMutableDictionary *contextsExtendsFunction = TNSGetContextsExtendFunctions();
        NSNumber* jsContextID = [ObjCInheritance getJSContextID:ctx.JSGlobalContextRef];
        JSValue *extendsFunction = [contextsExtendsFunction objectForKey:jsContextID];
        if (!extendsFunction) {{
            extendsFunction = [ctx evaluateScript:@""(function Extends() {{ return ObjCInheritance.extends(this, arguments[0], arguments[1]); }});""];
            [contextsExtendsFunction setObject:extendsFunction forKey:jsContextID];
        }}
        constructor[@""extends""] = extendsFunction;
        constructor[@""__tns_class""] = [{0} class];

        // Save the constructor in a global cache
        [constructors setObject:constructor forKey:@""{0}""];
    }}

    return constructor;
}}", @interface.Name, superPrototypeSet);
        }

        private void WriteClassRegistration(IFormatter formatter, InterfaceDeclaration @interface)
        {
            formatter.WriteLine("Protocol *TNSRegister{0}Class() {{ return @protocol(TNS{0}Static); }}", @interface.Name);
        }
    }
}
