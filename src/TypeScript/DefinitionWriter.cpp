#include "DefinitionWriter.h"
#include <clang/AST/DeclObjC.h>
#include <algorithm>
#include <iterator>
#include "../Meta/Utils.h"
#include "../Meta/MetaEntities.h"

namespace TypeScript {
using namespace Meta;

static std::set<std::string> hiddenMethods = { "retain", "release", "autorelease", "allocWithZone", "zone" };

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

void DefinitionWriter::visit(InterfaceMeta* meta)
{
    CompoundMemberMap<MethodMeta> compoundStaticMethods;
    for (auto& method : meta->staticMethods) {
        compoundStaticMethods.emplace(method->id.jsName, std::make_pair(meta, method));
    }

    CompoundMemberMap<PropertyMeta> compoundProperties;
    for (auto& property : meta->properties) {
        if (compoundProperties.find(property->id.jsName) == compoundProperties.end()) {
            compoundProperties.emplace(property->id.jsName, std::make_pair(meta, property));
        }
    }

    CompoundMemberMap<MethodMeta> compoundInstanceMethods;
    for (auto& method : meta->instanceMethods) {
        compoundInstanceMethods.emplace(method->id.jsName, std::make_pair(meta, method));
    }

    std::set<ProtocolMeta*> inheritedProtocols;
    for (DeclId baseClassId = meta->base; !baseClassId.jsName.empty();) {
        InterfaceMeta* baseClass = _container.getMetaAs<InterfaceMeta>(baseClassId).get();

        for (auto& method : baseClass->staticMethods) {
            if (compoundStaticMethods.find(method->id.jsName) == compoundStaticMethods.end()) {
                compoundStaticMethods.emplace(method->id.jsName, std::make_pair(baseClass, method));
            }
        }

        for (auto& property : baseClass->properties) {
            if (compoundProperties.find(property->id.jsName) == compoundProperties.end()) {
                compoundProperties.emplace(property->id.jsName, std::make_pair(baseClass, property));
            }
        }

        for (auto& method : baseClass->instanceMethods) {
            if (compoundInstanceMethods.find(method->id.jsName) == compoundInstanceMethods.end()) {
                compoundInstanceMethods.emplace(method->id.jsName, std::make_pair(baseClass, method));
            }
        }

        for (auto& protocol : baseClass->protocols) {
            getMembersRecursive(protocol, compoundStaticMethods, compoundProperties, compoundInstanceMethods, inheritedProtocols);
        }

        baseClassId = baseClass->base;
    }

    _buffer << std::endl << "\tdeclare class " << meta->id.jsName;
    if (!meta->base.name.empty()) {
        _buffer << " extends " << localizeReference(meta->base);
    }

    std::set<ProtocolMeta*> protocols;
    if (meta->protocols.size()) {
        _buffer << " implements ";
        for (size_t i = 0; i < meta->protocols.size(); i++) {
            getMembersRecursive(meta->protocols[i], compoundStaticMethods, compoundProperties, compoundInstanceMethods, protocols);
            _buffer << localizeReference(meta->protocols[i]);
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
            _buffer << "\t\tstatic " << output << std::endl;
        }
    }

    for (auto& propertyPair : compoundProperties) {
        auto owner = propertyPair.second.first;
        if (owner == meta || immediateProtocols.find(reinterpret_cast<ProtocolMeta*>(owner)) != immediateProtocols.end()) {
            _buffer << "\t\t" << writeProperty(propertyPair.second.second.get(), meta);
            if (owner != meta) {
                _buffer << " //inherited from " << localizeReference(DeclId("", owner->id.jsName, "", owner->id.module));
            }
            _buffer << std::endl;
        }
    }

    for (auto& methodPair : compoundInstanceMethods) {
        if (compoundProperties.find(methodPair.first) != compoundProperties.end()) {
            continue;
        }

        std::string output = writeMethod(methodPair, meta, immediateProtocols);
        if (output.size()) {
            _buffer << "\t\t" << output << std::endl;
        }
    }

    _buffer << "\t}" << std::endl;
}

void DefinitionWriter::getMembersRecursive(DeclId& protocol,
                                           CompoundMemberMap<MethodMeta>& staticMethods,
                                           CompoundMemberMap<PropertyMeta>& properties,
                                           CompoundMemberMap<MethodMeta>& instanceMethods,
                                           std::set<ProtocolMeta*>& visitedProtocols)
{
    if (ProtocolMeta* protocolMeta = _container.getMetaAs<ProtocolMeta>(protocol).get()) {
        visitedProtocols.insert(protocolMeta);

        for (auto& method : protocolMeta->staticMethods) {
            if (staticMethods.find(method->id.jsName) == staticMethods.end()) {
                staticMethods.emplace(method->id.jsName, std::make_pair(protocolMeta, method));
            }
        }

        for (auto& property : protocolMeta->properties) {
            if (properties.find(property->id.jsName) == properties.end()) {
                properties.emplace(property->id.jsName, std::make_pair(protocolMeta, property));
            }
        }

        for (auto& method : protocolMeta->instanceMethods) {
            if (instanceMethods.find(method->id.jsName) == instanceMethods.end()) {
                instanceMethods.emplace(method->id.jsName, std::make_pair(protocolMeta, method));
            }
        }

        for (auto& protocol : protocolMeta->protocols) {
            getMembersRecursive(protocol, staticMethods, properties, instanceMethods, visitedProtocols);
        }
    }
}

void DefinitionWriter::visit(ProtocolMeta* meta)
{
    _buffer << std::endl;

    _buffer << "\tinterface " << meta->id.jsName;
    if (meta->protocols.size()) {
        _buffer << " extends ";
        for (size_t i = 0; i < meta->protocols.size(); i++) {
            _buffer << localizeReference(meta->protocols[i]);
            if (i < meta->protocols.size() - 1) {
                _buffer << ", ";
            }
        }
    }
    _buffer << " {" << std::endl;

    for (auto& property : meta->properties) {
        _buffer << "\t\t" << writeProperty(property.get(), meta) << std::endl;
    }

    for (auto& method : meta->instanceMethods) {
        if (hiddenMethods.find(method->id.jsName) == hiddenMethods.end()) {
            _buffer << "\t\t" << writeMethod(method.get(), meta) << std::endl;
        }
    }

    _buffer << "\t}" << std::endl;

    _buffer << "\tdeclare var " << meta->id.jsName << ": any; /* Protocol */" << std::endl;
}

std::string DefinitionWriter::writeMethod(MethodMeta* meta, BaseClassMeta* owner)
{
    clang::ObjCMethodDecl& methodDecl = *clang::dyn_cast<clang::ObjCMethodDecl>(meta->declaration);
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

    output << meta->id.jsName;
    if (owner->type == MetaType::Protocol && methodDecl.getImplementationControl() == clang::ObjCMethodDecl::ImplementationControl::Optional) {
        output << "?";
    }

    output << "(";

    size_t lastParamIndex = meta->getFlags(::Meta::MetaFlags::MethodHasErrorOutParameter) ? (meta->signature.size() - 1) : meta->signature.size();
    for (size_t i = 1; i < lastParamIndex; i++) {
        output << sanitizeParameterName(parameterNames[i - 1]) << ": " << tsifyType(meta->signature[i]);
        if (i < lastParamIndex - 1) {
            output << ", ";
        }
    }
    output << "): ";

    Type& retType = meta->signature[0];
    if (retType.is(TypeInstancetype)) {
        output << owner->id.jsName;
    }
    else {
        output << tsifyType(retType);
    }

    output << ";";
    return output.str();
}

std::string DefinitionWriter::writeMethod(CompoundMemberMap<MethodMeta>::value_type& methodPair, BaseClassMeta* owner, const std::set<ProtocolMeta*>& protocols)
{
    std::ostringstream output;

    auto memberOwner = methodPair.second.first;
    auto& method = methodPair.second.second;

    if (hiddenMethods.find(method->id.jsName) != hiddenMethods.end()) {
        return std::string();
    }

    if (memberOwner == owner
        || protocols.find(reinterpret_cast<ProtocolMeta*>(memberOwner)) != protocols.end()
        || method->signature[0].is(TypeInstancetype)) {

        output << writeMethod(method.get(), owner);
        if (memberOwner != owner) {
            output << " //inherited from " << localizeReference(memberOwner->id.jsName, memberOwner->id.module->getFullModuleName());
        }
    }

    return output.str();
}

std::string DefinitionWriter::writeProperty(PropertyMeta* meta, BaseClassMeta* owner)
{
    std::ostringstream output;

    output << meta->id.jsName;
    if (owner->type == MetaType::Protocol && clang::dyn_cast<clang::ObjCPropertyDecl>(meta->declaration)->getPropertyImplementation() == clang::ObjCPropertyDecl::PropertyControl::Optional) {
        output << "?";
    }
    output << ": " << tsifyType(meta->getter->signature[0]) << ";";

    return output.str();
}

void DefinitionWriter::visit(CategoryMeta* meta)
{
}

void DefinitionWriter::visit(FunctionMeta* meta)
{
    clang::FunctionDecl& functionDecl = *clang::dyn_cast<clang::FunctionDecl>(meta->declaration);

    std::ostringstream params;
    for (size_t i = 1; i < meta->signature.size(); i++) {
        std::string name = sanitizeParameterName(functionDecl.getParamDecl(i - 1)->getNameAsString());
        params << (name.size() ? name : "p" + std::to_string(i)) << ": " << tsifyType(meta->signature[i]);
        if (i < meta->signature.size() - 1) {
            params << ", ";
        }
    }

    _buffer << std::endl;
    _buffer << "\tdeclare function " << meta->id.jsName
            << "(" << params.str() << "): " << tsifyType(meta->signature[0]) << ";";
    _buffer << std::endl;
}

void DefinitionWriter::visit(StructMeta* meta)
{
    _buffer << std::endl;

    _buffer << "\tinterface " << meta->id.jsName << " {" << std::endl;
    writeMembers(meta->fields);
    _buffer << "\t}" << std::endl;

    _buffer << "\tdeclare var " << meta->id.jsName << ": interop.StructType<" << meta->id.jsName << ">;";

    _buffer << std::endl;
}

void DefinitionWriter::visit(UnionMeta* meta)
{
    _buffer << std::endl;

    _buffer << "\tinterface " << meta->id.jsName << " {" << std::endl;
    writeMembers(meta->fields);
    _buffer << "\t}" << std::endl;

    _buffer << std::endl;
}

void DefinitionWriter::writeMembers(const std::vector<RecordField>& fields)
{
    for (auto& field : fields) {
        _buffer << "\t\t" << field.name << ": " << tsifyType(field.encoding) << ";" << std::endl;
    }
}

void DefinitionWriter::visit(JsCodeMeta* meta)
{
    if (clang::EnumConstantDecl* enumConstantDecl = clang::dyn_cast<clang::EnumConstantDecl>(meta->declaration)) {
        clang::EnumDecl* enumDecl = clang::dyn_cast<clang::EnumDecl>(enumConstantDecl->getLexicalDeclContext());
        if (!enumDecl->hasNameForLinkage()) {
            _buffer << std::endl;
            _buffer << "\t declare const " << meta->id.jsName << ": number;";
            _buffer << std::endl;
        }
    }
    else if (clang::EnumDecl* enumDecl = clang::dyn_cast<clang::EnumDecl>(meta->declaration)) {
        if (enumDecl->hasNameForLinkage()) {
            std::vector<std::string> fieldNames{ enumDecl->getNameAsString() };
            std::vector<clang::EnumConstantDecl*> fields;
            for (clang::EnumConstantDecl* member : enumDecl->enumerators()) {
                fieldNames.push_back(member->getNameAsString());
                fields.push_back(member);
            }

            std::string prefix(Utils::getCommonWordPrefix(fieldNames));

            _buffer << std::endl;
            _buffer << "\tdeclare const enum " << meta->id.jsName << " {" << std::endl;

            for (size_t i = 0; i < fields.size(); i++) {
                _buffer << "\t\t" << fields[i]->getNameAsString().substr(prefix.size()) << " = " << fields[i]->getInitVal().toString(10);
                if (i < fields.size() - 1) {
                    _buffer << ",";
                }
                _buffer << std::endl;
            }

            _buffer << "\t}";
            _buffer << std::endl;
        }
    }
}

void DefinitionWriter::visit(VarMeta* meta)
{
    _buffer << std::endl;
    _buffer << "\tdeclare var " << meta->id.jsName << ": " << tsifyType(meta->signature) << ";";
    _buffer << std::endl;
}

std::string DefinitionWriter::writeFunctionProto(const std::vector<Type>& signature)
{
    std::ostringstream output;
    output << "(";

    for (size_t i = 1; i < signature.size(); i++) {
        output << "p" << i << ": " << tsifyType(signature[i]);
        if (i < signature.size() - 1) {
            output << ", ";
        }
    }

    output << ") => " << tsifyType(signature[0]);
    return output.str();
}

std::string DefinitionWriter::localizeReference(const std::string& jsName, std::string moduleName)
{
    return jsName;
}

std::string DefinitionWriter::localizeReference(const DeclId& name)
{
    return localizeReference(name.jsName, name.module->getFullModuleName());
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
        IdTypeDetails& details = type.getDetailsAs<IdTypeDetails>();
        if (details.protocols.size() == 1) {
            return localizeReference(details.protocols[0]);
        }
        return "any";
    }
    case TypeConstantArray:
        return "interop.Reference<" + tsifyType(type.getDetailsAs<ConstantArrayTypeDetails>().innerType) + ">";
    case TypeIncompleteArray:
        return "interop.Reference<" + tsifyType(type.getDetailsAs<IncompleteArrayTypeDetails>().innerType) + ">";
    case TypePointer: {
        PointerTypeDetails& details = type.getDetailsAs<PointerTypeDetails>();
        if (details.innerType.getType() == TypeVoid) {
            return "interop.Pointer";
        }

        return "interop.Reference<" + tsifyType(details.innerType) + ">";
    }
    case TypeBlock:
        return writeFunctionProto(type.getDetailsAs<BlockTypeDetails>().signature);
    case TypeFunctionPointer:
        return "interop.FunctionReference<" + writeFunctionProto(type.getDetailsAs<FunctionPointerTypeDetails>().signature)
               + ">";
    case TypeInterface:
    case TypeBridgedInterface: {
        InterfaceTypeDetails& details = type.getDetailsAs<InterfaceTypeDetails>();
        if (details.id.jsName == "NSNumber") {
            return "number";
        }
        else if (details.id.jsName == "NSString") {
            return "string";
        }
        else if (details.id.jsName == "NSDate") {
            return "Date";
        }
        else {
            return localizeReference(details.id);
        }
    }
    case TypeStruct:
        return localizeReference(type.getDetailsAs<StructTypeDetails>().id);
    case TypeUnion:
        return localizeReference(type.getDetailsAs<UnionTypeDetails>().id);
    case TypeAnonymousStruct:
    case TypeAnonymousUnion: {
        std::ostringstream output;
        output << "{ ";

        std::vector<RecordField>& fields = type.getDetailsAs<AnonymousStructTypeDetails>().fields;
        for (auto& field : fields) {
            output << field.name << ": " << tsifyType(field.encoding) << "; ";
        }

        output << "}";
        return output.str();
    }
    case TypeEnum:
        return localizeReference(type.getDetailsAs<EnumTypeDetails>().name);
    case TypeUnknown:
    case TypeVaList:
    case TypeInstancetype:
    default:
        break;
    }

    assert(false);
    return "";
}

std::string DefinitionWriter::write()
{
    _buffer.clear();
    _importedModules.clear();

    for (auto& metaPair : *_module) {
        metaPair.second->visit(this);
    }

    std::ostringstream output;

    output << _buffer.str();

    return output.str();
}
}