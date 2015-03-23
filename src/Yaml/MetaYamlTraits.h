#pragma once

#include "../Meta/MetaEntities.h"

//TODO: Move these definitions (and others) in the .cpp file
namespace llvm {
    namespace yaml {
        bool operator ==(Meta::Version& x, const Meta::Version& y) {
            return x.Major == y.Major && x.Minor == y.Minor && x.SubMinor == y.SubMinor;
        }

        Meta::MetaFlags operator | (Meta::MetaFlags& value1, Meta::MetaFlags value2) {
            return (Meta::MetaFlags)((uint32_t)value1 | (uint32_t)value2);
        }
    }
}

#include <llvm/Support/YAMLTraits.h>

LLVM_YAML_IS_SEQUENCE_VECTOR(std::string)
LLVM_YAML_IS_SEQUENCE_VECTOR(Meta::FQName)
LLVM_YAML_IS_SEQUENCE_VECTOR(Meta::RecordField)
LLVM_YAML_IS_SEQUENCE_VECTOR(std::shared_ptr<Meta::Meta>)
LLVM_YAML_IS_SEQUENCE_VECTOR(std::shared_ptr<Meta::MethodMeta>)
LLVM_YAML_IS_SEQUENCE_VECTOR(std::shared_ptr<Meta::PropertyMeta>)
LLVM_YAML_IS_SEQUENCE_VECTOR(Meta::TypeEncoding)
LLVM_YAML_STRONG_TYPEDEF(std::shared_ptr<Meta::Meta>, BaseMeta)
LLVM_YAML_STRONG_TYPEDEF(std::shared_ptr<Meta::TypeEncoding>, BaseTypeEncoding)

namespace llvm {
    namespace yaml {

        // Module
        template <>
        struct MappingTraits<Meta::Module> {

            class NormalizedModule {
            public:
                NormalizedModule(IO& io)
                        : name(), declarations() { }

                NormalizedModule(IO& io, Meta::Module& module)
                        : name(module.getName()),
                          declarations(module.getDeclarations()) {}

                Meta::Module denormalize(IO &io) { return Meta::Module(name, this->declarations); }

                std::string name;
                std::vector<std::shared_ptr<Meta::Meta>> declarations;
            };

            static void mapping(IO &io, Meta::Module &module) {
                MappingNormalization<NormalizedModule, Meta::Module> keys(io, module);

                io.mapRequired("name", keys->name);
                io.mapRequired("items", keys->declarations);
            }
        };

        // Version
        template <>
        struct ScalarTraits<Meta::Version> {
            static void output(const Meta::Version &value, void *context, raw_ostream &out) {
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

            static StringRef input(StringRef stringValue, void *context, Meta::Version &value) {
                value = UNKNOWN_VERSION;
                if(stringValue.size() == 0) {
                    return StringRef();
                }
                std::string version = stringValue.str();

                unsigned long firstDotIndex = version.find(".");
                value.Major = (firstDotIndex != std::string::npos) ? std::stoi(version.substr(0, firstDotIndex)) : std::stoi(version);
                if(firstDotIndex != std::string::npos) {
                    unsigned long secondDotIndex = version.find(".", firstDotIndex + 1);
                    value.Minor = std::stoi(version.substr(firstDotIndex + 1, (secondDotIndex != std::string::npos) ? secondDotIndex - firstDotIndex - 1 : std::string::npos));
                    if(secondDotIndex != std::string::npos) {
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

            static void bitset(IO& io, Meta::MetaFlags& value) {

                io.bitSetCase(value, "HasName",  Meta::MetaFlags::HasName);
                io.bitSetCase(value, "IsIosAppExtensionAvailable",    Meta::MetaFlags::IsIosAppExtensionAvailable);

                io.bitSetCase(value, "FunctionIsVariadic", Meta::MetaFlags::FunctionIsVariadic);
                io.bitSetCase(value, "FunctionOwnsReturnedCocoaObject",  Meta::MetaFlags::FunctionOwnsReturnedCocoaObject);

                io.bitSetCase(value, "MethodIsVariadic",  Meta::MetaFlags::MethodIsVariadic);
                io.bitSetCase(value, "MethodIsNullTerminatedVariadic",  Meta::MetaFlags::MethodIsNullTerminatedVariadic);
                io.bitSetCase(value, "MethodOwnsReturnedCocoaObject",  Meta::MetaFlags::MethodOwnsReturnedCocoaObject);

                io.bitSetCase(value, "PropertyHasGetter",  Meta::MetaFlags::PropertyHasGetter);
                io.bitSetCase(value, "PropertyHasSetter",  Meta::MetaFlags::PropertyHasSetter);
            }
        };

        // MetaType
        template <>
        struct ScalarEnumerationTraits<Meta::MetaType> {
            static void enumeration(IO &io, Meta::MetaType &value) {
                io.enumCase(value, "Undefined", Meta::MetaType::Undefined);
                io.enumCase(value, "Struct", Meta::MetaType::Struct);
                io.enumCase(value, "Union", Meta::MetaType::Union);
                io.enumCase(value, "Function", Meta::MetaType::Function);
                io.enumCase(value, "JsCode", Meta::MetaType::JsCode);
                io.enumCase(value, "Var", Meta::MetaType::Var);
                io.enumCase(value, "Interface", Meta::MetaType::Interface);
                io.enumCase(value, "Protocol", Meta::MetaType::Protocol);
                io.enumCase(value, "Category", Meta::MetaType::Category);
            }
        };

        // FQName
        template <>
        struct MappingTraits<Meta::FQName> {

            static void mapping(IO &io, Meta::FQName& name) {
                io.mapRequired("Module", name.module);
                io.mapOptional("Name", name.jsName);
            }
        };

        // shared_ptr<TypeEncoding>
        template <>
        struct MappingTraits<std::shared_ptr<Meta::TypeEncoding>> {

            static void mapping(IO &io, std::shared_ptr<Meta::TypeEncoding>& type) {
                std::string test = "[type]";
                io.mapRequired("Type", test);
            }
        };

        // TypeEncoding
        template <>
        struct MappingTraits<Meta::TypeEncoding> {

            static void mapping(IO &io, Meta::TypeEncoding& type) {
                Meta::TypeEncoding *typePtr = &type;
                std::string typeName = "[type]";
                io.mapRequired("Type", typeName);
//                if(Meta::UnknownEncoding *encoding = dynamic_cast<Meta::UnknownEncoding*>(typePtr))
//                    typeName = "Unknown";
//                if(Meta::VaListEncoding *encoding = dynamic_cast<Meta::VaListEncoding*>(typePtr))
//                    typeName = "VaList";
//                if(Meta::ProtocolEncoding *encoding = dynamic_cast<Meta::ProtocolEncoding*>(typePtr))
//                    typeName = "Protocol";
//                if(Meta::VoidEncoding *encoding = dynamic_cast<Meta::VoidEncoding*>(typePtr))
//                    typeName = "Void";
//                if(Meta::BoolEncoding *encoding = dynamic_cast<Meta::BoolEncoding*>(typePtr))
//                    typeName = "Bool";
//                if(Meta::ShortEncoding *encoding = dynamic_cast<Meta::ShortEncoding*>(typePtr))
//                    typeName = "Short";
//                if(Meta::UShortEncoding *encoding = dynamic_cast<Meta::UShortEncoding*>(typePtr))
//                    typeName = "Ushort";
//                if(Meta::IntEncoding *encoding = dynamic_cast<Meta::IntEncoding*>(typePtr))
//                    typeName = "Int";
//                if(Meta::UIntEncoding *encoding = dynamic_cast<Meta::UIntEncoding*>(typePtr))
//                    typeName = "UInt";
//                if(Meta::LongEncoding *encoding = dynamic_cast<Meta::LongEncoding*>(typePtr))
//                    typeName = "Long";
//                if(Meta::ULongEncoding *encoding = dynamic_cast<Meta::ULongEncoding*>(typePtr))
//                    typeName = "ULong";
//                if(Meta::LongLongEncoding *encoding = dynamic_cast<Meta::LongLongEncoding*>(typePtr))
//                    typeName = "LongLong";
//                if(Meta::ULongLongEncoding *encoding = dynamic_cast<Meta::ULongLongEncoding*>(typePtr))
//                    typeName = "ULongLong";
//                if(Meta::UnsignedCharEncoding *encoding = dynamic_cast<Meta::UnsignedCharEncoding*>(typePtr))
//                    typeName = "UChar";
//                if(Meta::UnicharEncoding *encoding = dynamic_cast<Meta::UnicharEncoding*>(typePtr))
//                    typeName = "Unichar";
//                if(Meta::SignedCharEncoding *encoding = dynamic_cast<Meta::SignedCharEncoding*>(typePtr))
//                    typeName = "CharS";
//                if(Meta::CStringEncoding *encoding = dynamic_cast<Meta::CStringEncoding*>(typePtr))
//                    typeName = "CString";
//                if(Meta::FloatEncoding *encoding = dynamic_cast<Meta::FloatEncoding*>(typePtr))
//                    typeName = "Float";
//                if(Meta::DoubleEncoding *encoding = dynamic_cast<Meta::DoubleEncoding*>(typePtr))
//                    typeName = "Double";
//                if(Meta::SelectorEncoding *encoding = dynamic_cast<Meta::SelectorEncoding*>(typePtr))
//                    typeName = "Selector";
//                if(Meta::ClassEncoding *encoding = dynamic_cast<Meta::ClassEncoding*>(typePtr))
//                    typeName = "Class";
//                if(Meta::InstancetypeEncoding *encoding = dynamic_cast<Meta::InstancetypeEncoding*>(typePtr))
//                    typeName = "Instancetype";
//                if(Meta::IdEncoding *encoding = dynamic_cast<Meta::IdEncoding*>(typePtr)) {
//                    typeName = "Id";
//                }
//                io.mapRequired("Type", typeName);
            }
        };

        // BaseMeta
        template <>
        struct MappingTraits<BaseMeta> {

            static void mapping(IO &io, std::shared_ptr<Meta::Meta>& meta) {
                io.mapRequired("Name", meta->name);
                io.mapRequired("JsName", meta->jsName);
                io.mapOptional("Module", meta->module, std::string(""));
                io.mapOptional("IntroducedIn", meta->introducedIn, UNKNOWN_VERSION);
                io.mapRequired("Flags", meta->flags);
                io.mapRequired("Type", meta->type);
            }
        };

        // shared_ptr<MethodMeta>
        template <>
        struct MappingTraits<std::shared_ptr<Meta::MethodMeta>> {

            static void mapping(IO &io, std::shared_ptr<Meta::MethodMeta>& meta) {
                std::shared_ptr<Meta::Meta> baseMeta = std::static_pointer_cast<Meta::MethodMeta>(meta);
                MappingTraits<BaseMeta>::mapping(io, baseMeta);
                io.mapRequired("Selctor", meta->selector);
                io.mapRequired("Signature", meta->signature);
                io.mapRequired("TypeEncoding", meta->typeEncoding);
            }
        };

        // shared_ptr<PropertyMeta>
        template <>
        struct MappingTraits<std::shared_ptr<Meta::PropertyMeta>> {

            static void mapping(IO &io, std::shared_ptr<Meta::PropertyMeta>& meta) {
                std::shared_ptr<Meta::Meta> baseMeta = std::static_pointer_cast<Meta::PropertyMeta>(meta);
                MappingTraits<BaseMeta>::mapping(io, baseMeta);

                if(meta->getter)
                    io.mapRequired("Getter", meta->getter);
                if(meta->setter)
                    io.mapRequired("Setter", meta->setter);
            }
        };

        // shared_ptr<BaseClassMeta>
        template <>
        struct MappingTraits<std::shared_ptr<Meta::BaseClassMeta>> {

            static void mapping(IO &io, std::shared_ptr<Meta::BaseClassMeta>& meta) {
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
        struct MappingTraits<std::shared_ptr<Meta::FunctionMeta>> {

            static void mapping(IO &io, std::shared_ptr<Meta::FunctionMeta>& meta) {
                std::shared_ptr<Meta::Meta> baseMeta = std::static_pointer_cast<Meta::FunctionMeta>(meta);
                MappingTraits<BaseMeta>::mapping(io, baseMeta);
                io.mapRequired("Signature", meta->signature);
            }
        };

        // RecordField
        template <>
        struct MappingTraits<Meta::RecordField> {

            static void mapping(IO &io, Meta::RecordField& field) {
                io.mapRequired("Name", field.name);
                io.mapRequired("Signature", field.encoding);
            }
        };

        // shared_ptr<RecordMeta>
        template <>
        struct MappingTraits<std::shared_ptr<Meta::RecordMeta>> {

            static void mapping(IO &io, std::shared_ptr<Meta::RecordMeta>& meta) {
                std::shared_ptr<Meta::Meta> baseMeta = std::static_pointer_cast<Meta::RecordMeta>(meta);
                MappingTraits<BaseMeta>::mapping(io, baseMeta);
                io.mapRequired("Fields", meta->fields);
            }
        };

        // shared_ptr<StructMeta>
        template <>
        struct MappingTraits<std::shared_ptr<Meta::StructMeta>> {

            static void mapping(IO &io, std::shared_ptr<Meta::StructMeta>& meta) {
                std::shared_ptr<Meta::RecordMeta> baseRecordMeta = std::static_pointer_cast<Meta::StructMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::RecordMeta>>::mapping(io, baseRecordMeta);
            }
        };

        // shared_ptr<UnionMeta>
        template <>
        struct MappingTraits<std::shared_ptr<Meta::UnionMeta>> {

            static void mapping(IO &io, std::shared_ptr<Meta::UnionMeta>& meta) {
                std::shared_ptr<Meta::RecordMeta> baseRecordMeta = std::static_pointer_cast<Meta::UnionMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::RecordMeta>>::mapping(io, baseRecordMeta);
            }
        };

        // shared_ptr<VarMeta>
        template <>
        struct MappingTraits<std::shared_ptr<Meta::VarMeta>> {

            static void mapping(IO &io, std::shared_ptr<Meta::VarMeta>& meta) {
                std::shared_ptr<Meta::Meta> baseMeta = std::static_pointer_cast<Meta::VarMeta>(meta);
                MappingTraits<BaseMeta>::mapping(io, baseMeta);
                io.mapRequired("Signature", meta->signature);
            }
        };

        // shared_ptr<JsCodeMeta>
        template <>
        struct MappingTraits<std::shared_ptr<Meta::JsCodeMeta>> {

            static void mapping(IO &io, std::shared_ptr<Meta::JsCodeMeta>& meta) {
                std::shared_ptr<Meta::Meta> baseMeta = std::static_pointer_cast<Meta::JsCodeMeta>(meta);
                MappingTraits<BaseMeta>::mapping(io, baseMeta);
                io.mapRequired("JsCode", meta->jsCode);
            }
        };

        // shared_ptr<InterfaceMeta>
        template <>
        struct MappingTraits<std::shared_ptr<Meta::InterfaceMeta>> {

            static void mapping(IO &io, std::shared_ptr<Meta::InterfaceMeta>& meta) {
                std::shared_ptr<Meta::BaseClassMeta> baseClassMeta = std::static_pointer_cast<Meta::InterfaceMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::BaseClassMeta>>::mapping(io, baseClassMeta);
                io.mapRequired("Base", meta->baseName);
            }
        };

        // shared_ptr<ProtocolMeta>
        template <>
        struct MappingTraits<std::shared_ptr<Meta::ProtocolMeta>> {

            static void mapping(IO &io, std::shared_ptr<Meta::ProtocolMeta>& meta) {
                std::shared_ptr<Meta::BaseClassMeta> baseClassMeta = std::static_pointer_cast<Meta::ProtocolMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::BaseClassMeta>>::mapping(io, baseClassMeta);
            }
        };

        // shared_ptr<CategoryMeta>
        template <>
        struct MappingTraits<std::shared_ptr<Meta::CategoryMeta>> {

            static void mapping(IO &io, std::shared_ptr<Meta::CategoryMeta>& meta) {
                std::shared_ptr<Meta::BaseClassMeta> baseClassMeta = std::static_pointer_cast<Meta::CategoryMeta>(meta);
                MappingTraits<std::shared_ptr<Meta::BaseClassMeta>>::mapping(io, baseClassMeta);
            }
        };

        // shared_ptr<Meta>
        // These traits check which is the actual run-time type of the meta and forward to the corresponding traits.
        template <>
        struct MappingTraits<std::shared_ptr<Meta::Meta>> {

            static void mapping(IO &io, std::shared_ptr<Meta::Meta>& meta) {
                switch(meta->type) {
                    case Meta::MetaType::Function : {
                        std::shared_ptr<Meta::FunctionMeta> function = std::static_pointer_cast<Meta::FunctionMeta>(meta);
                        MappingTraits<std::shared_ptr<Meta::FunctionMeta>>::mapping(io, function);
                        break;
                    }
                    case Meta::MetaType::Struct : {
                        std::shared_ptr<Meta::StructMeta> structMeta = std::static_pointer_cast<Meta::StructMeta>(meta);
                        MappingTraits<std::shared_ptr<Meta::StructMeta>>::mapping(io, structMeta);
                        break;
                    }
                    case Meta::MetaType::Union : {
                        std::shared_ptr<Meta::UnionMeta> unionMeta = std::static_pointer_cast<Meta::UnionMeta>(meta);
                        MappingTraits<std::shared_ptr<Meta::UnionMeta>>::mapping(io, unionMeta);
                        break;
                    }
                    case Meta::MetaType::Var : {
                        std::shared_ptr<Meta::VarMeta> var = std::static_pointer_cast<Meta::VarMeta>(meta);
                        MappingTraits<std::shared_ptr<Meta::VarMeta>>::mapping(io, var);
                        break;
                    }
                    case Meta::MetaType::JsCode : {
                        std::shared_ptr<Meta::JsCodeMeta> jsCode = std::static_pointer_cast<Meta::JsCodeMeta>(meta);
                        MappingTraits<std::shared_ptr<Meta::JsCodeMeta>>::mapping(io, jsCode);
                        break;
                    }
                    case Meta::MetaType::Interface : {
                        std::shared_ptr<Meta::InterfaceMeta> interface = std::static_pointer_cast<Meta::InterfaceMeta>(meta);
                        MappingTraits<std::shared_ptr<Meta::InterfaceMeta>>::mapping(io, interface);
                        break;
                    }
                    case Meta::MetaType::Protocol : {
                        std::shared_ptr<Meta::ProtocolMeta> protocol = std::static_pointer_cast<Meta::ProtocolMeta>(meta);
                        MappingTraits<std::shared_ptr<Meta::ProtocolMeta>>::mapping(io, protocol);
                        break;
                    }
                    case Meta::MetaType::Category : {
                        std::shared_ptr<Meta::CategoryMeta> category = std::static_pointer_cast<Meta::CategoryMeta>(meta);
                        MappingTraits<std::shared_ptr<Meta::CategoryMeta>>::mapping(io, category);
                        break;
                    }
                    case Meta::MetaType::Undefined :
                    default : {
                        throw std::runtime_error("Unknown type of meta object.");
                    }
                }
            }
        };
    }
}
