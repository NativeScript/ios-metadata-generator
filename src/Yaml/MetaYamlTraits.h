#pragma once

#include "../Meta/MetaEntities.h"
#include "../Meta/Utils.h"

namespace llvm {
namespace yaml {
    bool operator==(Meta::Version& x, const Meta::Version& y)
    {
        return x.Major == y.Major && x.Minor == y.Minor && x.SubMinor == y.SubMinor;
    }

    Meta::MetaFlags operator|(Meta::MetaFlags& value1, Meta::MetaFlags value2)
    {
        return (Meta::MetaFlags)((uint32_t)value1 | (uint32_t)value2);
    }
}
}

#include <llvm/Support/YAMLTraits.h>

LLVM_YAML_IS_SEQUENCE_VECTOR(std::string)
LLVM_YAML_IS_SEQUENCE_VECTOR(Meta::DeclId)
LLVM_YAML_IS_SEQUENCE_VECTOR(clang::Module::LinkLibrary)
LLVM_YAML_IS_SEQUENCE_VECTOR(Meta::RecordField)
LLVM_YAML_IS_SEQUENCE_VECTOR(std::shared_ptr<Meta::Meta>)
LLVM_YAML_IS_SEQUENCE_VECTOR(std::shared_ptr<Meta::MethodMeta>)
LLVM_YAML_IS_SEQUENCE_VECTOR(std::shared_ptr<Meta::PropertyMeta>)
LLVM_YAML_IS_SEQUENCE_VECTOR(Meta::Type)
LLVM_YAML_STRONG_TYPEDEF(std::shared_ptr<Meta::Meta>, BaseMeta)

namespace llvm {
namespace yaml {

    // ModuleMeta
    template <>
    struct MappingTraits<Meta::ModuleMeta> {

        class NormalizedModule {
        public:
            NormalizedModule(IO& io)
                : clangModule()
                , declarations()
            {
            }

            NormalizedModule(IO& io, Meta::ModuleMeta& module)
                : clangModule(module.getClangModule())
            {
                for (auto& pair : module) {
                    declarations.push_back(pair.second);
                }
            }

            Meta::ModuleMeta denormalize(IO& io)
            {
                Meta::ModuleMeta module(clangModule, declarations);
                return module;
            }

            clang::Module* clangModule;
            std::vector<std::shared_ptr<Meta::Meta> > declarations;
        };

        static void mapping(IO& io, Meta::ModuleMeta& module)
        {
            MappingNormalization<NormalizedModule, Meta::ModuleMeta> keys(io, module);

            io.mapRequired("Module", *keys->clangModule);
            io.mapRequired("Items", keys->declarations);
        }
    };

    // Version
    template <>
    struct ScalarTraits<Meta::Version> {
        static void output(const Meta::Version& value, void* context, raw_ostream& out)
        {
            if (value.Major >= 0) {
                out << value.Major;
                if (value.Minor >= 0) {
                    out << "." << value.Minor;
                    if (value.SubMinor >= 0) {
                        out << "." << value.SubMinor;
                    }
                }
            }
        }

        static StringRef input(StringRef stringValue, void* context, Meta::Version& value)
        {
            value = UNKNOWN_VERSION;
            if (stringValue.size() == 0) {
                return StringRef();
            }
            std::string version = stringValue.str();

            unsigned long firstDotIndex = version.find(".");
            value.Major = (firstDotIndex != std::string::npos) ? std::stoi(version.substr(0, firstDotIndex)) : std::stoi(version);
            if (firstDotIndex != std::string::npos) {
                unsigned long secondDotIndex = version.find(".", firstDotIndex + 1);
                value.Minor = std::stoi(version.substr(firstDotIndex + 1, (secondDotIndex != std::string::npos) ? secondDotIndex - firstDotIndex - 1 : std::string::npos));
                if (secondDotIndex != std::string::npos) {
                    value.SubMinor = std::stoi(version.substr(secondDotIndex + 1, std::string::npos));
                }
            }

            // TODO: We can validate the version and return non-empty string if the yaml format of the version is invalid
            return StringRef();
        }
        // Determine if this scalar needs quotes.
        static bool mustQuote(StringRef) { return false; }
    };

    // MetaFlags
    template <>
    struct ScalarBitSetTraits<Meta::MetaFlags> {

        static void bitset(IO& io, Meta::MetaFlags& value)
        {
            io.bitSetCase(value, "IsIosAppExtensionAvailable", Meta::MetaFlags::IsIosAppExtensionAvailable);
            //io.bitSetCase(value, "HasName",  Meta::MetaFlags::HasName);

            io.bitSetCase(value, "FunctionIsVariadic", Meta::MetaFlags::FunctionIsVariadic);
            io.bitSetCase(value, "FunctionOwnsReturnedCocoaObject", Meta::MetaFlags::FunctionOwnsReturnedCocoaObject);

            io.bitSetCase(value, "MethodIsVariadic", Meta::MetaFlags::MethodIsVariadic);
            io.bitSetCase(value, "MethodIsNullTerminatedVariadic", Meta::MetaFlags::MethodIsNullTerminatedVariadic);
            io.bitSetCase(value, "MethodOwnsReturnedCocoaObject", Meta::MetaFlags::MethodOwnsReturnedCocoaObject);
        }
    };

    // MetaType
    template <>
    struct ScalarEnumerationTraits<Meta::MetaType> {
        static void enumeration(IO& io, Meta::MetaType& value)
        {
            io.enumCase(value, "Undefined", Meta::MetaType::Undefined);
            io.enumCase(value, "Struct", Meta::MetaType::Struct);
            io.enumCase(value, "Union", Meta::MetaType::Union);
            io.enumCase(value, "Function", Meta::MetaType::Function);
            io.enumCase(value, "JsCode", Meta::MetaType::JsCode);
            io.enumCase(value, "Var", Meta::MetaType::Var);
            io.enumCase(value, "Interface", Meta::MetaType::Interface);
            io.enumCase(value, "Protocol", Meta::MetaType::Protocol);
            io.enumCase(value, "Category", Meta::MetaType::Category);
            io.enumCase(value, "Method", Meta::MetaType::Method);
            io.enumCase(value, "Property", Meta::MetaType::Property);
        }
    };

    // TypeType
    template <>
    struct ScalarEnumerationTraits<Meta::TypeType> {
        static void enumeration(IO& io, Meta::TypeType& value)
        {
            io.enumCase(value, "Void", Meta::TypeType::TypeVoid);
            io.enumCase(value, "Bool", Meta::TypeType::TypeBool);
            io.enumCase(value, "Short", Meta::TypeType::TypeShort);
            io.enumCase(value, "Ushort", Meta::TypeType::TypeUShort);
            io.enumCase(value, "Int", Meta::TypeType::TypeInt);
            io.enumCase(value, "UInt", Meta::TypeType::TypeUInt);
            io.enumCase(value, "Long", Meta::TypeType::TypeLong);
            io.enumCase(value, "ULong", Meta::TypeType::TypeULong);
            io.enumCase(value, "LongLong", Meta::TypeType::TypeLongLong);
            io.enumCase(value, "ULongLong", Meta::TypeType::TypeULongLong);
            io.enumCase(value, "Char", Meta::TypeType::TypeSignedChar);
            io.enumCase(value, "UChar", Meta::TypeType::TypeUnsignedChar);
            io.enumCase(value, "Unichar", Meta::TypeType::TypeUnichar);
            io.enumCase(value, "CString", Meta::TypeType::TypeCString);
            io.enumCase(value, "Float", Meta::TypeType::TypeFloat);
            io.enumCase(value, "Double", Meta::TypeType::TypeDouble);
            io.enumCase(value, "Selector", Meta::TypeType::TypeSelector);
            io.enumCase(value, "Class", Meta::TypeType::TypeClass);
            io.enumCase(value, "Instancetype", Meta::TypeType::TypeInstancetype);
            io.enumCase(value, "Id", Meta::TypeType::TypeId);
            io.enumCase(value, "ConstantArray", Meta::TypeType::TypeConstantArray);
            io.enumCase(value, "IncompleteArray", Meta::TypeType::TypeIncompleteArray);
            io.enumCase(value, "Interface", Meta::TypeType::TypeInterface);
            io.enumCase(value, "Interface", Meta::TypeType::TypeBridgedInterface);
            io.enumCase(value, "Pointer", Meta::TypeType::TypePointer);
            io.enumCase(value, "FunctionPointer", Meta::TypeType::TypeFunctionPointer);
            io.enumCase(value, "Block", Meta::TypeType::TypeBlock);
            io.enumCase(value, "Struct", Meta::TypeType::TypeStruct);
            io.enumCase(value, "Union", Meta::TypeType::TypeUnion);
            io.enumCase(value, "AnonymousStruct", Meta::TypeType::TypeAnonymousStruct);
            io.enumCase(value, "AnonymousUnion", Meta::TypeType::TypeAnonymousUnion);
            io.enumCase(value, "Enum", Meta::TypeType::TypeEnum);
            io.enumCase(value, "VaList", Meta::TypeType::TypeVaList);
            io.enumCase(value, "Protocol", Meta::TypeType::TypeProtocol);
            io.enumCase(value, "Unknown", Meta::TypeType::TypeUnknown);
        }
    };

    // clang::Module::LinkLibrary
    template <>
    struct MappingTraits<clang::Module::LinkLibrary> {

        static void mapping(IO& io, clang::Module::LinkLibrary& lib)
        {
            io.mapRequired("Library", lib.Library);
            io.mapRequired("IsFramework", lib.IsFramework);
        }
    };

    // clang::Module
    template <>
    struct MappingTraits<clang::Module> {

        static void mapping(IO& io, clang::Module& module)
        {
            std::string fullModuleName = module.getFullModuleName();
            bool isPartOfFramework = module.isPartOfFramework();
            bool isSystem = module.IsSystem;
            std::vector<clang::Module::LinkLibrary> libs;

            Meta::Utils::getAllLinkLibraries(&module, libs);

            io.mapRequired("FullName", fullModuleName);
            io.mapRequired("IsPartOfFramework", isPartOfFramework);
            io.mapRequired("IsSystemModule", isSystem);
            io.mapRequired("Libraries", libs);
        }
    };

    // DeclId
    template <>
    struct MappingTraits<Meta::DeclId> {

        static void mapping(IO& io, Meta::DeclId& id)
        {
            io.mapRequired("Name", id.name);
            io.mapRequired("JsName", id.jsName);
            io.mapRequired("Filename", id.fileName);
            if (id.module != nullptr)
                io.mapRequired("Module", *id.module);
        }
    };

    // Type
    template <>
    struct MappingTraits<Meta::Type> {

        static void mapping(IO& io, Meta::Type& type)
        {
            Meta::TypeType typeType = type.getType();
            io.mapRequired("Type", typeType);

            switch (typeType) {
            case Meta::TypeType::TypeId: {
                Meta::IdTypeDetails& details = type.getDetailsAs<Meta::IdTypeDetails>();
                io.mapRequired("WithProtocols", details.protocols);
                break;
            }
            case Meta::TypeType::TypeConstantArray: {
                Meta::ConstantArrayTypeDetails& details = type.getDetailsAs<Meta::ConstantArrayTypeDetails>();
                io.mapRequired("ArrayType", details.innerType);
                io.mapRequired("Size", details.size);
                break;
            }
            case Meta::TypeType::TypeIncompleteArray: {
                Meta::IncompleteArrayTypeDetails& details = type.getDetailsAs<Meta::IncompleteArrayTypeDetails>();
                io.mapRequired("ArrayType", details.innerType);
                break;
            }
            case Meta::TypeType::TypeInterface: {
                Meta::InterfaceTypeDetails& details = type.getDetailsAs<Meta::InterfaceTypeDetails>();
                io.mapRequired("Id", details.id);
                io.mapRequired("WithProtocols", details.protocols);
                break;
            }
            case Meta::TypeType::TypeBridgedInterface: {
                Meta::BridgedInterfaceTypeDetails& details = type.getDetailsAs<Meta::BridgedInterfaceTypeDetails>();
                io.mapRequired("Id", details.id);
                break;
            }
            case Meta::TypeType::TypePointer: {
                Meta::PointerTypeDetails& details = type.getDetailsAs<Meta::PointerTypeDetails>();
                io.mapRequired("PointerType", details.innerType);
                break;
            }
            case Meta::TypeType::TypeFunctionPointer: {
                Meta::FunctionPointerTypeDetails& details = type.getDetailsAs<Meta::FunctionPointerTypeDetails>();
                io.mapRequired("Signature", details.signature);
                break;
            }
            case Meta::TypeType::TypeBlock: {
                Meta::BlockTypeDetails& details = type.getDetailsAs<Meta::BlockTypeDetails>();
                io.mapRequired("Signature", details.signature);
                break;
            }
            case Meta::TypeType::TypeStruct: {
                Meta::StructTypeDetails& details = type.getDetailsAs<Meta::StructTypeDetails>();
                std::string fullModuleName = details.id.module->getFullModuleName();
                io.mapRequired("Module", fullModuleName);
                io.mapRequired("Name", details.id.jsName);
                break;
            }
            case Meta::TypeType::TypeUnion: {
                Meta::UnionTypeDetails& details = type.getDetailsAs<Meta::UnionTypeDetails>();
                std::string fullModuleName = details.id.module->getFullModuleName();
                io.mapRequired("Module", fullModuleName);
                io.mapRequired("Name", details.id.jsName);
                break;
            }
            case Meta::TypeType::TypeAnonymousStruct: {
                Meta::AnonymousStructTypeDetails& details = type.getDetailsAs<Meta::AnonymousStructTypeDetails>();
                io.mapRequired("Fields", details.fields);
                break;
            }
            case Meta::TypeType::TypeAnonymousUnion: {
                Meta::AnonymousUnionTypeDetails& details = type.getDetailsAs<Meta::AnonymousUnionTypeDetails>();
                io.mapRequired("Fields", details.fields);
                break;
            }
            case Meta::TypeType::TypeEnum: {
                Meta::EnumTypeDetails& details = type.getDetailsAs<Meta::EnumTypeDetails>();
                io.mapRequired("UnderlyingType", details.underlyingType);
                io.mapRequired("Name", details.name.jsName);
                break;
            }
            default: {
            }
            }
        }
    };

    // BaseMeta
    template <>
    struct MappingTraits<BaseMeta> {
        static void mapping(IO& io, std::shared_ptr<Meta::Meta>& meta)
        {
            io.mapRequired("Id", meta->id);
            io.mapOptional("IntroducedIn", meta->introducedIn, UNKNOWN_VERSION);
            io.mapRequired("Flags", meta->flags);
            io.mapRequired("Type", meta->type);
        }
    };

    // shared_ptr<MethodMeta>
    template <>
    struct MappingTraits<std::shared_ptr<Meta::MethodMeta> > {

        static void mapping(IO& io, std::shared_ptr<Meta::MethodMeta>& meta)
        {
            std::shared_ptr<Meta::Meta> baseMeta = std::static_pointer_cast<Meta::MethodMeta>(meta);
            MappingTraits<BaseMeta>::mapping(io, baseMeta);
            io.mapRequired("Signature", meta->signature);
        }
    };

    // shared_ptr<PropertyMeta>
    template <>
    struct MappingTraits<std::shared_ptr<Meta::PropertyMeta> > {

        static void mapping(IO& io, std::shared_ptr<Meta::PropertyMeta>& meta)
        {
            std::shared_ptr<Meta::Meta> baseMeta = std::static_pointer_cast<Meta::PropertyMeta>(meta);
            MappingTraits<BaseMeta>::mapping(io, baseMeta);

            if (meta->getter)
                io.mapRequired("Getter", meta->getter);
            if (meta->setter)
                io.mapRequired("Setter", meta->setter);
        }
    };

    // shared_ptr<BaseClassMeta>
    template <>
    struct MappingTraits<std::shared_ptr<Meta::BaseClassMeta> > {

        static void mapping(IO& io, std::shared_ptr<Meta::BaseClassMeta>& meta)
        {
            std::shared_ptr<Meta::Meta> baseMeta = std::static_pointer_cast<Meta::Meta>(meta);
            MappingTraits<BaseMeta>::mapping(io, baseMeta);
            io.mapRequired("InstanceMethods", meta->instanceMethods);
            io.mapRequired("StaticMethods", meta->staticMethods);
            io.mapRequired("Properties", meta->properties);
            io.mapRequired("Protocols", meta->protocols);
        }
    };

    // shared_ptr<FunctionMeta>
    template <>
    struct MappingTraits<std::shared_ptr<Meta::FunctionMeta> > {

        static void mapping(IO& io, std::shared_ptr<Meta::FunctionMeta>& meta)
        {
            std::shared_ptr<Meta::Meta> baseMeta = std::static_pointer_cast<Meta::FunctionMeta>(meta);
            MappingTraits<BaseMeta>::mapping(io, baseMeta);
            io.mapRequired("Signature", meta->signature);
        }
    };

    // RecordField
    template <>
    struct MappingTraits<Meta::RecordField> {

        static void mapping(IO& io, Meta::RecordField& field)
        {
            io.mapRequired("Name", field.name);
            io.mapRequired("Signature", field.encoding);
        }
    };

    // shared_ptr<RecordMeta>
    template <>
    struct MappingTraits<std::shared_ptr<Meta::RecordMeta> > {

        static void mapping(IO& io, std::shared_ptr<Meta::RecordMeta>& meta)
        {
            std::shared_ptr<Meta::Meta> baseMeta = std::static_pointer_cast<Meta::RecordMeta>(meta);
            MappingTraits<BaseMeta>::mapping(io, baseMeta);
            io.mapRequired("Fields", meta->fields);
        }
    };

    // shared_ptr<StructMeta>
    template <>
    struct MappingTraits<std::shared_ptr<Meta::StructMeta> > {

        static void mapping(IO& io, std::shared_ptr<Meta::StructMeta>& meta)
        {
            std::shared_ptr<Meta::RecordMeta> baseRecordMeta = std::static_pointer_cast<Meta::StructMeta>(meta);
            MappingTraits<std::shared_ptr<Meta::RecordMeta> >::mapping(io, baseRecordMeta);
        }
    };

    // shared_ptr<UnionMeta>
    template <>
    struct MappingTraits<std::shared_ptr<Meta::UnionMeta> > {

        static void mapping(IO& io, std::shared_ptr<Meta::UnionMeta>& meta)
        {
            std::shared_ptr<Meta::RecordMeta> baseRecordMeta = std::static_pointer_cast<Meta::UnionMeta>(meta);
            MappingTraits<std::shared_ptr<Meta::RecordMeta> >::mapping(io, baseRecordMeta);
        }
    };

    // shared_ptr<VarMeta>
    template <>
    struct MappingTraits<std::shared_ptr<Meta::VarMeta> > {

        static void mapping(IO& io, std::shared_ptr<Meta::VarMeta>& meta)
        {
            std::shared_ptr<Meta::Meta> baseMeta = std::static_pointer_cast<Meta::VarMeta>(meta);
            MappingTraits<BaseMeta>::mapping(io, baseMeta);
            io.mapRequired("Signature", meta->signature);
        }
    };

    // shared_ptr<JsCodeMeta>
    template <>
    struct MappingTraits<std::shared_ptr<Meta::JsCodeMeta> > {

        static void mapping(IO& io, std::shared_ptr<Meta::JsCodeMeta>& meta)
        {
            std::shared_ptr<Meta::Meta> baseMeta = std::static_pointer_cast<Meta::JsCodeMeta>(meta);
            MappingTraits<BaseMeta>::mapping(io, baseMeta);
            io.mapRequired("JsCode", meta->jsCode);
        }
    };

    // shared_ptr<InterfaceMeta>
    template <>
    struct MappingTraits<std::shared_ptr<Meta::InterfaceMeta> > {

        static void mapping(IO& io, std::shared_ptr<Meta::InterfaceMeta>& meta)
        {
            std::shared_ptr<Meta::BaseClassMeta> baseClassMeta = std::static_pointer_cast<Meta::InterfaceMeta>(meta);
            MappingTraits<std::shared_ptr<Meta::BaseClassMeta> >::mapping(io, baseClassMeta);
            io.mapRequired("Base", meta->base);
        }
    };

    // shared_ptr<ProtocolMeta>
    template <>
    struct MappingTraits<std::shared_ptr<Meta::ProtocolMeta> > {

        static void mapping(IO& io, std::shared_ptr<Meta::ProtocolMeta>& meta)
        {
            std::shared_ptr<Meta::BaseClassMeta> baseClassMeta = std::static_pointer_cast<Meta::ProtocolMeta>(meta);
            MappingTraits<std::shared_ptr<Meta::BaseClassMeta> >::mapping(io, baseClassMeta);
        }
    };

    // shared_ptr<CategoryMeta>
    template <>
    struct MappingTraits<std::shared_ptr<Meta::CategoryMeta> > {

        static void mapping(IO& io, std::shared_ptr<Meta::CategoryMeta>& meta)
        {
            std::shared_ptr<Meta::BaseClassMeta> baseClassMeta = std::static_pointer_cast<Meta::CategoryMeta>(meta);
            MappingTraits<std::shared_ptr<Meta::BaseClassMeta> >::mapping(io, baseClassMeta);
            io.mapRequired("ExtendedInterface", meta->extendedInterface);
        }
    };

    // shared_ptr<Meta>
    // These traits check which is the actual run-time type of the meta and forward to the corresponding traits.
    template <>
    struct MappingTraits<std::shared_ptr<Meta::Meta> > {

        static void mapping(IO& io, std::shared_ptr<Meta::Meta>& meta)
        {
            switch (meta->type) {
            case Meta::MetaType::Function: {
                std::shared_ptr<Meta::FunctionMeta> function = std::static_pointer_cast<Meta::FunctionMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::FunctionMeta> >::mapping(io, function);
                break;
            }
            case Meta::MetaType::Struct: {
                std::shared_ptr<Meta::StructMeta> structMeta = std::static_pointer_cast<Meta::StructMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::StructMeta> >::mapping(io, structMeta);
                break;
            }
            case Meta::MetaType::Union: {
                std::shared_ptr<Meta::UnionMeta> unionMeta = std::static_pointer_cast<Meta::UnionMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::UnionMeta> >::mapping(io, unionMeta);
                break;
            }
            case Meta::MetaType::Var: {
                std::shared_ptr<Meta::VarMeta> var = std::static_pointer_cast<Meta::VarMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::VarMeta> >::mapping(io, var);
                break;
            }
            case Meta::MetaType::JsCode: {
                std::shared_ptr<Meta::JsCodeMeta> jsCode = std::static_pointer_cast<Meta::JsCodeMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::JsCodeMeta> >::mapping(io, jsCode);
                break;
            }
            case Meta::MetaType::Interface: {
                std::shared_ptr<Meta::InterfaceMeta> interface = std::static_pointer_cast<Meta::InterfaceMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::InterfaceMeta> >::mapping(io, interface);
                break;
            }
            case Meta::MetaType::Protocol: {
                std::shared_ptr<Meta::ProtocolMeta> protocol = std::static_pointer_cast<Meta::ProtocolMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::ProtocolMeta> >::mapping(io, protocol);
                break;
            }
            case Meta::MetaType::Category: {
                std::shared_ptr<Meta::CategoryMeta> category = std::static_pointer_cast<Meta::CategoryMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::CategoryMeta> >::mapping(io, category);
                break;
            }
            case Meta::MetaType::Method: {
                std::shared_ptr<Meta::MethodMeta> method = std::static_pointer_cast<Meta::MethodMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::MethodMeta> >::mapping(io, method);
                break;
            }
            case Meta::MetaType::Property: {
                std::shared_ptr<Meta::PropertyMeta> property = std::static_pointer_cast<Meta::PropertyMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::PropertyMeta> >::mapping(io, property);
                break;
            }
            case Meta::MetaType::Undefined:
            default: {
                throw std::runtime_error("Unknown type of meta object.");
            }
            }
        }
    };
}
}
