#pragma once

#include <string>
#include <vector>
#include "TypeEncodingEntities.h"

using namespace std;

#define UNKNOWN_VERSION { -1, -1, -1 };

namespace Meta {

    struct Version {
        int Major;
        int Minor;
        int SubMinor;
    };

    // TODO: Change values (and maybe rename) some of the flag values
    enum MetaFlags : uint8_t {
        None = 0,

        HasName = 1 << 7,
        IsIosAppExtensionAvailable = 1 << 6,

        FunctionIsVariadic = 1 << 5,
        FunctionOwnsReturnedCocoaObject = 1 << 4,

        MemberIsLocalJsNameDuplicate = 1 << 0,
        MemberHasJsNameDuplicateInHierarchy = 1 << 1,

        MethodIsVariadic = 1 << 2,
        MethodIsNullTerminatedVariadic = 1 << 3,
        MethodOwnsReturnedCocoaObject = 1 << 4,

        PropertyHasGetter = 1 << 2,
        PropertyHasSetter = 1 << 3
    };

    enum MetaType {
        Undefined = 0,
        Struct,
        Union,
        Function,
        JsCode,
        Var,
        Interface,
        Protocol,
        Category
    };

    class Meta {
    public:
        MetaType type = MetaType::Undefined;
        MetaFlags flags = MetaFlags::None;

        string name;
        string jsName;
        string module;

        // Availability
        Version introducedIn = UNKNOWN_VERSION;
        Version obsoletedIn = UNKNOWN_VERSION;
        Version deprecatedIn = UNKNOWN_VERSION;

        // visitors
        //virtual void serialize(utils::Serializer* serializer) = 0;

        bool getFlags(MetaFlags flags) {
            return (this->flags & flags) == flags;
        }

        void setFlags(MetaFlags flags, bool value) {
            value ? this->flags = (MetaFlags)(this->flags | flags) : this->flags = (MetaFlags)(this->flags & ~flags);
        }
    };

    class MethodMeta : public Meta {
    public:
        MethodMeta() : Meta() { }

        string selector;
        string typeEncoding;
        vector<TypeEncoding> signature;

        //virtual void serialize(utils::Serializer* serializer) override;
    };

    class PropertyMeta : public Meta {
    public:
        PropertyMeta() : Meta() { }

        std::unique_ptr<MethodMeta> getter;
        std::unique_ptr<MethodMeta> setter;

        //virtual void serialize(utils::Serializer* serializer) override;
    };

    class BaseClassMeta : public Meta {
    public:
        vector<MethodMeta> instanceMethods;
        vector<MethodMeta> staticMethods;
        vector<PropertyMeta> properties;
        vector<FQName> protocols;
    };

    class CategoryMeta : public BaseClassMeta {
    public:
        CategoryMeta() {
            this->type = MetaType::Category;
        }

        FQName extendedInterface;

        //virtual void serialize(utils::Serializer* serializer) override;
    };

    class InterfaceMeta : public BaseClassMeta {
    public:
        InterfaceMeta() {
            this->type = MetaType::Interface;
        }

        FQName baseName;

        //virtual void serialize(utils::Serializer* serializer) override;
    };

    class ProtocolMeta : public BaseClassMeta {
    public:
        ProtocolMeta() {
            this->type = MetaType::Protocol;
        }

        //virtual void serialize(utils::Serializer* serializer) override;
    };

    struct RecordField {
    public:
        std::string name;
        TypeEncoding encoding;
    };

    class RecordMeta : public Meta {
    public:
        vector<RecordField> fields;
    };

    class StructMeta : public RecordMeta {
    public:
        StructMeta() {
            this->type = MetaType::Struct;
        }

        //virtual void serialize(utils::Serializer* serializer) override;
    };

    class UnionMeta : public RecordMeta {
    public:
        UnionMeta() {
            this->type = MetaType::Union;
        }

        //virtual void serialize(utils::Serializer* serializer) override;
    };

    class FunctionMeta : public Meta {
    public:
        FunctionMeta() {
            this->type = MetaType::Function;
        }
        vector<TypeEncoding> signature;

        //virtual void serialize(utils::Serializer* serializer) override;
    };

    class JsCodeMeta : public Meta {
    public:
        JsCodeMeta() {
            this->type = MetaType::JsCode;
        }
        string jsCode;

        //virtual void serialize(utils::Serializer* serializer) override;
    };

    class VarMeta : public Meta {
    public:
        VarMeta() {
            this->type = MetaType::Var;
        }
        TypeEncoding signature;

        //virtual void serialize(utils::Serializer* serializer) override;
    };
}