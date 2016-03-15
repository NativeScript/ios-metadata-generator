#include "DefinitionWriter.h"
#include <clang/AST/DeclObjC.h>
#include <algorithm>
#include <iterator>
#include "Meta/Utils.h"
#include "Utils/StringUtils.h"

namespace TypeScript {
using namespace Meta;

static std::set<std::string> hiddenMethods = { "retain", "release", "autorelease", "allocWithZone", "zone", "countByEnumeratingWithStateObjectsCount" };

static std::set<std::string> bannedIdentifiers = { "function", "arguments", "in" };

static std::string sanitizeParameterName(const std::string& parameterName)
{
    if (bannedIdentifiers.find(parameterName) != bannedIdentifiers.end()) {
        return "_" + parameterName;
    }
    else {
        return parameterName;
    }
}

static std::string getTypeParametersStringOrEmpty(const clang::ObjCInterfaceDecl* interfaceDecl)
{
    std::ostringstream output;
    if (clang::ObjCTypeParamList* typeParameters = interfaceDecl->getTypeParamListAsWritten()) {
        if (typeParameters->size()) {
            output << "<";
            for (unsigned i = 0; i < typeParameters->size(); i++) {
                clang::ObjCTypeParamDecl* typeParam = *(typeParameters->begin() + i);
                output << typeParam->getNameAsString();
                if (i < typeParameters->size() - 1) {
                    output << ", ";
                }
            }
            output << ">";
        }
    }

    return output.str();
}

std::string DefinitionWriter::getTypeArgumentsStringOrEmpty(const clang::ObjCObjectType* objectType)
{
    std::ostringstream output;
    llvm::ArrayRef<clang::QualType> typeArgs = objectType->getTypeArgsAsWritten();
    if (!typeArgs.empty()) {
        output << "<";
        for (unsigned i = 0; i < typeArgs.size(); i++) {
            output << tsifyType(*_typeFactory.create(typeArgs[i]));
            if (i < typeArgs.size() - 1) {
                output << ", ";
            }
        }
        output << ">";
    }

    return output.str();
}

void DefinitionWriter::visit(InterfaceMeta* meta)
{
    CompoundMemberMap<MethodMeta> compoundStaticMethods;
    for (MethodMeta* method : meta->staticMethods) {
        compoundStaticMethods.emplace(method->jsName, std::make_pair(meta, method));
    }

    CompoundMemberMap<PropertyMeta> baseClassProperties;
    CompoundMemberMap<PropertyMeta> ownProperties;
    for (PropertyMeta* property : meta->properties) {
        if (ownProperties.find(property->jsName) == ownProperties.end()) {
            ownProperties.emplace(property->jsName, std::make_pair(meta, property));
        }
    }

    CompoundMemberMap<MethodMeta> compoundInstanceMethods;
    for (MethodMeta* method : meta->instanceMethods) {
        compoundInstanceMethods.emplace(method->jsName, std::make_pair(meta, method));
    }

    std::set<ProtocolMeta*> inheritedProtocols;
    if (meta->base != nullptr) {
        InterfaceMeta* baseClass = meta->base;

        for (MethodMeta* method : baseClass->staticMethods) {
            if (compoundStaticMethods.find(method->jsName) == compoundStaticMethods.end()) {
                compoundStaticMethods.emplace(method->jsName, std::make_pair(baseClass, method));
            }
        }

        for (PropertyMeta* property : baseClass->properties) {
            if (baseClassProperties.find(property->jsName) == baseClassProperties.end()) {
                baseClassProperties.emplace(property->jsName, std::make_pair(baseClass, property));
            }
        }

        for (MethodMeta* method : baseClass->instanceMethods) {
            if (compoundInstanceMethods.find(method->jsName) == compoundInstanceMethods.end()) {
                compoundInstanceMethods.emplace(method->jsName, std::make_pair(baseClass, method));
            }
        }

        for (ProtocolMeta* protocol : baseClass->protocols) {
            getMembersRecursive(protocol, &compoundStaticMethods, &baseClassProperties, &compoundInstanceMethods, inheritedProtocols);
        }
    }

    _buffer << std::endl
            << _docSet.getCommentFor(meta).toString("") << "declare class " << meta->jsName << getTypeParametersStringOrEmpty(clang::cast<clang::ObjCInterfaceDecl>(meta->declaration));
    if (meta->base != nullptr) {
        _buffer << " extends " << localizeReference(*meta->base) << getTypeArgumentsStringOrEmpty(clang::cast<clang::ObjCInterfaceDecl>(meta->declaration)->getSuperClassType());
    }

    CompoundMemberMap<PropertyMeta> protocolInheritedProperties;
    std::set<ProtocolMeta*> protocols;
    if (meta->protocols.size()) {
        _buffer << " implements ";
        for (size_t i = 0; i < meta->protocols.size(); i++) {
            getMembersRecursive(meta->protocols[i], &compoundStaticMethods, &protocolInheritedProperties, &compoundInstanceMethods, protocols);
            _buffer << localizeReference(*meta->protocols[i]);
            if (i < meta->protocols.size() - 1) {
                _buffer << ", ";
            }
        }
    }
    _buffer << " {" << std::endl;

    std::set<ProtocolMeta*> immediateProtocols;
    for (auto protocol : protocols) {
        if (inheritedProtocols.find(protocol) == inheritedProtocols.end()) {
            immediateProtocols.insert(protocol);
        }
    }

    for (auto& methodPair : compoundStaticMethods) {
        std::string output = writeMethod(methodPair, meta, immediateProtocols);
        if (output.size()) {
            MethodMeta* method = methodPair.second.second;
            BaseClassMeta* owner = methodPair.second.first;
            _buffer << std::endl
                    << _docSet.getCommentFor(method, owner).toString("\t");
            _buffer << "\tstatic " << output << std::endl;
        }
    }

    for (auto& propertyPair : ownProperties) {
        BaseClassMeta* owner = propertyPair.second.first;
        PropertyMeta* propertyMeta = propertyPair.second.second;

        if (owner == meta) {

            this->writeProperty(propertyMeta, owner, meta, baseClassProperties);
        }
    }

    for (auto& propertyPair : protocolInheritedProperties) {
        BaseClassMeta* owner = propertyPair.second.first;
        PropertyMeta* propertyMeta = propertyPair.second.second;

        bool isDuplicated = ownProperties.find(propertyMeta->jsName) != ownProperties.end();
        if (immediateProtocols.find(reinterpret_cast<ProtocolMeta*>(owner)) != immediateProtocols.end() && !isDuplicated) {
            this->writeProperty(propertyMeta, owner, meta, baseClassProperties);
        }
    }

    auto objectAtIndexedSubscript = compoundInstanceMethods.find("objectAtIndexedSubscript");
    if (objectAtIndexedSubscript != compoundInstanceMethods.end()) {
        const Type* retType = objectAtIndexedSubscript->second.second->signature[0];
        std::string indexerReturnType = computeMethodReturnType(retType, meta);
        _buffer << "\t[index: number]: " << indexerReturnType << ";" << std::endl;
    }

    if (compoundInstanceMethods.find("countByEnumeratingWithStateObjectsCount") != compoundInstanceMethods.end()) {
        _buffer << "\t[Symbol.iterator](): Iterator<any>;" << std::endl;
    }

    for (auto& methodPair : compoundInstanceMethods) {
        if (methodPair.second.second->getFlags(MethodIsInitializer)) {
            _buffer << std::endl
                    << _docSet.getCommentFor(methodPair.second.second, methodPair.second.first).toString("\t");
            _buffer << "\t" << writeConstructor(methodPair, meta) << std::endl;
        }
    }

    for (auto& methodPair : compoundInstanceMethods) {
        if (ownProperties.find(methodPair.first) != ownProperties.end() || methodPair.second.second->getFlags(MethodIsInitializer)) {
            continue;
        }

        std::string output = writeMethod(methodPair, meta, immediateProtocols);
        if (output.size()) {
            _buffer << std::endl
                    << _docSet.getCommentFor(methodPair.second.second, methodPair.second.first).toString("\t");
            _buffer << "\t" << output << std::endl;
        }
    }

    _buffer << "}" << std::endl;
}

void DefinitionWriter::writeProperty(PropertyMeta* propertyMeta, BaseClassMeta* owner, InterfaceMeta* target, CompoundMemberMap<PropertyMeta> baseClassProperties) {
    _buffer << std::endl
            << _docSet.getCommentFor(propertyMeta, owner).toString("\t");
    _buffer << "\t";

    if (!propertyMeta->setter) {
        _buffer << "/* readonly */ ";
    }

    bool optOutTypeChecking = false;
    auto result = baseClassProperties.find(propertyMeta->jsName);
    if (result != baseClassProperties.end()) {
        optOutTypeChecking = result->second.second->getter->signature[0] != propertyMeta->getter->signature[0];
    }
    _buffer << writeProperty(propertyMeta, target, optOutTypeChecking);

    if (owner != target) {
        _buffer << " // inherited from " << localizeReference(*owner);
    }

    _buffer << std::endl;
}

void DefinitionWriter::getMembersRecursive(ProtocolMeta* protocolMeta,
    CompoundMemberMap<MethodMeta>* staticMethods,
    CompoundMemberMap<PropertyMeta>* properties,
    CompoundMemberMap<MethodMeta>* instanceMethods,
    std::set<ProtocolMeta*>& visitedProtocols)
{
    visitedProtocols.insert(protocolMeta);

    if (staticMethods) {
        for (MethodMeta* method : protocolMeta->staticMethods) {
            if (staticMethods->find(method->jsName) == staticMethods->end()) {
                staticMethods->emplace(method->jsName, std::make_pair(protocolMeta, method));
            }
        }
    }

    if (properties) {
        for (PropertyMeta* property : protocolMeta->properties) {
            if (properties->find(property->jsName) == properties->end()) {
                properties->emplace(property->jsName, std::make_pair(protocolMeta, property));
            }
        }
    }

    if (instanceMethods) {
        for (MethodMeta* method : protocolMeta->instanceMethods) {
            if (instanceMethods->find(method->jsName) == instanceMethods->end()) {
                instanceMethods->emplace(method->jsName, std::make_pair(protocolMeta, method));
            }
        }
    }

    for (ProtocolMeta* protocol : protocolMeta->protocols) {
        getMembersRecursive(protocol, staticMethods, properties, instanceMethods, visitedProtocols);
    }
}

void DefinitionWriter::visit(ProtocolMeta* meta)
{
    _buffer << std::endl
            << _docSet.getCommentFor(meta).toString("");

    _buffer << "interface " << meta->jsName;
    std::map<std::string, PropertyMeta*> conformedProtocolsProperties;
    if (meta->protocols.size()) {
        _buffer << " extends ";
        for (size_t i = 0; i < meta->protocols.size(); i++) {
            std::transform(meta->protocols[i]->properties.begin(), meta->protocols[i]->properties.end(), std::inserter(conformedProtocolsProperties, conformedProtocolsProperties.end()), [](PropertyMeta* propertyMeta) {
                return std::make_pair(propertyMeta->jsName, propertyMeta);
            });

            _buffer << localizeReference(*meta->protocols[i]);
            if (i < meta->protocols.size() - 1) {
                _buffer << ", ";
            }
        }
    }
    _buffer << " {" << std::endl;

    for (PropertyMeta* property : meta->properties) {
        bool optOutTypeChecking = conformedProtocolsProperties.find(property->jsName) != conformedProtocolsProperties.end();
        _buffer << std::endl
                << _docSet.getCommentFor(property, meta).toString("\t") << "\t" << writeProperty(property, meta, optOutTypeChecking) << std::endl;
    }

    for (MethodMeta* method : meta->instanceMethods) {
        if (hiddenMethods.find(method->jsName) == hiddenMethods.end()) {
            _buffer << std::endl
                    << _docSet.getCommentFor(method, meta).toString("\t") << "\t" << writeMethod(method, meta) << std::endl;
        }
    }

    _buffer << "}" << std::endl;

    _buffer << "declare var " << meta->jsName << ": {" << std::endl;

    _buffer << std::endl
            << "\tprototype: " << meta->jsName << ";" << std::endl;

    CompoundMemberMap<MethodMeta> compoundStaticMethods;
    for (MethodMeta* method : meta->staticMethods) {
        compoundStaticMethods.emplace(method->jsName, std::make_pair(meta, method));
    }

    std::set<ProtocolMeta*> protocols;
    for (ProtocolMeta* protocol : meta->protocols) {
        getMembersRecursive(protocol, &compoundStaticMethods, nullptr, nullptr, protocols);
    }

    for (auto& methodPair : compoundStaticMethods) {
        std::string output = writeMethod(methodPair, meta, protocols);
        if (output.size()) {
            MethodMeta* method = methodPair.second.second;
            BaseClassMeta* owner = methodPair.second.first;
            _buffer << std::endl
                    << _docSet.getCommentFor(method, owner).toString("\t");
            _buffer << "\t" << output << std::endl;
        }
    }

    _buffer << "};" << std::endl;
}

std::string DefinitionWriter::writeConstructor(const CompoundMemberMap<MethodMeta>::value_type& initializer,
    const BaseClassMeta* owner)
{
    MethodMeta* method = initializer.second.second;
    assert(method->getFlags(MethodIsInitializer));

    std::ostringstream output;

    if (method->constructorTokens == "") {
        output << "constructor();";
    }
    else {
        std::vector<std::string> ctorTokens;
        StringUtils::split(method->constructorTokens, ':', std::back_inserter(ctorTokens));
        output << "constructor(o: { ";
        for (size_t i = 0; i < ctorTokens.size(); i++) {
            output << ctorTokens[i] << ": ";
            output << (i + 1 < method->signature.size() ? tsifyType(*method->signature[i + 1]) : "void") << "; ";
        }
        output << "});";
    }

    BaseClassMeta* initializerOwner = initializer.second.first;
    if (initializerOwner != owner) {
        output << " // inherited from " << initializerOwner->jsName;
    }

    return output.str();
}

std::string DefinitionWriter::writeMethod(MethodMeta* meta, BaseClassMeta* owner)
{
    const clang::ObjCMethodDecl& methodDecl = *clang::dyn_cast<clang::ObjCMethodDecl>(meta->declaration);
    auto parameters = methodDecl.parameters();

    std::vector<std::string> parameterNames;
    std::transform(parameters.begin(), parameters.end(), std::back_inserter(parameterNames), [](clang::ParmVarDecl* param) {
        return param->getNameAsString();
    });

    for (size_t i = 0; i < parameterNames.size(); i++) {
        for (size_t n = 0; n < parameterNames.size(); n++) {
            if (parameterNames[i] == parameterNames[n] && i != n) {
                parameterNames[n] += std::to_string(n);
            }
        }
    }

    std::ostringstream output;

    output << meta->jsName;

    const Type* retType = meta->signature[0];
    if (!methodDecl.isInstanceMethod() && owner->is(MetaType::Interface)) {
        if (retType->is(TypeInstancetype) || DefinitionWriter::hasClosedGenerics(*retType)) {
            output << getTypeParametersStringOrEmpty(
                clang::cast<clang::ObjCInterfaceDecl>(static_cast<const InterfaceMeta*>(owner)->declaration));
        }
    }

    if ((owner->type == MetaType::Protocol && methodDecl.getImplementationControl() == clang::ObjCMethodDecl::ImplementationControl::Optional) || (owner->is(MetaType::Protocol) && meta->getFlags(MethodIsInitializer))) {
        output << "?";
    }

    output << "(";

    size_t lastParamIndex = meta->getFlags(::Meta::MetaFlags::MethodHasErrorOutParameter) ? (meta->signature.size() - 1) : meta->signature.size();
    for (size_t i = 1; i < lastParamIndex; i++) {
        output << sanitizeParameterName(parameterNames[i - 1]) << ": " << tsifyType(*meta->signature[i]);
        if (i < lastParamIndex - 1) {
            output << ", ";
        }
    }
    output << "): " << computeMethodReturnType(retType, owner) << ";";
    return output.str();
}

std::string DefinitionWriter::writeMethod(CompoundMemberMap<MethodMeta>::value_type& methodPair, BaseClassMeta* owner, const std::set<ProtocolMeta*>& protocols)
{
    std::ostringstream output;

    BaseClassMeta* memberOwner = methodPair.second.first;
    MethodMeta* method = methodPair.second.second;

    if (hiddenMethods.find(method->jsName) != hiddenMethods.end()) {
        return std::string();
    }

    if (memberOwner == owner
        || protocols.find(static_cast<ProtocolMeta*>(memberOwner)) != protocols.end()
        || method->signature[0]->is(TypeInstancetype)) {

        output << writeMethod(method, owner);
        if (memberOwner != owner) {
            output << " // inherited from " << localizeReference(memberOwner->jsName, memberOwner->module->getFullModuleName());
        }
    }

    return output.str();
}

std::string DefinitionWriter::writeProperty(PropertyMeta* meta, BaseClassMeta* owner, bool optOutTypeChecking) {
    std::ostringstream output;

    output << meta->jsName;
    if (owner->is(MetaType::Protocol) && clang::dyn_cast<clang::ObjCPropertyDecl>(meta->declaration)->getPropertyImplementation() == clang::ObjCPropertyDecl::PropertyControl::Optional) {
        output << "?";
    }

    std::string returnType = tsifyType(*meta->getter->signature[0]);
    if (optOutTypeChecking) {
        output << ": any; /*" << returnType << " */";
    } else {
        output << ": " << returnType << ";";
    }

    return output.str();
}

void DefinitionWriter::visit(CategoryMeta* meta)
{
}

void DefinitionWriter::visit(FunctionMeta* meta)
{
    const clang::FunctionDecl& functionDecl = *clang::dyn_cast<clang::FunctionDecl>(meta->declaration);

    std::ostringstream params;
    for (size_t i = 1; i < meta->signature.size(); i++) {
        std::string name = sanitizeParameterName(functionDecl.getParamDecl(i - 1)->getNameAsString());
        params << (name.size() ? name : "p" + std::to_string(i)) << ": " << tsifyType(*meta->signature[i]);
        if (i < meta->signature.size() - 1) {
            params << ", ";
        }
    }

    _buffer << std::endl
            << _docSet.getCommentFor(meta).toString("");
    _buffer << "declare function " << meta->jsName
            << "(" << params.str() << "): ";

    std::string returnName = tsifyType(*meta->signature[0]);
    if (meta->getFlags(MetaFlags::FunctionReturnsUnmanaged)) {
        returnName = "interop.Unmanaged<" + returnName + ">";
    }

    _buffer << returnName << ";";

    _buffer << std::endl;
}

void DefinitionWriter::visit(StructMeta* meta)
{
    TSComment comment = _docSet.getCommentFor(meta);
    _buffer << std::endl
            << comment.toString("");

    _buffer << "interface " << meta->jsName << " {" << std::endl;
    writeMembers(meta->fields, comment.fields);
    _buffer << "}" << std::endl;

    _buffer << "declare var " << meta->jsName << ": interop.StructType<" << meta->jsName << ">;";

    _buffer << std::endl;
}

void DefinitionWriter::visit(UnionMeta* meta)
{
    TSComment comment = _docSet.getCommentFor(meta);
    _buffer << std::endl
            << comment.toString("");

    _buffer << "interface " << meta->jsName << " {" << std::endl;
    writeMembers(meta->fields, comment.fields);
    _buffer << "}" << std::endl;

    _buffer << std::endl;
}

void DefinitionWriter::writeMembers(const std::vector<RecordField>& fields, std::vector<TSComment> fieldsComments)
{
    for (size_t i = 0; i < fields.size(); i++) {
        if (i < fieldsComments.size()) {
            _buffer << fieldsComments[i].toString("\t");
        }
        _buffer << "\t" << fields[i].name << ": " << tsifyType(*fields[i].encoding) << ";" << std::endl;
    }
}

void DefinitionWriter::visit(EnumMeta* meta)
{
    _buffer << std::endl
            << _docSet.getCommentFor(meta).toString("");
    _buffer << "declare const enum " << meta->jsName << " {" << std::endl;

    std::vector<EnumField>& fields = meta->swiftNameFields.size() != 0 ? meta->swiftNameFields : meta->fullNameFields;

    for (size_t i = 0; i < fields.size(); i++) {
        _buffer << std::endl
                << _docSet.getCommentFor(meta->fullNameFields[i].name, MetaType::EnumConstant).toString("\t");
        _buffer << "\t" << fields[i].name << " = " << fields[i].value;
        if (i < fields.size() - 1) {
            _buffer << ",";
        }
        _buffer << std::endl;
    }

    _buffer << "}";
    _buffer << std::endl;
}

void DefinitionWriter::visit(VarMeta* meta)
{
    _buffer << std::endl
            << _docSet.getCommentFor(meta).toString("");
    _buffer << "declare var " << meta->jsName << ": " << tsifyType(*meta->signature) << ";" << std::endl;
}

std::string DefinitionWriter::writeFunctionProto(const std::vector<Type*>& signature)
{
    std::ostringstream output;
    output << "(";

    for (size_t i = 1; i < signature.size(); i++) {
        output << "p" << i << ": " << tsifyType(*signature[i]);
        if (i < signature.size() - 1) {
            output << ", ";
        }
    }

    output << ") => " << tsifyType(*signature[0]);
    return output.str();
}

void DefinitionWriter::visit(MethodMeta* meta)
{
}

void DefinitionWriter::visit(PropertyMeta* meta)
{
}

void DefinitionWriter::visit(EnumConstantMeta* meta)
{
}

std::string DefinitionWriter::localizeReference(const std::string& jsName, std::string moduleName)
{
    return jsName;
}

std::string DefinitionWriter::localizeReference(const ::Meta::Meta& meta)
{
    return localizeReference(meta.jsName, meta.module->getFullModuleName());
}

bool DefinitionWriter::hasClosedGenerics(const Type& type)
{
    if (type.is(TypeInterface)) {
        const InterfaceType& interfaceType = type.as<InterfaceType>();
        return interfaceType.typeArguments.size();
    }

    return false;
}

std::string DefinitionWriter::tsifyType(const Type& type)
{
    switch (type.getType()) {
    case TypeVoid:
        return "void";
    case TypeBool:
        return "boolean";
    case TypeSignedChar:
    case TypeUnsignedChar:
    case TypeShort:
    case TypeUShort:
    case TypeInt:
    case TypeUInt:
    case TypeLong:
    case TypeULong:
    case TypeLongLong:
    case TypeULongLong:
    case TypeFloat:
    case TypeDouble:
        return "number";
    case TypeUnichar:
    case TypeCString:
    case TypeSelector:
        return "string";
    case TypeProtocol:
        return "any /* Protocol */";
    case TypeClass:
        return "typeof " + localizeReference("NSObject", "ObjectiveC");
    case TypeId: {
        const IdType& idType = type.as<IdType>();
        if (idType.protocols.size() == 1) {
            return localizeReference(*idType.protocols[0]);
        }
        return "any";
    }
    case TypeConstantArray:
        return "interop.Reference<" + tsifyType(*type.as<ConstantArrayType>().innerType) + ">";
    case TypeIncompleteArray:
        return "interop.Reference<" + tsifyType(*type.as<IncompleteArrayType>().innerType) + ">";
    case TypePointer: {
        const PointerType& pointerType = type.as<PointerType>();
        return (pointerType.innerType->is(TypeVoid)) ? "interop.Pointer" : "interop.Reference<" + tsifyType(*pointerType.innerType) + ">";
    }
    case TypeBlock:
        return writeFunctionProto(type.as<BlockType>().signature);
    case TypeFunctionPointer:
        return "interop.FunctionReference<" + writeFunctionProto(type.as<FunctionPointerType>().signature)
            + ">";
    case TypeInterface:
    case TypeBridgedInterface: {
        if (type.is(TypeType::TypeBridgedInterface) && type.as<BridgedInterfaceType>().isId()) {
            return tsifyType(IdType());
        }

        const InterfaceMeta& interface = type.is(TypeType::TypeInterface) ? *type.as<InterfaceType>().interface : *type.as<BridgedInterfaceType>().bridgedInterface;
        if (interface.name == "NSNumber") {
            return "number";
        }
        else if (interface.name == "NSString") {
            return "string";
        }
        else if (interface.name == "NSDate") {
            return "Date";
        }

        std::ostringstream output;
        output << localizeReference(interface);

        bool hasClosedGenerics = DefinitionWriter::hasClosedGenerics(type);
        if (hasClosedGenerics) {
            const InterfaceType& interfaceType = type.as<InterfaceType>();
            output << "<";
            for (size_t i = 0; i < interfaceType.typeArguments.size(); i++) {
                output << tsifyType(*interfaceType.typeArguments[i]);
                if (i < interfaceType.typeArguments.size() - 1) {
                    output << ", ";
                }
            }
            output << ">";
        }
        else {
            // This also translates CFArray to NSArray<any>
            if (auto typeParamList = clang::dyn_cast<clang::ObjCInterfaceDecl>(interface.declaration)->getTypeParamListAsWritten()) {
                output << "<";
                for (size_t i = 0; i < typeParamList->size(); i++) {
                    output << "any";
                    if (i < typeParamList->size() - 1) {
                        output << ", ";
                    }
                }
                output << ">";
            }
        }

        return output.str();
    }
    case TypeStruct:
        return localizeReference(*type.as<StructType>().structMeta);
    case TypeUnion:
        return localizeReference(*type.as<UnionType>().unionMeta);
    case TypeAnonymousStruct:
    case TypeAnonymousUnion: {
        std::ostringstream output;
        output << "{ ";

        const std::vector<RecordField>& fields = type.as<AnonymousStructType>().fields;
        for (auto& field : fields) {
            output << field.name << ": " << tsifyType(*field.encoding) << "; ";
        }

        output << "}";
        return output.str();
    }
    case TypeEnum:
        return localizeReference(*type.as<EnumType>().enumMeta);
    case TypeTypeArgument:
        return type.as<TypeArgumentType>().name;
    case TypeVaList:
    case TypeInstancetype:
    default:
        break;
    }

    assert(false);
    return "";
}

std::string DefinitionWriter::computeMethodReturnType(const Type* retType, const BaseClassMeta* owner)
{
    std::ostringstream output;
    if (retType->is(TypeInstancetype)) {
        output << owner->jsName;
        if (owner->is(MetaType::Interface)) {
            output << getTypeParametersStringOrEmpty(clang::cast<clang::ObjCInterfaceDecl>(static_cast<const InterfaceMeta*>(owner)->declaration));
        }
    }
    else {
        output << tsifyType(*retType);
    }

    return output.str();
}

std::string DefinitionWriter::write()
{
    _buffer.clear();
    _importedModules.clear();
    for (::Meta::Meta* meta : _module.second) {
        meta->visit(this);
    }

    std::ostringstream output;

    output << _buffer.str();

    return output.str();
}
}
