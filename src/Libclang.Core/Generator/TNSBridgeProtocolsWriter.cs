using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Libclang.Core.Ast;
using Libclang.Core.Common;
using Libclang.Core.Types;

namespace Libclang.Core.Generator
{
    public class TNSBridgeProtocolsWriter : BaseTNSBridgeWriter
    {
        private readonly string path;
        private readonly Dictionary<ProtocolDeclaration, List<MethodDeclaration>> protocols;
        private string[] protocolsToSkip = {"UITextSelecting", "CIFilterConstructor"};

        protected override string JSContext
        {
            get { return "[JSContext contextWithJSGlobalContextRef:" + JSContextRef + "]"; }
        }

        protected override string JSContextRef
        {
            get { return "__cContext"; }
        }

        public TNSBridgeProtocolsWriter(string path, Dictionary<ProtocolDeclaration, List<MethodDeclaration>> protocols,
            MultiDictionary<FunctionDeclaration, BaseRecordDeclaration> functionToRecords)
            : base(functionToRecords)
        {
            this.path = path;
            this.protocols = protocols;
        }

        public void Generate()
        {
            if (!Directory.Exists(this.path))
            {
                Directory.CreateDirectory(this.path);
            }

            protocols.Keys.AsParallel().ForAll(protocol => { GenerateBindingsForProtocol(protocol); });
        }

        private void GenerateBindingsForProtocol(ProtocolDeclaration protocol)
        {
            GenerateProtocolImplementationHeader(protocol);
            if (!protocolsToSkip.Contains(protocol.Name))
                GenerateProtocolImplementation(protocol);
        }

        private const string ProtocolImplementationHeaderTemplate = @"#import <Foundation/Foundation.h>

@interface {0}Implementation : NSObject <{0}>

{1}

@end";

        private void GenerateProtocolImplementationHeader(ProtocolDeclaration protocol)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(this.path, protocol.Name + "Implementation.h")))
            {
                writer.Write(string.Format(ProtocolImplementationHeaderTemplate, protocol.Name,
                    GenerateJSDerivedExportHeaderMethodSignatures(protocol)));
            }
        }

        private string GenerateJSDerivedExportHeaderMethodSignatures(ProtocolDeclaration protocol)
        {
            List<MethodDeclaration> methods;
            if (!this.protocols.TryGetValue(protocol, out methods))
            {
                throw new Exception("Protocol methods not found.");
            }

            StringBuilder sb = new StringBuilder();
            foreach (MethodDeclaration method in methods)
            {
                sb.AppendFormat("-{0};\n", GenerateProtocolHeaderMethodSignature(method));
            }

            return sb.ToString();
        }


        private const string ProtocolImplementationTemplate = @"#import <JavaScriptCore/JavaScriptCore.h>
#import ""{0}Implementation.h""
#import <TNSBridgeInfrastructure/MarshallingService.h>
#import <TNSBridgeInfrastructure/ObjCInheritance.h>
#import <TNSBridgeInfrastructure/TNSRefValue.h>
#import <TNSBridgeInfrastructure/TNSBuffer.h>
#import <TNSBridgeInfrastructure/BigIntWrapper.h>
{1}

@implementation {0}Implementation

{2}

@end
";

        private void GenerateProtocolImplementation(ProtocolDeclaration protocol)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(this.path, protocol.Name + "Implementation.m")))
            {
                writer.Write(ProtocolImplementationTemplate, protocol.Name, GetImports(protocols[protocol]),
                    GenerateProtocolImplementationMethodsImplementations(protocol));
            }
        }

        private string GenerateProtocolImplementationMethodsImplementations(ProtocolDeclaration protocol)
        {
            List<MethodDeclaration> methods;
            if (!this.protocols.TryGetValue(protocol, out methods))
            {
                throw new Exception("Protocol methods not found.");
            }

            StringBuilder sb = new StringBuilder();
            foreach (MethodDeclaration method in methods)
            {
                sb.AppendFormat("{0};\n", GenerateProtocolImplementationMethodImplementation(method));
            }

            return sb.ToString();
        }

        private const string ProtocolImplementationMethodImplementationsTemplate = @"
-{0}{{
    id<JSDerivedProtocol> instance = (id<JSDerivedProtocol>)self;
    JSContext *__jsContext = [ObjCInheritance getJSContextFromJSContextRef:instance.tns_jscontext];
    JSObjectRef __jsMethod = [ObjCInheritance getOverridenMethod:instance forSelector:_cmd andNativeProperty:{property_name}];

    if (__jsMethod == NULL) {{
        @throw ([NSException exceptionWithName:@""NotImplementedException"" reason:@""Method is not implemented"" userInfo:nil]);
    }}

    {debug_log}
	{marshall_arguments_to_js}
    JSValueRef __exception = NULL;
	{jsvalueref_result_declaration}JSObjectCallAsFunction(instance.tns_jscontext, (JSObjectRef)__jsMethod, instance.tns_object, {args_count}, __args , &__exception);
    if (__exception) __jsContext.exceptionHandler(__jsContext, [JSValue valueWithJSValueRef:__exception inContext:__jsContext]);
	{return_keyword} {marshall_result_from_js}
}}";

        private string GenerateProtocolImplementationMethodImplementation(MethodDeclaration method)
        {
            TypeDefinition resolvedReturnType = method.ReturnType.Resolve();
            bool isVoid = (resolvedReturnType is PrimitiveType) &&
                          (resolvedReturnType as PrimitiveType).Type == PrimitiveTypeType.Void;

            string marshall_arguments_to_js = GenerateArgumentsMarshallingToJS(method.Parameters).EscapeBraces();
            string jsvalueref_result_declaration = string.Empty;
            string marshall_result_from_js = string.Empty;
            string return_keyword = string.Empty;

            if (!isVoid)
            {
                jsvalueref_result_declaration = "JSValueRef __result = ";
                marshall_result_from_js = MarshallResultFromJSValueRef(method.ReturnType);
                return_keyword = "return";
            }

            StringBuilder sb = new StringBuilder(ProtocolImplementationMethodImplementationsTemplate)
                .Replace("{marshall_result_from_js}", marshall_result_from_js)
                .Replace("{marshall_arguments_to_js}", marshall_arguments_to_js)
                .Replace("{jsvalueref_result_declaration}", jsvalueref_result_declaration)
                .Replace("{args_count}", method.Parameters.Count.ToString())
                .Replace("{return_keyword}", return_keyword)
                .Replace("{debug_log}", DebugLog)
                .Replace("{property_name}", GetPropertyName(method))
                ;


            string normalSignature = GenerateProtocolHeaderMethodSignature(method);

            return string.Format(sb.ToString(), normalSignature);
        }
    }
}
