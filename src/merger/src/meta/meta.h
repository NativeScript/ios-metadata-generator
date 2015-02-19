#ifndef META_H
#define META_H

#include <string>
#include <vector>
#include "../typeEncoding/typeEncoding.h"
#include "../utils/serializer.h"

using namespace std;

struct Version {
    int Major;
    int Minor;
    int SubMinor;
};

#define UNKNOWN_VERSION { -1, -1, -1 };

namespace meta {
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

    enum SymbolType {
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
        virtual ~Meta() { }

        SymbolType type = SymbolType::Undefined;
        MetaFlags flags = MetaFlags::None;

        string name;
        string jsName;
        string module;

        // Availability
        Version introducedIn = UNKNOWN_VERSION;
        Version obsoletedIn = UNKNOWN_VERSION;
        Version deprecatedIn = UNKNOWN_VERSION;

        // visitors
        virtual void serialize(utils::Serializer* serializer) = 0;

        friend void swapMeta(Meta& lhs, Meta& rhs);
        void swap(Meta& lhs, Meta& rhs) {
            swapMeta(lhs, rhs);
        }
    };

    class MethodMeta : public Meta {
    public:
        MethodMeta() : Meta() { }

        MethodMeta(MethodMeta&& other) :
            Meta(other),
            selector(std::move(other.selector)),
            typeEncoding(other.typeEncoding),
            signature(std::move(other.signature)) { }

        string selector;
        string typeEncoding;
        vector<std::unique_ptr<typeEncoding::TypeEncoding>> signature;

        virtual void serialize(utils::Serializer* serializer) override;

        MethodMeta& operator=(MethodMeta other)
        {
            swapMethodMeta(*this, other);
            return *this;
        }

        friend void swapMethodMeta(MethodMeta& lhs, MethodMeta& rhs);
        void swap(MethodMeta& lhs, MethodMeta& rhs) {
            swapMethodMeta(lhs, rhs);
        }
    };

    class PropertyMeta : public Meta {
    public:
        PropertyMeta() : Meta() { }

        PropertyMeta(PropertyMeta&& other) :
            Meta(other),
            getter(std::move(other.getter)),
            setter(std::move(other.setter)) { }

        std::unique_ptr<MethodMeta> getter;
        std::unique_ptr<MethodMeta> setter;

        virtual void serialize(utils::Serializer* serializer) override;

        PropertyMeta& operator=(PropertyMeta other)
        {
            swapPropertyMeta(*this, other);
            return *this;
        }

        friend void swapPropertyMeta(PropertyMeta& lhs, PropertyMeta& rhs);
        void swap(PropertyMeta& lhs, PropertyMeta& rhs) {
            swapPropertyMeta(lhs, rhs);
        }
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
            this->type = SymbolType::Category;
        }

        FQName extendedInterface;

        virtual void serialize(utils::Serializer* serializer) override;
    };

    class InterfaceMeta : public BaseClassMeta {
    public:
        InterfaceMeta() {
            this->type = SymbolType::Interface;
        }

        FQName baseName;

        virtual void serialize(utils::Serializer* serializer) override;
    };

    class ProtocolMeta : public BaseClassMeta {
    public:
        ProtocolMeta() {
            this->type = SymbolType::Protocol;
        }

        virtual void serialize(utils::Serializer* serializer) override;
    };

    struct RecordField {
    public:
        std::string name;
        std::unique_ptr<typeEncoding::TypeEncoding> encoding;
    };

    class RecordMeta : public Meta {
    public:
        vector<RecordField> fields;
    };

    class StructMeta : public RecordMeta {
    public:
        StructMeta() {
            this->type = SymbolType::Struct;
        }

        virtual void serialize(utils::Serializer* serializer) override;
    };

    class UnionMeta : public RecordMeta {
    public:
        UnionMeta() {
            this->type = SymbolType::Union;
        }

        virtual void serialize(utils::Serializer* serializer) override;
    };

    class FunctionMeta : public Meta {
    public:
        FunctionMeta() {
            this->type = SymbolType::Function;
        }
        vector<std::unique_ptr<typeEncoding::TypeEncoding>> signature;

        virtual void serialize(utils::Serializer* serializer) override;
    };

    class JsCodeMeta : public Meta {
    public:
        JsCodeMeta() {
            this->type = SymbolType::JsCode;
        }
        string jsCode;

        virtual void serialize(utils::Serializer* serializer) override;
    };

    class VarMeta : public Meta {
    public:
        VarMeta() {
            this->type = SymbolType::Var;
        }
        std::unique_ptr<typeEncoding::TypeEncoding> signature;

        virtual void serialize(utils::Serializer* serializer) override;
    };
}

#endif