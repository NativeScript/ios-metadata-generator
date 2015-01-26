using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Libclang.Core.Ast;
using Libclang.Core.Common;
using Libclang.Core.Types;

namespace Libclang.Core.Generator
{
    public abstract class BaseTNSBridgeWriter
    {
        protected abstract string JSContext { get; }
        protected abstract string JSContextRef { get; }

        protected const string DebugLog =
            "#if defined(DEBUG_LOG) && DEBUG_LOG \n    NSLog(@\"    %@ %@ %s\", [self class], NSStringFromSelector(_cmd), strrchr(__FILE__, '/')); \n    #endif";

        protected const string DebugCLog =
            "#if defined(DEBUG_LOG) && DEBUG_LOG \n    NSLog(@\"    %s %s\", __FUNCTION__, strrchr(__FILE__, '/')); \n    #endif";

        protected readonly MultiDictionary<FunctionDeclaration, BaseRecordDeclaration> functionToRecords;

        protected BaseTNSBridgeWriter(MultiDictionary<FunctionDeclaration, BaseRecordDeclaration> functionToRecords)
        {
            this.functionToRecords = functionToRecords;
        }

        protected string GetImports(IEnumerable<FunctionDeclaration> functions)
        {
            var sb = new StringBuilder();

            var imports =
                functions.Where(functionToRecords.ContainsKey)
                    .SelectMany(x => functionToRecords[x])
                    .DistinctBy(x => x.GetFileName());

            foreach (var import in imports)
            {
                //sb.AppendLine("#import \"../../PlainC/Generated/{0}\"", import.GetFileName() + ".h");
                sb.AppendLine("#import \"{0}\"", import.GetFileName() + ".h");
            }

            return sb.ToString().TrimEnd();
        }

        private const string MarshallJSValueToJSValueFormat = "__args[{0}] = [{1} JSValueRef];";
        private const string MarshallPrimitiveToJSValueFormat = "[JSValue valueWith{0}:{1} inContext:__jsContext]";

        protected const string MarshallObjectToJSValueFormat =
            "[MarshallingService marshallObjectToJSValue:{0} inContext:__jsContext withClassName:{1}]";

        protected static string GenerateArgumentsMarshallingToJS(IList<ParameterDeclaration> parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("JSValueRef __args[{0}]", parameters.Count);
            if (parameters.Count > 0)
            {
                sb.Append(" = {0}");
            }
            sb.AppendLine(";");

            for (int i = 0; i < parameters.Count; i++)
            {
                ParameterDeclaration param = parameters[i];

                sb.AppendFormat("            ");

                if (param.Type.IsObjCType())
                {
                    sb.AppendLine(MarshallJSValueToJSValueFormat, i,
                        string.Format(MarshallObjectToJSValueFormat, param.Name, GetObjcClassName(param.Type)));
                }
                else
                {
                    string toJSValueSuffix = param.Type.ToJSValueSelectorSuffix();
                    if (toJSValueSuffix.Length > 0)
                    {
                        sb.AppendLine(MarshallJSValueToJSValueFormat, i,
                            string.Format(MarshallPrimitiveToJSValueFormat, toJSValueSuffix, param.Name));
                    }
                    else if (param.Type.GetBaseRecordDeclaration() != null)
                    {
                        var declaration = param.Type.GetBaseRecordDeclaration();

                        sb.AppendLine("{0} *__copy_{1} = malloc(sizeof({0}));", declaration.GetFullName(), param.Name);
                        sb.AppendLine("*__copy_{0} = {0};", param.Name);

                        sb.AppendLine(
                            "{0} *__{1}Wrapper = [[[{0} alloc] initWithPointer:__copy_{1} inContext:__jsContext.JSGlobalContextRef] autorelease];",
                            declaration.GetWrapperName(), param.Name);
                        sb.AppendLine("__{0}Wrapper->_isInjected = false;", param.Name);
                        sb.AppendLine(MarshallJSValueToJSValueFormat, i,
                            string.Format("[JSValue valueWithObject:__{0}Wrapper inContext:__jsContext]", param.Name));
                    }
                    else if (param.Type.IsSelectorType())
                    {
                        sb.AppendLine(MarshallJSValueToJSValueFormat, i,
                            string.Format(MarshallObjectToJSValueFormat,
                                string.Format("NSStringFromSelector({0})", param.Name), GetObjcClassName(param.Type)));
                    }
                    else if (param.Type.IsClassType())
                    {
                        sb.AppendLine(MarshallJSValueToJSValueFormat, i,
                            string.Format(MarshallObjectToJSValueFormat,
                                string.Format("NSStringFromClass({0})", param.Name), GetObjcClassName(param.Type)));
                    }
                    else if (param.Type.IsProtocolType())
                    {
                        sb.AppendLine(MarshallJSValueToJSValueFormat, i,
                            string.Format(MarshallObjectToJSValueFormat,
                                string.Format("NSStringFromProtocol({0})", param.Name), GetObjcClassName(param.Type)));
                    }
                    else if (param.Type.AsPrimitivePointerType() != null)
                    {
                        sb.AppendLine(MarshallJSValueToJSValueFormat, i,
                            string.Format("TNSCreateBufferWithPointer({0}, @\"{1}\", __jsContext)", param.Name,
                                param.Type.GetPointerName()));
                    }
                    else if (param.Type.IsBigIntType())
                    {
                        var typeType = param.Type.AsBigIntType();
                        switch (typeType)
                        {
                            case PrimitiveTypeType.Long:
                            case PrimitiveTypeType.LongLong:
                                sb.AppendLine(MarshallJSValueToJSValueFormat, i,
                                    string.Format("TNSLongLongToJSValue({0}, __jsContext)", param.Name));
                                break;

                            case PrimitiveTypeType.ULong:
                            case PrimitiveTypeType.ULongLong:
                                sb.AppendLine(MarshallJSValueToJSValueFormat, i,
                                    string.Format("TNSULongLongToJSValue({0}, __jsContext)", param.Name));
                                break;

                            default:
                                throw new InvalidOperationException();
                        }
                    }
                    else if (param.Type.GetPointerToBaseRecordDeclaration() != null)
                    {
                        var declaration = param.Type.GetPointerToBaseRecordDeclaration();

                        if (declaration.IsAnonymousWithoutTypedef())
                        {
                            throw new ArgumentException("Anonymous struct: " + declaration.GetFullName());
                        }

                        sb.AppendLine(MarshallJSValueToJSValueFormat, i,
                            string.Format("TNSCreateBufferWithPointer({0}, @\"{1}\", __jsContext)", param.Name,
                                declaration.GetWrapperName()));
                    }
                    else
                    {
                        sb.AppendLine("/*, Missing marshalling*/");
                    }
                }
            }

            return sb.ToString().TrimEnd();
        }

        private const bool UseJSValue = false;

        protected static string ConvertToJSType(TypeDefinition type)
        {
            if ((type is DeclarationReferenceType) && ((type as DeclarationReferenceType).Target is TypedefDeclaration) &&
                ((type as DeclarationReferenceType).Target as TypedefDeclaration).Name == "BOOL")
            {
                return "_Bool";
            }

            if (type.IsBigIntType())
            {
                return "JSValue*";
            }

            return UseJSValue || !type.IsPrimitive() ? "JSValue*" : type.ResolvePrimitive().ToString();
        }

        protected static string GenerateMethodSignatureFormat(MethodDeclaration method, IList<string> parameterNames,
            string methodNamePrefix)
        {
            StringBuilder sb = new StringBuilder("{0}").Append(methodNamePrefix);

            string[] nameTokens = method.Name.Split(new[] {":"}, StringSplitOptions.None);

            if (nameTokens.Length == 1)
            {
                return sb.Append(nameTokens[0]).ToString();
            }

            if (parameterNames.Count != nameTokens.Length - 1 || nameTokens[nameTokens.Length - 1] != string.Empty)
            {
                throw new Exception("Invalid method name");
            }

            for (int i = 0; i < nameTokens.Length - 1; i++)
            {
                if (i > 0)
                {
                    sb.Append(' ');
                }

                sb.AppendFormat("{0}:{1}{2}", nameTokens[i], "{" + (i + 1).ToString() + "}", parameterNames[i]);
            }

            return sb.ToString();
        }

        public static string GetMessage(MethodDeclaration method, IList<string> arguments, string methodPrefix = "")
        {
            Debug.Assert(method.Parameters.Count == arguments.Count);

            string messageFormat = GenerateMethodSignatureFormat(method, arguments, string.Empty);

            int argsCount = method.Parameters.Count;

            string[] types = Enumerable.Repeat(string.Empty, argsCount + 1).ToArray();
            types[0] = methodPrefix;
            return string.Format(messageFormat, types);
        }

        public static string GetMethodSignature(MethodDeclaration method, bool isJsType, string methodPrefix = "")
        {
            string messageFormat = GenerateMethodSignatureFormat(method,
                method.Parameters.Select(param => param.Name).ToArray(), methodPrefix);

            int argsCount = method.Parameters.Count;

            string[] types = Enumerable.Repeat(string.Empty, argsCount + 1).ToArray();

            Func<TypeDefinition, string> typeFormatter = x => isJsType ? ConvertToJSType(x) : x.ToString();

            types[0] = "(" + typeFormatter(method.ReturnType) + ")";
            for (int i = 0; i < argsCount; i++)
            {
                types[i + 1] = "(" + typeFormatter(method.Parameters[i].Type) + ")";
            }

            return string.Format(messageFormat, types);
        }

        protected string GenerateProtocolHeaderMethodSignature(MethodDeclaration method)
        {
            return GetMethodSignature(method, false);
        }

        protected string GenerateJSExportHeaderMethodSignature(MethodDeclaration method, string methodPrefix = "")
        {
            return (method.IsStatic ? "+" : "-") + GetMethodSignature(method, true, methodPrefix);
        }

        protected void GenerateBlockForFunction(IFormatter formatter, FunctionDeclaration function,
            string returnVarName = "__result", bool isBlock = false)
        {
            formatter.WriteLine("^({0}) {{",
                string.Join(", ", function.Parameters.Select(x => ConvertToJSType(x.Type) + " " + x.Name)));
            formatter.Indent();

            formatter.WriteLine(DebugCLog);

            formatter.WriteLine("JSContext *__jsContext = [JSContext currentContext];");

            formatter.WriteLine("@try {");
            formatter.Indent();

            formatter.WriteLine(GenerateParametersMarshallingFromJS(function.Parameters));

            TypeDefinition returnType = function.ReturnType.ResolveWithEnums();

            if (function.IsVariadic)
            {
                if (isBlock)
                {
                    formatter.WriteLine(this.GenerateVariadicBlockCall(function, returnVarName));
                }
                else
                {
                    formatter.WriteLine(this.GenerateVariadicFunctionCall(function, returnVarName));
                }
            }
            else
            {
                if (!returnType.IsVoid())
                {
                    formatter.Write("{0} = ", function.ReturnType.ToStringInternal(returnVarName));
                }
                formatter.WriteLine("{0}({1});", function.Name,
                    string.Join(", ", function.Parameters.Select(x => "marshalled_" + x.Name)));
            }

            foreach (
                var param in
                    function.Parameters.Where(
                        p =>
                            p.Type is PointerType && ((PointerType) p.Type).Target.IsObjCType() &&
                            !(((PointerType) p.Type).Target is IdType)))
            {
                formatter.WriteLine("[refvalue_{0} retainPtr];", param.Name);
            }

            if (!returnType.IsVoid())
            {
                formatter.WriteLine(MarshallResultToJSValue(function, null, true, returnVarName));
            }

            formatter.Outdent();
            formatter.WriteLine("}");

            formatter.WriteLine("@catch (NSException *__exception) {");
            formatter.Indent();
            formatter.WriteLine("[ObjCInheritance handleException:__exception inContext:__jsContext];");
            formatter.Outdent();
            formatter.WriteLine("}");

            formatter.Outdent();
            formatter.Write("}");
        }

        protected string GenerateParametersMarshallingFromJS(IList<ParameterDeclaration> parameters)
        {
            if (parameters.Count == 0)
            {
                return "// No arguments";
            }

            StringBuilder sb = new StringBuilder();
            foreach (ParameterDeclaration param in parameters)
            {
                sb.Append(MarshallParameterFromJS(param, param.Name, isJsValueRef: false)).Append('\t');
            }

            return sb.ToString().TrimEnd();
        }

        protected string MarshallParametersFromJSValueRef(IList<ParameterDeclaration> parameters)
        {
            var sb = new StringBuilder();

            sb.AppendLine(
                "if(argumentCount != {0}) {{ [NSException raise: @\"Invalid parameter count\" format: @\"Invalid number of parameters passed to a function.\" ]; }};",
                parameters.Count);
            sb.AppendLine();

            for (int i = 0; i < parameters.Count; i++)
            {
                string name = string.Format("arguments[{0}]", i);

                if (parameters[i].Type.AsBlock() != null)
                {
                    name = string.Format("{0}_copy", parameters[i].Name);
                    sb.AppendLine(
                        "__block JSValue *{0} = [JSValue valueWithJSValueRef:arguments[{1}] inContext:__jsContext];",
                        name, i);
                    sb.Append(MarshallParameterFromJS(parameters[i], name, isJsValueRef: false));
                }
                else
                {
                    sb.Append(MarshallParameterFromJS(parameters[i], name, isJsValueRef: true));
                }
            }

            return sb.ToString().TrimEnd();
        }

        private string MarshallParameterFromJS(ParameterDeclaration param, string name, bool isJsValueRef)
        {
            var sb = new StringBuilder();

            var type = param.Type.Resolve();

            var jsValue = name;
            var jsValueRef = name;
            if (isJsValueRef)
            {
                jsValue = string.Format("[JSValue valueWithJSValueRef:{0} inContext:__jsContext]", name);
            }
            else
            {
                jsValueRef = string.Format("{0}.JSValueRef", name);
            }

            if (type is PointerType && ((PointerType) type).Target.IsObjCType() &&
                !(((PointerType) type).Target is IdType))
            {
                sb.AppendLine("TNSRefValue *refvalue_{0} = [{1} toObjectOfClass:[TNSRefValue class]];", param.Name,
                    jsValue);
                sb.AppendLine("refvalue_{0}.className = @\"{1}\";", param.Name,
                    ((PointerType) type).Target.ToObjcType().Name);
            }

            sb.Append(TypeToSafeString(param.Type, "marshalled_" + param.Name));

            if (!param.Type.IsBigIntType() && (param.Type.IsPrimitive() || param.Type.IsObjCBOOL()))
            {
                if (isJsValueRef)
                {
                    if (param.Type.IsPrimitiveBoolean() || param.Type.IsObjCBOOL())
                    {
                        sb.AppendFormat(" = JSValueToBoolean(__jsContext.JSGlobalContextRef, {0})", name);
                    }
                    else
                    {
                        sb.AppendFormat(" = JSValueToNumber(__jsContext.JSGlobalContextRef, {0}, NULL)", name);
                    }
                }
                else
                {
                    sb.AppendFormat(" = {0}", param.Name);
                }
            }
            else if (param.Type.IsObjCType())
            {
                sb.AppendFormat(" = [MarshallingService marshallJSValueRefToObject:{0} inContext:__jsContext]",
                    jsValueRef);
            }
            else if (param.Type.GetBaseRecordDeclaration() != null)
            {
                var declaration = param.Type.GetBaseRecordDeclaration();
                sb.AppendFormat(" = *((({1} *)[{0} toObjectOfClass:[{1} class]])->_ptr)", jsValue,
                    declaration.GetWrapperName());
            }
            else if (type.AsBlock() != null)
            {
                using (StringWriter writer = new StringWriter())
                {
                    IFormatter formatter = new Formatter(writer);
                    MarshallBlockArgument(formatter, type.AsBlock(), jsValue);
                    sb.AppendFormat(" = {0}", formatter.ToString());
                }
            }
            else if (type.AsFunctionPointer() != null && !type.AsFunctionPointer().IsVariadic)
            {
                sb.Append(" = " + MarshallFunctionPointerArgument(type.AsFunctionPointer(), jsValue));
            }
            else if (param.Type.IsSelectorType())
            {
                sb.AppendFormat(" = NSSelectorFromString([{0} toString])", jsValue);
            }
            else if (param.Type.IsClassType())
            {
                sb.AppendFormat(" = NSClassFromString([{0} toString])", jsValue);
            }
            else if (param.Type.IsProtocolType())
            {
                sb.AppendFormat(" = NSProtocolFromString([{0} toString])", jsValue);
            }
            else if (param.Type.IsCFType())
            {
                sb.AppendFormat(" = ({0})[MarshallingService marshallJSValueRefToObject:{1} inContext:__jsContext]",
                    (param.Type as DeclarationReferenceType).Target.Name, jsValueRef);
            }
            else if (type is PointerType && ((PointerType) type).Target.IsObjCType() &&
                     !(((PointerType) type).Target is IdType))
            {
                sb.AppendFormat(" = refvalue_{0} != nil ? &refvalue_{0}->ptr : NULL", param.Name);
            }
            else if (type is PointerType || type.AsPrimitivePointerType() != null)
            {
                sb.AppendFormat(" = TNSGetPointerFromBuffer((JSObjectRef){0}, __jsContext)", jsValueRef);
            }
            else if (param.Type.IsBigIntType())
            {
                var typeType = param.Type.AsBigIntType();
                switch (typeType)
                {
                    case PrimitiveTypeType.Long:
                    case PrimitiveTypeType.LongLong:
                        sb.AppendFormat(" = ({0})TNSJSValueToLongLong({1})", typeType.ToCTypeString(), jsValue);
                        break;

                    case PrimitiveTypeType.ULong:
                    case PrimitiveTypeType.ULongLong:
                        sb.AppendFormat(" = ({0})TNSJSValueToULongLong({1})", typeType.ToCTypeString(), jsValue);

                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
            else
            {
                sb.Append("/*Missing marshalling*/");
            }
            sb.AppendLine(";");

            return sb.ToString();
        }

        private string MarshallFunctionPointerArgument(FunctionPointerType functionPointer, string jsValue)
        {
            using (var output = new StringWriter())
            {
                var formatter = new Formatter(output);

                formatter.Write("TNSCreateClosure(__getcif_function_proto_{0}(), {1}, &__function_proto_{0}, NULL)",
                    functionPointer.Id, jsValue);

                return formatter.ToString();
            }
        }

        protected string MarshallResultToJSValue(FunctionDeclaration function, InterfaceDeclaration @interface = null,
            bool marshallPrimitiveTypes = true, string resultVarName = "__result")
        {
            var returnType = function.ReturnType;
            var resultSb = new StringBuilder();

            if (returnType.IsObjCBOOL())
            {
                resultSb.AppendFormat("return [JSValue valueWithBool:{0} inContext:__jsContext];", resultVarName);
            }
            else if (!returnType.IsBigIntType() && returnType.IsPrimitive())
            {
                resultSb.AppendFormat("return {0};", resultVarName);
            }
            else if (returnType.GetBaseRecordDeclaration() != null)
            {
                var declaration = returnType.GetBaseRecordDeclaration();

                resultSb.AppendLine("{0} *copy = malloc(sizeof({0}));", declaration.GetFullName());
                resultSb.AppendFormat("*copy = {0};", resultVarName).AppendLine();
                resultSb.AppendLine(
                    "{0} *{1}Wrapper = [[[{0} alloc] initWithPointer:copy inContext:" + JSContextRef + "] autorelease];",
                    declaration.GetWrapperName(), resultVarName);
                resultSb.AppendFormat("{0}Wrapper->_isInjected = false;", resultVarName).AppendLine();
                resultSb.AppendFormat(
                    "JSValue *{0}Value = [JSValue valueWithObject:__resultWrapper inContext:__jsContext];",
                    resultVarName).AppendLine();
                resultSb.AppendFormat("return {0}Value;", resultVarName);
            }
            else if (returnType.IsSelectorType())
            {
                resultSb.AppendFormat(
                    "return [JSValue valueWithObject:NSStringFromSelector({0}) inContext:__jsContext];", resultVarName);
            }
            else if (returnType.IsClassType())
            {
                resultSb.AppendFormat("return [JSValue valueWithObject:NSStringFromClass({0}) inContext:__jsContext];",
                    resultVarName);
            }
            else if (returnType.IsProtocolType())
            {
                resultSb.AppendFormat(
                    "return [JSValue valueWithObject:NSStringFromProtocol({0}) inContext:__jsContext];", resultVarName);
            }
            else if (returnType.IsObjCType())
            {
                string marshallServiceCall = MarshallObjectToJSValueFormat;
                if (!marshallPrimitiveTypes)
                {
                    marshallServiceCall = marshallServiceCall.Insert(marshallServiceCall.Length - 1,
                        " withPrimitiveTypeMarshalling:false");
                }
                if (returnType is InstanceType && @interface != null)
                {
                    returnType = InstanceType.ToReference(@interface);
                }
                resultSb.AppendFormat("return {0};",
                    string.Format(marshallServiceCall, resultVarName, GetObjcClassName(returnType)));
            }
            else if (returnType.IsCFType())
            {
                resultSb.AppendFormat("id {0}AsId = (id){0};", resultVarName).AppendLine();

                if (function.Name.Contains("Create") || function.Name.Contains("Copy"))
                {
                    resultSb.AppendFormat("{0}AsId = [{0}AsId autorelease];", resultVarName).AppendLine();
                }

                resultSb.AppendFormat("return {0};",
                    string.Format(MarshallObjectToJSValueFormat, resultVarName + "AsId", "nil"));
            }
            else if (returnType.AsPrimitivePointerType() != null)
            {
                resultSb.AppendFormat("return TNSCreateBufferWithPointer({0}, @\"{1}\", __jsContext);", resultVarName,
                    returnType.GetPointerName());
            }
            else if (returnType.GetPointerToBaseRecordDeclaration() != null)
            {
                var declaration = returnType.GetPointerToBaseRecordDeclaration();

                if (declaration.IsAnonymousWithoutTypedef())
                {
                    throw new ArgumentException("Anonymous struct: " + declaration.GetFullName());
                }

                resultSb.AppendFormat("return TNSCreateBufferWithPointer({0}, @\"{1}\", __jsContext);", resultVarName,
                    declaration.GetWrapperName());
            }
            else if (returnType.IsBigIntType())
            {
                var typeType = returnType.AsBigIntType();
                switch (typeType)
                {
                    case PrimitiveTypeType.Long:
                    case PrimitiveTypeType.LongLong:
                        resultSb.AppendFormat("return TNSLongLongToJSValue({0}, __jsContext);", resultVarName);
                        break;

                    case PrimitiveTypeType.ULong:
                    case PrimitiveTypeType.ULongLong:
                        resultSb.AppendFormat("return TNSULongLongToJSValue({0}, __jsContext);", resultVarName);
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
            else if (returnType.AsBlock() != null)
            {
                // Get the function declaration
                FunctionPointerType block = returnType.AsBlock();
                FunctionDeclaration funcDecl = new FunctionDeclaration(resultVarName, block.ReturnType);
                funcDecl.IsVariadic = block.IsVariadic;
                funcDecl.Parameters.AddRange(block.Parameters);

                using (StringWriter writer = new StringWriter())
                {
                    IFormatter formatter = new Formatter(writer);
                    GenerateBlockForFunction(formatter, funcDecl, "__block" + resultVarName, true);
                    resultSb.AppendFormat("return [JSValue valueWithObject: {0} inContext: __jsContext];",
                        formatter.ToString());
                }
            }
            else
            {
                resultSb.AppendLine("//missing return");
                resultSb.Append("return NULL;");
            }

            return resultSb.ToString();
        }

        protected string MarshallResultToJSValueRef(FunctionDeclaration function, InterfaceDeclaration @interface = null,
            bool marshallPrimitiveTypes = true)
        {
            if (function.ReturnType.IsPrimitiveBoolean() || function.ReturnType.IsObjCBOOL())
            {
                return "return JSValueMakeBoolean(ctx, __result);";
            }

            if (!function.ReturnType.IsBigIntType() && function.ReturnType.IsPrimitive())
            {
                return "return JSValueMakeNumber(ctx, __result);";
            }

            if (MarshallResultToJSValue(function, @interface, marshallPrimitiveTypes).EndsWith("NULL;"))
            {
                return "return NULL;";
            }

            return MarshallResultToJSValue(function, @interface, marshallPrimitiveTypes).TrimEnd(';') + ".JSValueRef;";
        }

        protected static string MarshallResultFromJSValueRef(TypeDefinition returnType)
        {
            string format = "({0})({1});";
            string jsValue = "[JSValue valueWithJSValueRef:__result inContext:__jsContext]";

            if (!returnType.IsBigIntType() && returnType.IsPrimitive())
            {
                string primitive = string.Format("[{0} to{1}]", jsValue, returnType.FromJSValueSelectorSuffix());
                return string.Format(format, returnType.ToString(), primitive);
            }

            string wrapper =
                "(({0} *)[[JSValue valueWithJSValueRef:__result inContext:__jsContext] toObjectOfClass:[{0} class]])->_ptr";

            if (returnType.GetBaseRecordDeclaration() != null)
            {
                var declaration = returnType.GetBaseRecordDeclaration();
                var value = "*" + string.Format(wrapper, declaration.GetWrapperName());
                // TODO: Autorelease
                return string.Format(format, declaration.GetFullName(), value);
            }

            if (returnType.IsSelectorType())
            {
                return string.Format("NSSelectorFromString([{0} toString]);", jsValue);
            }
            if (returnType.IsClassType())
            {
                return string.Format("NSClassFromString([{0} toString]);", jsValue);
            }
            if (returnType.IsProtocolType())
            {
                return string.Format("NSProtocolFromString([{0} toString]);", jsValue);
            }

            if (returnType.AsPrimitivePointerType() != null)
            {
                return string.Format("TNSGetPointerFromBuffer((JSObjectRef){0}, __jsContext);", "__result");
            }

            if (returnType.GetPointerToBaseRecordDeclaration() != null)
            {
                return string.Format("TNSGetPointerFromBuffer((JSObjectRef){0}, __jsContext);", "__result");
            }


            if (returnType.IsBigIntType())
            {
                var typeType = returnType.AsBigIntType();
                switch (typeType)
                {
                    case PrimitiveTypeType.Long:
                    case PrimitiveTypeType.LongLong:
                        return string.Format("({0})TNSJSValueToLongLong({1});", typeType.ToCTypeString(),
                            "[JSValue valueWithJSValueRef:__result inContext:__jsContext]");

                    case PrimitiveTypeType.ULong:
                    case PrimitiveTypeType.ULongLong:
                        return string.Format("({0})TNSJSValueToULongLong({1});", typeType.ToCTypeString(),
                            "[JSValue valueWithJSValueRef:__result inContext:__jsContext]");

                    default:
                        throw new InvalidOperationException();
                }
            }

            string @object =
                string.Format("[MarshallingService marshallJSValueRefToObject:[{0} JSValueRef] inContext:__jsContext]",
                    jsValue);
            return string.Format(format, returnType.ToString(), @object);
        }

        protected static string GetObjcClassName(TypeDefinition type)
        {
            type = type.Resolve();
            return (type is IdType || type.IsSelectorType() || type.IsClassType() || type.IsProtocolType() ||
                    type is InstanceType)
                ? "nil"
                : string.Format("@\"{0}\"", type.ToObjcType().Name);
        }

        protected string GenerateJavaScriptMethodName(string selector)
        {
            string[] methodNameTokens = selector.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder result = new StringBuilder(methodNameTokens[0]);
            for (int i = 1; i < methodNameTokens.Length; i++)
            {
                result.Append(Char.ToUpper(methodNameTokens[i][0]));
                result.Append(methodNameTokens[i].Substring(1));
            }

            return result.ToString();
        }

        protected string GenerateVariadicFunctionCall(FunctionDeclaration function, string returnVarName = "__result")
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("// >>> begin variadic call");
            int fixedParametersCount = function.Parameters.Count - 1;
            output.AppendLine("int __fixedArgumentsCount = {0};", fixedParametersCount);
            output.AppendLine("int __totalArgumentsCount = __fixedArgumentsCount + [marshalled___varArgs count];");
            output.AppendLine(GenerateFfiType(function.ReturnType, "__ffiReturnType"));
            output.AppendLine("ffi_type **__argTypes = malloc(__totalArgumentsCount * sizeof(void *));");

            for (int index = 0; index < fixedParametersCount; index++)
            {
                ParameterDeclaration current = function.Parameters[index];
                string varName = String.Format("__argTypes[{0}]", index);
                output.AppendLine(GenerateFfiType(current.Type, varName, true));
            }

            output.AppendLine();
            output.AppendLine("void **__argValues = malloc(__totalArgumentsCount * sizeof(void *));");

            for (int index = 0; index < fixedParametersCount; index++)
            {
                ParameterDeclaration current = function.Parameters[index];
                output.AppendLine("__argValues[{0}] = &{1};", index, "marshalled_" + current.Name);
            }

            output.AppendLine();
            if (function.ReturnType.IsVoid())
            {
                output.AppendFormat("void *{0} = NULL;", returnVarName).AppendLine();
            }
            else
            {
                output.AppendLine("{0} = ({1})0;", function.ReturnType.ToStringInternal(returnVarName),
                    function.ReturnType.ToStringInternal(string.Empty));
            }

            // void callVariadicFunction(ffi_type* returnType, void *returnValuePtr, ffi_type **argTypes, void **argValues, NSArray *varArgs, int fixedCount, int totalCount);
            output.AppendLine(
                "callVariadicFunction({0}, __ffiReturnType, &{1}, __argTypes, __argValues, marshalled___varArgs, __fixedArgumentsCount, __totalArgumentsCount);",
                function.Name, returnVarName);
            output.AppendLine("free(__argTypes);");
            output.AppendLine("free(__argValues);");
            output.AppendLine("// <<< end variadic call");
            return output.ToString();
        }

        protected string GenerateVariadicBlockCall(FunctionDeclaration function, string returnVarName = "__result")
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("// >>> begin variadic call");
            int fixedParametersCount = function.Parameters.Count;
            output.AppendLine("int __fixedArgumentsCount = {0};", fixedParametersCount);
            output.AppendLine("int __totalArgumentsCount = __fixedArgumentsCount + [marshalled___varArgs count];");
            output.AppendLine(GenerateFfiType(function.ReturnType, "__ffiReturnType"));
            output.AppendLine("ffi_type **__argTypes = malloc(__totalArgumentsCount * sizeof(void *));");
            output.AppendLine("__argTypes[0] = &ffi_type_pointer;");

            for (int index = 0; index < fixedParametersCount; index++)
            {
                ParameterDeclaration current = function.Parameters[index];
                string varName = String.Format("__argTypes[{0}]", index + 1);
                output.AppendLine(GenerateFfiType(current.Type, varName, true));
            }

            output.AppendLine();
            output.AppendLine("void **__argValues = malloc(__totalArgumentsCount * sizeof(void *));");
            output.AppendLine("__argValues[0] = &{0};", function.Name);

            for (int index = 0; index < fixedParametersCount; index++)
            {
                ParameterDeclaration current = function.Parameters[index];
                output.AppendLine("__argValues[{0}] = &{1};", index + 1, "marshalled_" + current.Name);
            }

            output.AppendLine();
            if (function.ReturnType.IsVoid())
            {
                output.AppendFormat("void *{0} = NULL;", returnVarName).AppendLine();
            }
            else
            {
                output.AppendLine("{0} = ({1})0;", function.ReturnType.ToStringInternal(returnVarName),
                    function.ReturnType.ToStringInternal(string.Empty));
            }


            output.AppendLine(
                "struct Block_layout { void *isa; int flags; int reserved;  void (*invoke)(void *, ...); struct Block_descriptor *descriptor; };");
            output.AppendLine(
                "struct Block_layout *__block__layout__struct = (struct Block_layout *)(__bridge void *){0};",
                function.Name);
            function.Name = "__block__layout__struct -> invoke";
            // void callVariadicFunction(ffi_type* returnType, void *returnValuePtr, ffi_type **argTypes, void **argValues, NSArray *varArgs, int fixedCount, int totalCount);
            output.AppendLine(
                "callVariadicFunction({0}, __ffiReturnType, &{1}, __argTypes, __argValues, marshalled___varArgs, __fixedArgumentsCount, __totalArgumentsCount);",
                function.Name, returnVarName);
            output.AppendLine("free(__argTypes);");
            output.AppendLine("free(__argValues);");
            output.AppendLine("// <<< end variadic call");
            return output.ToString();
        }

        protected string GenerateVariadicJSDerivedCall(MethodDeclaration method, bool marshall = false)
        {
            string prefix = (marshall) ? "marshalled_" : string.Empty;
            string reciever = (marshall) ? "__reciever" : "self";
            string resultParam = (marshall) ? "&__result" : "&__return_value";
            StringBuilder output = new StringBuilder();
            output.AppendLine("\n// >>> begin variadic call");
            int fixedParametersCount = method.Parameters.Count + 1;
            output.AppendLine("int __fixedArgumentsCount = {0};", fixedParametersCount);
            output.AppendLine("int __totalArgumentsCount = __fixedArgumentsCount + [{0}__varArgs count];", prefix);
            output.AppendLine(GenerateFfiType(method.ReturnType, "__ffiReturnType"));
            output.AppendLine();
            output.AppendLine("ffi_type **__argTypes = malloc(__totalArgumentsCount * sizeof(void *));");

            output.AppendLine("__argTypes[0] = &ffi_type_pointer;");
            output.AppendLine("__argTypes[1] = &ffi_type_pointer;");
            for (int index = 0; index < fixedParametersCount - 2; index++)
            {
                ParameterDeclaration current = method.Parameters[index];
                string varName = String.Format("__argTypes[{0}]", index + 2);
                output.AppendLine(GenerateFfiType(current.Type, varName, true));
            }

            output.AppendLine();
            output.AppendLine("void **__argValues = malloc(__totalArgumentsCount * sizeof(void *));");

            output.AppendLine("__argValues[0] = &{0};", reciever);
            output.AppendLine("__argValues[1] = &__variadic_selector;");
            for (int index = 0; index < fixedParametersCount - 2; index++)
            {
                ParameterDeclaration current = method.Parameters[index];
                output.AppendLine("__argValues[{0}] = &{1};", index + 2, prefix + current.Name);
            }

            output.AppendLine();
            resultParam = (method.ReturnType.IsVoid()) ? "NULL" : resultParam;
            // void callVariadicFunction(ffi_type* returnType, void *returnValuePtr, ffi_type **argTypes, void **argValues, NSArray *varArgs, int fixedCount, int totalCount);
            output.AppendLine(
                "callVariadicFunction(__functionPtr, __ffiReturnType, {0}, __argTypes, __argValues, {1}__varArgs, __fixedArgumentsCount, __totalArgumentsCount);",
                resultParam, prefix);
            output.AppendLine("free(__argTypes);");
            output.AppendLine("free(__argValues);");
            output.AppendLine("// <<< end variadic call");
            return output.ToString();
        }

        protected string GenerateVariadicJSExposedCall(MethodDeclaration method)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("\n// >>> begin variadic call");
            int fixedParametersCount = method.Parameters.Count + 1;
            output.AppendLine("int __fixedArgumentsCount = {0};", fixedParametersCount);
            output.AppendLine("int __totalArgumentsCount = __fixedArgumentsCount + [__varArgs count];");
            output.AppendLine(GenerateFfiType(method.ReturnType, "__ffiReturnType"));
            output.AppendLine();
            output.AppendLine("ffi_type **__argTypes = malloc(__totalArgumentsCount * sizeof(void *));");

            output.AppendLine("__argTypes[0] = &ffi_type_pointer;");
            output.AppendLine("__argTypes[1] = &ffi_type_pointer;");
            for (int index = 0; index < fixedParametersCount - 2; index++)
            {
                ParameterDeclaration current = method.Parameters[index];
                string varName = string.Format("__argTypes[{0}]", index + 2);
                output.AppendLine(GenerateFfiType(current.Type, varName, true));
            }

            output.AppendLine();
            output.AppendLine("void **__argValues = malloc(__totalArgumentsCount * sizeof(void *));");

            output.AppendLine("__argValues[0] = &(self -> _tns_instance);");
            output.AppendLine("__argValues[1] = &__variadic_selector;");
            for (int index = 0; index < fixedParametersCount - 2; index++)
            {
                ParameterDeclaration current = method.Parameters[index];
                output.AppendLine("__argValues[{0}] = &{1};", index + 2, current.Name);
            }

            output.AppendLine();
            string resultParam = (method.ReturnType.IsVoid()) ? "NULL" : "&__return_value";
            // void callVariadicFunction(ffi_type* returnType, void *returnValuePtr, ffi_type **argTypes, void **argValues, NSArray *varArgs, int fixedCount, int totalCount);
            output.AppendLine(
                "callVariadicFunction(__functionPtr, __ffiReturnType, {0}, __argTypes, __argValues, __varArgs, __fixedArgumentsCount, __totalArgumentsCount);",
                resultParam);
            output.AppendLine("free(__argTypes);");
            output.AppendLine("free(__argValues);");
            output.AppendLine("// <<< end variadic call");
            return output.ToString();
        }

        protected string GenerateVariadicStaticCall(MethodDeclaration method, string reciever)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("\n// >>> begin variadic call");
            int fixedParametersCount = method.Parameters.Count + 1;
            output.AppendLine("int __fixedArgumentsCount = {0};", fixedParametersCount);
            output.AppendLine("int __totalArgumentsCount = __fixedArgumentsCount + [marshalled___varArgs count];");
            output.AppendLine(GenerateFfiType(method.ReturnType, "__ffiReturnType"));
            output.AppendLine();
            output.AppendLine("ffi_type **__argTypes = malloc(__totalArgumentsCount * sizeof(void *));");

            output.AppendLine("__argTypes[0] = &ffi_type_pointer;");
            output.AppendLine("__argTypes[1] = &ffi_type_pointer;");
            for (int index = 0; index < fixedParametersCount - 2; index++)
            {
                ParameterDeclaration current = method.Parameters[index];
                string varName = string.Format("__argTypes[{0}]", index + 2);
                output.AppendLine(GenerateFfiType(current.Type, varName, true));
            }

            output.AppendLine();
            output.AppendLine("void **__argValues = malloc(__totalArgumentsCount * sizeof(void *));");

            string methodName = (method.IsConstructor) ? "alloc" : "class";
            output.AppendLine("id __class_reciever = [{0} {1}];", reciever, methodName);
            output.AppendLine("__argValues[0] = &(__class_reciever);");
            output.AppendLine("__argValues[1] = &__variadic_selector;");
            for (int index = 0; index < fixedParametersCount - 2; index++)
            {
                ParameterDeclaration current = method.Parameters[index];
                output.AppendLine("__argValues[{0}] = &marshalled_{1};", index + 2, current.Name);
            }

            output.AppendLine();
            string resultParam = (method.ReturnType.IsVoid()) ? "NULL" : "&__result";
            // void callVariadicFunction(ffi_type* returnType, void *returnValuePtr, ffi_type **argTypes, void **argValues, NSArray *varArgs, int fixedCount, int totalCount);
            output.AppendLine(
                "callVariadicFunction(__functionPtr, __ffiReturnType, {0}, __argTypes, __argValues, marshalled___varArgs, __fixedArgumentsCount, __totalArgumentsCount);",
                resultParam);
            output.AppendLine("free(__argTypes);");
            output.AppendLine("free(__argValues);");
            output.AppendLine("// <<< end variadic call");
            return output.ToString();
        }

        protected string GetVariadicSelectorForMethod(MethodDeclaration method)
        {
            string name = method.Name;
            string[] tokens = name.Split(':');
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < tokens.Length - 2; i++)
            {
                result.Append(tokens[i] + ":");
            }
            return result.ToString();
        }

        protected string GenerateFfiType(TypeDefinition type, string varName, bool declared = false)
        {
            StringBuilder output = new StringBuilder();
            bool isStruct = (type.Resolve() is DeclarationReferenceType &&
                             ((DeclarationReferenceType) type.Resolve()).Target is BaseRecordDeclaration);
            string declaration = (declared) ? string.Empty : "ffi_type *";
            string ffiType = (isStruct) ? "malloc(sizeof(ffi_type))" : "&" + type.ToFfiType();
            output.AppendFormat("{0}{1} = {2};", declaration, varName, ffiType);
            output.AppendLine();
            if (isStruct)
            {
                BaseRecordDeclaration recordDeclaration =
                    (BaseRecordDeclaration) ((DeclarationReferenceType) type.Resolve()).Target;
                int fieldsCount = recordDeclaration.Fields.Count;
                output.AppendLine(String.Format(
                    "{0} -> size = 0; {0} -> alignment = 0; {0} -> type = FFI_TYPE_STRUCT;", varName));
                output.AppendLine(String.Format("{0} -> elements = malloc({1} * sizeof(ffi_type *));", varName,
                    fieldsCount + 1));
                for (int index = 0; index < fieldsCount; index++)
                {
                    TypeDefinition fieldType = recordDeclaration.Fields[index].Type;
                    output.AppendLine(GenerateFfiType(fieldType, String.Format("{0} -> elements[{1}]", varName, index),
                        true));
                }
                output.AppendFormat("{0} -> elements[{1}] = NULL;", varName, fieldsCount);
                output.AppendLine();
            }
            return output.ToString();
        }

        protected static string GetFunctionPointerBindings(IEnumerable<FunctionDeclaration> functions)
        {
            var functionPointerParameters = functions.SelectMany(x => x.Parameters.Select(y => y.Type))
                .Select(x => x.AsFunctionPointer())
                .Where(x => x != null);

            return GetFunctionPointerBindings(functionPointerParameters);
        }

        protected static string GetFunctionPointerBindings(IEnumerable<FunctionPointerType> functionPointers)
        {
            functionPointers = functionPointers.DistinctBy(x => x.Id);

            var sb = new StringBuilder();

            foreach (var functionPointer in functionPointers)
            {
                sb.AppendLine(GenerateFunctionPointerBinding(functionPointer));
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        protected static string GenerateFunctionPointerBinding(FunctionPointerType functionPointer)
        {
            using (var writer = new StringWriter())
            {
                var formatter = new Formatter(writer);

                var returnType = functionPointer.ReturnType;
                var returnTypePointer = new PointerType(returnType);

                formatter.WriteLine("// " + functionPointer.ToString());

                // CIF
                {
                    formatter.WriteLine("static ffi_cif *__getcif_function_proto_{0}() {{", functionPointer.Id);
                    formatter.Indent();

                    formatter.WriteLine("static ffi_cif *cif = NULL;");

                    formatter.WriteLine("if (cif == NULL) {");
                    formatter.Indent();

                    formatter.WriteLine("cif = malloc(sizeof(ffi_cif));");
                    formatter.WriteLine("static ffi_type *__args[] = {{ {0} }};",
                        string.Join(", ", functionPointer.Parameters.Select(x => "&" + x.Type.ToFfiType())));

                    formatter.WriteLine("if (ffi_prep_cif(cif, FFI_DEFAULT_ABI, {0}, &{1}, __args) != FFI_OK) {{",
                        functionPointer.Parameters.Count, returnType.ToFfiType());
                    formatter.Indent();
                    formatter.WriteLine(
                        "@throw [NSException exceptionWithName:NSGenericException reason:@\"Failed to prepare cif\" userInfo:nil];");
                    formatter.Outdent();
                    formatter.WriteLine("}");

                    formatter.Outdent();
                    formatter.WriteLine("}");

                    formatter.WriteLine("return cif;");

                    formatter.Outdent();
                    formatter.WriteLine("}");
                }

                // FUNCTION BINDING
                {
                    formatter.WriteLine(
                        "static void __function_proto_{0}(ffi_cif *__cif, void *__ret, void **__argsp, void *__user_data) {{",
                        functionPointer.Id);
                    formatter.Indent();

                    formatter.WriteLine("FFIJSClosure *jsClosure = __user_data;");
                    formatter.WriteLine(
                        "JSContext *__jsContext = [ObjCInheritance getJSContextFromJSContextRef:jsClosure->context];");
                    formatter.WriteLine();

                    if (!returnType.IsVoid())
                    {
                        formatter.WriteLine("*(({0})__ret) = {1};", returnTypePointer.ToString(),
                            returnType.GetDefaultValue());
                        formatter.WriteLine();
                    }

                    for (int i = 0; i < functionPointer.Parameters.Count; i++)
                    {
                        var param = functionPointer.Parameters[i];
                        formatter.WriteLine("{0} = *(({1})__argsp[{2}]);", TypeToSafeString(param.Type, param.Name),
                            new PointerType(param.Type).ToString(), i);
                    }
                    formatter.WriteLine();

                    formatter.WriteLine(GenerateArgumentsMarshallingToJS(functionPointer.Parameters));
                    formatter.WriteLine();

                    formatter.WriteLine("JSValueRef __exception = NULL;");

                    if (!returnType.IsVoid())
                    {
                        formatter.Write("JSValueRef __result = ");
                    }

                    formatter.WriteLine(
                        "JSObjectCallAsFunction(__jsContext.JSGlobalContextRef, jsClosure->function, NULL, {0}, __args, &__exception);",
                        functionPointer.Parameters.Count);
                    formatter.WriteLine("if (__exception) {");
                    formatter.Indent();
                    formatter.WriteLine(
                        "__jsContext.exceptionHandler(__jsContext, [JSValue valueWithJSValueRef:__exception inContext:__jsContext]);");
                    formatter.WriteLine("return;");
                    formatter.Outdent();
                    formatter.WriteLine("}");

                    if (!returnType.IsVoid())
                    {
                        formatter.WriteLine("{0} = {1}", returnType.ToString("__return_value"),
                            MarshallResultFromJSValueRef(returnType));
                        formatter.WriteLine("*(({0})__ret) = __return_value;", returnTypePointer.ToString());
                    }

                    formatter.Outdent();
                    formatter.WriteLine("}");
                }

                return writer.ToString();
            }
        }

        private void MarshallBlockArgument(IFormatter formatter, FunctionPointerType block, string jsValue)
        {
            formatter.WriteLine("^ ({0}) {{",
                string.Join(", ", block.Parameters.Select(x => x.Type.ToStringInternal(x.Name))));
            formatter.Indent();
            formatter.Indent();

            //formatter.WriteLine("JSContext *__jsContext = [JSContext currentContext];");
            formatter.WriteLine(GenerateArgumentsMarshallingToJS(block.Parameters));

            formatter.WriteLine("JSValueRef __exception = NULL;");

            if (!block.ReturnType.IsVoid())
            {
                formatter.Write("JSValueRef __result = ");
            }

            formatter.WriteLine(
                "JSObjectCallAsFunction(__jsContext.JSGlobalContextRef, {0}.JSValueRef, NULL, {1}, __args, &__exception);",
                jsValue, block.Parameters.Count);
            formatter.WriteLine(
                "if (__exception) __jsContext.exceptionHandler(__jsContext, [JSValue valueWithJSValueRef:__exception inContext:__jsContext]);");

            if (!block.ReturnType.IsVoid())
            {
                formatter.WriteLine("return " + MarshallResultFromJSValueRef(block.ReturnType));
            }

            formatter.Outdent();
            formatter.Write("}");
        }

        protected static string TypeToSafeString(TypeDefinition type, string identifier = "")
        {
            if (type.Resolve() is IncompleteArrayType)
            {
                return "void *" + identifier;
            }

            return type.ToString(identifier);
        }

        protected string GetPropertyName(MethodDeclaration method)
        {
            if (!method.IsImplicit)
            {
                return "nil";
            }

            var property =
                method.Parent.Properties.Single(
                    x => x.Getter.Name == method.Name || x.Setter != null && x.Setter.Name == method.Name);

            return string.Format("@\"{0}\"", property.Name);
        }

        protected TypeDefinition GetActualMethodReturnType(MethodDeclaration method,
            InterfaceDeclaration interfaceDeclaration)
        {
            if (method.ReturnType is InstanceType)
            {
                return InstanceType.ToReference(interfaceDeclaration);
            }
            return method.ReturnType;
        }

        protected string GetSuperMessage(MethodDeclaration method, InterfaceDeclaration interfaceDecl, string self,
            string paramPrefix = "", string selector = null, bool invokeIt = true)
        {
            // selector: if selector is null - the method name is used
            // invokeIt: if the method pointer to be invoked. The default is true.
            var result = new StringBuilder();

            TypeDefinition actualReturnType = GetActualMethodReturnType(method, interfaceDecl);
            var cast = string.Format("{0} ",
                actualReturnType.ToString(String.Format("(*)(id, SEL{0})",
                    string.Concat(method.Parameters.Select(x => ", " + x.Type.ToString())))));
            var func = actualReturnType.GetBaseRecordDeclaration() == null
                ? "class_getMethodImplementation"
                : "class_getMethodImplementation_stret";
            var cmd = string.Format("@selector({0})", selector ?? method.Name);
            var imp = string.Format("{0}(class_getSuperclass(class_getSuperclass(object_getClass({1}))), {2})", func,
                self, cmd);
            var args = string.Concat(method.Parameters.Select(x => ", " + paramPrefix + x.Name));

            result.AppendFormat("(({0}){1})", cast, imp);
            if (invokeIt)
            {
                result.AppendFormat("({0}, {1}{2})", self, cmd, args);
            }
            result.Append(";");

            return result.ToString();
        }

        protected string GetObjcMessage(MethodDeclaration method, InterfaceDeclaration interfaceDecl, string self,
            string paramPrefix = "")
        {
            var result = new StringBuilder();
            var actualReturnType = GetActualMethodReturnType(method, interfaceDecl);

            var cast = string.Format("({0})",
                actualReturnType.ToString(string.Format("(*)(id, SEL{0})",
                    string.Concat(method.Parameters.Select(x => ", " + x.Type.ToString())))));
            var func = actualReturnType.GetBaseRecordDeclaration() == null
                ? "class_getMethodImplementation"
                : "class_getMethodImplementation_stret";
            var imp = string.Format("{0}(object_getClass(__cls), __sel)", func);
            var args = string.Concat(method.Parameters.Select(x => ", " + paramPrefix + x.Name));

            result.AppendFormat("({0}{1})({2}, __sel{3})", cast, imp, self, args);

            return result.ToString();
        }
    }
}
