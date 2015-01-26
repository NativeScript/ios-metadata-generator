using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Libclang.Core.Ast;
using Libclang.Core.Common;
using Libclang.Core.Types;

namespace Libclang.Core.Generator
{
    public class TNSBridgeStructWriter : BaseTNSBridgeWriter
    {
        protected readonly IDictionary<BaseRecordDeclaration, Tuple<BaseRecordDeclaration, FieldDeclaration>>
            anonymousToParent =
                new Dictionary<BaseRecordDeclaration, Tuple<BaseRecordDeclaration, FieldDeclaration>>();

        protected readonly MultiDictionary<BaseRecordDeclaration, BaseRecordDeclaration> dependencies =
            new MultiDictionary<BaseRecordDeclaration, BaseRecordDeclaration>();

        protected readonly IEnumerable<DocumentDeclaration> frameworks;

        protected readonly ICollection<BaseRecordDeclaration> unnamedAnonymousRecordFields =
            new HashSet<BaseRecordDeclaration>();

        protected readonly ICollection<string> bannedNames = new HashSet<string>
        {
            "__mbstate_t",
            "struct _opaque_pthread_attr_t",
            "struct _opaque_pthread_cond_t",
            "struct _opaque_pthread_condattr_t",
            "struct _opaque_pthread_mutex_t",
            "struct _opaque_pthread_mutexattr_t",
            "struct _opaque_pthread_once_t",
            "struct _opaque_pthread_rwlock_t",
            "struct _opaque_pthread_rwlockattr_t",
            "struct _opaque_pthread_t",
            "struct fd_set",
            "struct objc_object"
        };

        public TNSBridgeStructWriter(IEnumerable<DocumentDeclaration> frameworks)
            : base(null)
        {
            this.frameworks = frameworks;
        }

        protected override string JSContext
        {
            get { return "[JSContext contextWithJSGlobalContextRef:" + JSContextRef + "]"; }
        }

        protected override string JSContextRef
        {
            get { return "self.tns_jscontext"; }
        }

        protected void GenerateAnonymousToParent()
        {
            BaseRecordDeclaration[] allRecords =
                frameworks.SelectMany(x => x.Declarations.OfType<BaseRecordDeclaration>()).ToArray();
            foreach (BaseRecordDeclaration currentRecord in allRecords.Where(x => x.IsAnonymousWithoutTypedef()))
            {
                bool found = false;

                foreach (BaseRecordDeclaration record in allRecords)
                {
                    if (found)
                    {
                        break;
                    }

                    foreach (FieldDeclaration field in record.Fields)
                    {
                        if (field.Type is DeclarationReferenceType &&
                            (field.Type as DeclarationReferenceType).Target.FullName == currentRecord.FullName ||
                            (field.Type is ConstantArrayType &&
                             (field.Type as ConstantArrayType).ElementType is DeclarationReferenceType &&
                             ((field.Type as ConstantArrayType).ElementType as DeclarationReferenceType).Target.FullName ==
                             currentRecord.FullName))
                        {
                            anonymousToParent[currentRecord] = Tuple.Create(record, field);
                            found = true;
                            break;

                            // goto found;
                        }
                    }
                }

                if (!found)
                {
                    //Debug.WriteLine("Anonymous field: " + currentRecord.Name);
                    unnamedAnonymousRecordFields.Add(currentRecord);
                }
            }
        }

        protected void GenerateDependencies(IEnumerable<BaseRecordDeclaration> recordDeclarations)
        {
            foreach (BaseRecordDeclaration record in recordDeclarations)
            {
                foreach (FieldDeclaration field in record.Fields)
                {
                    TypeDefinition type = field.Type.Resolve();

                    AddDependency(record, type);

                    if (type is FunctionPointerType)
                    {
                        foreach (var nestedType in ((FunctionPointerType) type).ReferedTypes)
                        {
                            AddDependency(record, nestedType);
                        }
                    }
                }
            }
        }

        protected void AddDependency(BaseRecordDeclaration record, TypeDefinition type)
        {
            if (type.GetBaseRecordDeclaration() != null)
            {
                var declaration = type.GetBaseRecordDeclaration();
                AddDependency(record, declaration);
            }
            else if (type is ConstantArrayType &&
                     ((type as ConstantArrayType).ElementType.Resolve() is DeclarationReferenceType) &&
                     ((type as ConstantArrayType).ElementType.GetBaseRecordDeclaration() != null))
            {
                var declaration = (type as ConstantArrayType).ElementType.GetBaseRecordDeclaration();
                AddDependency(record, declaration);
            }
        }

        protected void AddDependency(BaseRecordDeclaration record, BaseRecordDeclaration declaration)
        {
            if (dependencies.ContainsKey(record) && dependencies[record].Any(x => x.Name == declaration.Name))
            {
                return;
            }

            dependencies.Add(record, declaration);
        }

        public virtual void Generate(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            else
            {
                new DirectoryInfo(directory).Clear();
            }

            // TODO: Remove structs starting with '_'
            List<BaseRecordDeclaration> recordDeclarations =
                frameworks.SelectMany(x => x.Declarations.OfType<BaseRecordDeclaration>())
                    .Where(x => !x.IsOpaque)
                    .Where(x => !x.Name.StartsWith("__darwin_"))
                    //.Where(x => !bannedNames.Contains(x.GetFullName()))
                    .DistinctBy(x => x.GetFullName())
                    .OrderBy(x => x.GetFullName())
                    .ToList();

            GenerateAnonymousToParent();
            GenerateDependencies(recordDeclarations);

            foreach (BaseRecordDeclaration record in recordDeclarations)
            {
                if (unnamedAnonymousRecordFields.Contains(record))
                {
                    continue;
                }

                string fileName = record.GetFileName();

                using (var headerWriter = new StreamWriter(Path.Combine(directory, fileName + ".h")))
                using (var implementationWriter = new StreamWriter(Path.Combine(directory, fileName + ".m")))
                {
                    {
                        if (dependencies.ContainsKey(record))
                        {
                            foreach (BaseRecordDeclaration import in dependencies[record])
                            {
                                headerWriter.WriteLine("#import \"{0}\"", import.GetFileName() + ".h");
                            }
                        }

                        headerWriter.WriteLine(WriteProtocol(record));
                        headerWriter.WriteLine(WriteHeader(record));
                    }

                    {
                        implementationWriter.WriteLine("#import \"{0}\"", fileName + ".h");
                        implementationWriter.WriteLine();

                        implementationWriter.WriteLine(
                            GetFunctionPointerBindings(
                                record.Fields.Where(x => x.Type.AsFunctionPointer() != null)
                                    .Select(x => x.Type.AsFunctionPointer())));
                        implementationWriter.WriteLine();

                        implementationWriter.WriteLine(WriteImplementation(record));
                    }
                }
            }
        }

        private string WriteProtocol(BaseRecordDeclaration record)
        {
            var result = new StringBuilder();

            result.AppendLine("#import <TNSBridgeInfrastructure/MarshallingService.h>");
            result.AppendLine("#import <TNSBridgeInfrastructure/ObjCInheritance.h>");
            result.AppendLine("#import <TNSBridgeInfrastructure/PointerWrapperProtocol.h>");
            result.AppendLine("#import <TNSBridgeInfrastructure/Utils.h>");
            result.AppendLine("#import <TNSBridgeInfrastructure/TNSBuffer.h>");
            result.AppendLine("#import <TNSBridgeInfrastructure/BigIntWrapper.h>");
            result.AppendLine("#import <ffi.h>");

            result.AppendLine();

            result.AppendLine("@protocol {0} <JSExport>", GetProtocolName(record));
            result.AppendLine(WritePropertyList(record));
            result.AppendLine("+(instancetype)create;");
            result.AppendLine("@end");
            return result.ToString();
        }

        private string WriteHeader(BaseRecordDeclaration record)
        {
            var result = new StringBuilder();

            result.AppendLine("@interface {0} : NSObject <{1}, PointerWrapperProtocol> {{", record.GetWrapperName(),
                GetProtocolName(record));
            result.AppendLine("    @public");
            string type = GetType(record);
            result.AppendLine("    {0} *_ptr;", type);
            result.AppendLine("    bool _isInjected;");
            result.AppendLine("}");
            result.AppendLine("@property (nonatomic) JSContextRef tns_jscontext;");
            result.AppendLine("-(instancetype) initWithPointer:(void *) ptr inContext:(JSContextRef)context;");
            result.AppendLine("@end");
            return result.ToString();
        }

        private string WriteImplementation(BaseRecordDeclaration record)
        {
            var result = new StringBuilder();

            result.AppendLine("@implementation {0}", record.GetWrapperName());

            result.AppendLine("+ (size_t)__size {{ return sizeof({0}); }}", GetType(record));

            foreach (FieldDeclaration field in record.Fields.Where(IsValidField))
            {
                string jsType = ToJsType(field.Type);
                string pascalName = char.ToUpperInvariant(field.Name[0]) + field.Name.Substring(1);

                if (jsType.EndsWith("Wrapper*"))
                {
                    result.AppendLine("@synthesize {0} = _{0};", field.Name);
                    result.AppendLine("-({0}) {1} {{ return _{1}; }}", jsType, field.Name);
                    if (!field.Type.IsConst)
                    {
                        result.AppendLine("-(void) set{1}:({0})value {{ _ptr->{2} = *(value->_ptr); }}", jsType,
                            pascalName, field.Name);
                    }
                }
                else if (field.Type.Resolve() is ConstantArrayType)
                {
                    result.AppendLine("@synthesize {0} = _{0};", field.Name);
                }
                else if (field.Type.AsPrimitivePointerType() != null)
                {
                    result.AppendLine(
                        "-({0}) {1} {{ return TNSCreateBufferWithPointer(_ptr->{1}, @\"{2}\", [ObjCInheritance getJSContextFromJSContextRef:_tns_jscontext]); }}",
                        jsType, field.Name, field.Type.AsPrimitivePointerType().Value.ToCTypeString());
                    if (!field.Type.IsConst)
                    {
                        result.AppendLine(
                            "-(void) set{1}:({0})value {{ _ptr->{2} = JSObjectGetPrivate((JSObjectRef)value.JSValueRef); }}",
                            jsType, pascalName, field.Name);
                    }
                }
                else if (field.Type.AsFunctionPointer() != null)
                {
                    if (!field.Type.IsConst)
                    {
                        result.AppendLine(
                            "-(void) set{1}:({0})value {{ _ptr->{2} = TNSCreateClosure(__getcif_function_proto_{3}(), value, &__function_proto_{3}, NULL); }}",
                            jsType, pascalName, field.Name, field.Type.AsFunctionPointer().Id);
                    }
                }
                else if (field.Type.GetPointerToBaseRecordDeclaration() != null)
                {
                    result.AppendLine(
                        "-({0}) {1} {{ return TNSCreateBufferWithPointer(_ptr->{1}, @\"{2}\", [ObjCInheritance getJSContextFromJSContextRef:_tns_jscontext]); }}",
                        jsType, field.Name, field.Type.GetPointerToBaseRecordDeclaration().GetWrapperName());
                    if (!field.Type.IsConst)
                    {
                        result.AppendLine(
                            "-(void) set{1}:({0})value {{ _ptr->{2} = JSObjectGetPrivate((JSObjectRef)value.JSValueRef); }}",
                            jsType, pascalName, field.Name);
                    }
                }
                else if (field.Type.IsBigIntType())
                {
                    var typeType = field.Type.AsBigIntType();
                    switch (typeType)
                    {
                        case PrimitiveTypeType.Long:
                        case PrimitiveTypeType.LongLong:
                            result.AppendLine(
                                "-({0}) {1} {{ return TNSLongLongToJSValue(_ptr->{1}, [ObjCInheritance getJSContextFromJSContextRef:_tns_jscontext]); }}",
                                jsType, field.Name);
                            if (!field.Type.IsConst)
                            {
                                result.AppendLine(
                                    "-(void) set{1}:({0})value {{ _ptr->{2} = ({3})TNSJSValueToLongLong(value); }}",
                                    jsType, pascalName, field.Name, typeType.ToCTypeString());
                            }
                            break;

                        case PrimitiveTypeType.ULong:
                        case PrimitiveTypeType.ULongLong:
                            result.AppendLine(
                                "-({0}) {1} {{ return TNSULongLongToJSValue(_ptr->{1}, [ObjCInheritance getJSContextFromJSContextRef:_tns_jscontext]); }}",
                                jsType, field.Name);
                            if (!field.Type.IsConst)
                            {
                                result.AppendLine(
                                    "-(void) set{1}:({0})value {{ _ptr->{2} = ({3})TNSJSValueToULongLong(value); }}",
                                    jsType, pascalName, field.Name, typeType.ToCTypeString());
                            }
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                }
                else // Primitive
                {
                    result.AppendLine("-({0}) {1} {{ return _ptr->{1}; }}", jsType, field.Name);
                    if (!field.Type.IsConst)
                    {
                        result.AppendLine("-(void) set{1}:({0})value {{ _ptr->{2} = value; }}", jsType, pascalName,
                            field.Name);
                    }
                }
            }

            result.AppendLine("@synthesize tns_jscontext = _tns_jscontext;");

            bool hasArrayField = record.Fields.Any(x => x.Type.Resolve() is ConstantArrayType);

            string type = GetType(record);

            result.AppendLine(@"+(instancetype) create {{
    {2}
    {0} *instance = [[[{0} alloc] init] autorelease];
    instance->_ptr = malloc(sizeof({1}));
    instance.tns_jscontext = JSContext.currentContext.JSGlobalContextRef;
    memset(instance->_ptr, 0, sizeof({1}));", record.GetWrapperName(), type, DebugLog);

            IEnumerable<FieldDeclaration> nestedRecords = record.Fields.Where(x =>
                x.Type.Resolve() is DeclarationReferenceType &&
                (x.Type.Resolve() as DeclarationReferenceType).Target.Canonical is BaseRecordDeclaration);

            foreach (FieldDeclaration field in nestedRecords)
            {
                var declaration = (field.Type.Resolve() as DeclarationReferenceType).Target as BaseRecordDeclaration;

                result.AppendLine(
                    "    instance->_{0} = [[{1} alloc] initWithPointer: &((*(instance->_ptr)).{0}) inContext: [[JSContext currentContext] JSGlobalContextRef]];",
                    field.Name,
                    declaration.GetWrapperName());
            }
            if (hasArrayField)
            {
                result.AppendLine("    [{0} createArrayContainer:instance inContext:[JSContext currentContext]];",
                    record.GetWrapperName());
            }
            result.AppendLine("    return instance;");
            result.AppendLine("}");

            result.AppendLine("-(instancetype) initWithPointer:(void *)ptr inContext:(JSContextRef)context {");
            result.AppendLine(
                "    if (self = [self init]) { _ptr = ptr; _isInjected = true; _tns_jscontext = context; }");
            foreach (FieldDeclaration field in nestedRecords)
            {
                var declaration = (field.Type.Resolve() as DeclarationReferenceType).Target as BaseRecordDeclaration;

                result.AppendLine(
                    "    _{0} = [[{1} alloc] initWithPointer: &((*(({2} *)_ptr)).{0}) inContext:" + JSContextRef + "];",
                    field.Name,
                    declaration.GetWrapperName(), GetType(record));
            }
            if (hasArrayField)
            {
                result.AppendLine(
                    "    [{0} createArrayContainer:self inContext:[ObjCInheritance getJSContextFromJSContextRef:context]];",
                    record.GetWrapperName());
            }
            result.AppendLine("    return self;");
            result.AppendLine("}");


            StringBuilder fieldReleases = new StringBuilder();
            foreach (FieldDeclaration field in nestedRecords)
            {
                fieldReleases.AppendFormat("[_{0} release]; ", field.Name);
            }

            result.AppendLine(@"-(void)dealloc {{
    if (!_isInjected) {{
        memset(_ptr, 0, sizeof({0}));
        free(_ptr);
    }}
    {1}
    [super dealloc];
}}", type, fieldReleases.ToString());
            result.AppendLine("-(void *)getPointer { return _ptr; }");

            if (hasArrayField)
            {
                CreateArrayClass(record, result);
            }

            result.AppendLine("@end");

            return result.ToString();
        }

        private static string WritePropertyList(BaseRecordDeclaration record)
        {
            var result = new StringBuilder();
            foreach (FieldDeclaration field in record.Fields.Where(IsValidField))
            {
                string type = ToJsType(field.Type);
                bool isRetained = type == "JSValue*" || field.Type.GetBaseRecordDeclaration() != null;
                result.AppendLine("@property (nonatomic{0}) {1} {2};", (isRetained ? ", retain" : ""), type, field.Name);
            }
            return result.ToString().TrimEnd();
        }

        protected string GetType(BaseRecordDeclaration record)
        {
            if (record.IsAnonymousWithoutTypedef())
            {
                var tuple = anonymousToParent[record];
                return string.Format("typeof((({0} *)NULL)->{1}{2})", GetType(tuple.Item1),
                    tuple.Item2.Name, ((tuple.Item2.Type is ConstantArrayType) ? "[0]" : ""));
            }

            return record.GetFullName();
        }

        private static bool IsValidField(FieldDeclaration field)
        {
            return !(field.Name.StartsWith("copy") || field.Name.StartsWith("new") ||
                     field.Name.IsEqualToAny("retain", "release", "dealloc", "init"));
        }

        private static string ToJsType(TypeDefinition type)
        {
            type = type.ResolveWithEnums();

            if (type.GetBaseRecordDeclaration() != null)
            {
                var declaration = type.GetBaseRecordDeclaration();
                return declaration.GetWrapperName() + "*";
            }

            return ConvertToJSType(type);
        }

        private void CreateArrayClass(BaseRecordDeclaration record, StringBuilder result)
        {
            var createArrayMethod = new StringBuilder();

            foreach (FieldDeclaration arrayField in record.Fields.Where(x => x.Type.Resolve() is ConstantArrayType))
            {
                string uniqueName = record.GetUniqueIdentifier() + "_" + arrayField.Name;

                var constantArrayType = (arrayField.Type.Resolve() as ConstantArrayType);
                TypeDefinition elementType = constantArrayType.ElementType.Resolve();

                bool isPrimitive = (elementType is PrimitiveType);
                bool isBoolean = elementType.IsPrimitiveBoolean() || elementType.IsObjCBOOL();
                bool isBaseRecord = (elementType is DeclarationReferenceType) &&
                                    (elementType as DeclarationReferenceType).Target is BaseRecordDeclaration;

                if (!(isPrimitive || isBaseRecord))
                {
                    continue;
                }

                result.AppendLine(@"JSValueRef TNS{0}ArrayGetProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName, JSValueRef* exception) {{
    NSUInteger index = ParsePositiveNumber(propertyName);
    if (index == -1 || !(index < {1})) return NULL;

    {2} *arr = JSObjectGetPrivate(object);", uniqueName, constantArrayType.Size,
                    constantArrayType.ElementType.ToString());
                if (isPrimitive)
                {
                    result.AppendLine("    {0} element = arr[index];", constantArrayType.ElementType.ToString());
                    if (isBoolean)
                    {
                        result.AppendLine("    return JSValueMakeBoolean(ctx, element);");
                    }
                    else
                    {
                        result.AppendLine("    return JSValueMakeNumber(ctx, element);");
                    }
                }
                if (isBaseRecord)
                {
                    result.AppendLine(@"    id wrapper = [[{0} alloc] initWithPointer:arr + index inContext: [[ObjCInheritance getJSContextFromJSContextRef:ctx] JSGlobalContextRef]];
    JSValue *result = [JSValue valueWithObject:wrapper inContext: [ObjCInheritance getJSContextFromJSContextRef:ctx]];
    return result.JSValueRef;",
                        ((elementType as DeclarationReferenceType).Target as BaseRecordDeclaration).GetWrapperName());
                }
                result.AppendLine("}");

                result.AppendLine(@"bool TNS{0}ArraySetProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName, JSValueRef valueRef, JSValueRef* exception) {{
    NSUInteger index = ParsePositiveNumber(propertyName);
    if (index == -1 || !(index < {1})) return false;

    {2} *arr = JSObjectGetPrivate(object);", uniqueName, constantArrayType.Size, elementType.ToString());
                if (isPrimitive || isBoolean)
                {
                    if (isBoolean)
                    {
                        result.AppendLine("    {0} value = JSValueToBoolean(ctx, valueRef);", elementType.ToString());
                    }
                    else
                    {
                        result.AppendLine("    {0} value = JSValueToNumber(ctx, valueRef, NULL);",
                            elementType.ToString());
                    }
                }
                if (isBaseRecord)
                {
                    result.AppendLine(
                        "    JSValue *jsvalue = [JSValue valueWithJSValueRef:valueRef inContext: [ObjCInheritance getJSContextFromJSContextRef:ctx]];");
                    result.AppendLine("    {0} *wrapper = [jsvalue toObjectOfClass:[{0} class]];",
                        ((elementType as DeclarationReferenceType).Target as BaseRecordDeclaration).GetWrapperName());
                    result.AppendLine("    {0} value = *(wrapper->_ptr);", elementType.ToString());
                }
                result.AppendLine(@"
    arr[index] = value;
    return true;
}");

                createArrayMethod.AppendLine(@"    {{
		static JSClassRef arrayClass = NULL;
		if (arrayClass == NULL) {{
			JSClassDefinition classDefinition = kJSClassDefinitionEmpty;
			classDefinition.getProperty = TNS{0}ArrayGetProperty;
			classDefinition.setProperty = TNS{0}ArraySetProperty;
			arrayClass = JSClassCreate(&classDefinition);
		}}

		void *arrRef = &(*(instance->_ptr)).{1};
		JSObjectRef objRef = JSObjectMake([context JSGlobalContextRef], arrayClass, arrRef);
		instance.{1} = [JSValue valueWithJSValueRef:objRef inContext:context];
	}}", uniqueName, arrayField.Name);
            }

            result.AppendLine("+(void)createArrayContainer:({0}*)instance inContext:(JSContext *)context {{",
                record.GetWrapperName());
            result.Append(createArrayMethod);
            result.AppendLine("}");
        }

        private static string GetProtocolName(BaseRecordDeclaration record)
        {
            return "TNS" + record.GetUniqueName() + "JSExport";
        }
    }
}
