#pragma once

#include <string>
#include <vector>
#include "TypeEntities.h"

#define UNKNOWN_VERSION { -1, -1, -1 }

namespace Meta {

    struct Version {
        int Major;
        int Minor;
        int SubMinor;
    };

    // TODO: Change values (and maybe rename) some of the flag values
    // TODO: Change binary conversation of the flags not to depend on the actual integral value of the flags.
    enum MetaFlags : uint16_t {
        // Common
        None                                  = 0,
        HasName                               = 1 << 1,
        IsIosAppExtensionAvailable            = 1 << 2,
        // Function
        FunctionIsVariadic                    = 1 << 3,
        FunctionOwnsReturnedCocoaObject       = 1 << 4,
        // Method
        MethodIsVariadic                      = 1 << 5,
        MethodIsNullTerminatedVariadic        = 1 << 6,
        MethodOwnsReturnedCocoaObject         = 1 << 7,
        // Property
        PropertyHasGetter                     = 1 << 8,
        PropertyHasSetter                     = 1 << 9,

        // TODO: remove these flags
        MemberIsLocalJsNameDuplicate          = 1 << 10,
        MemberHasJsNameDuplicateInHierarchy   = 1 << 11,
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
        Category,
        Method,
        Property
    };

    class Meta {
    public:
        MetaType type = MetaType::Undefined;
        MetaFlags flags = MetaFlags::None;

        std::string name;
        std::string jsName;
        std::string module;

        // Availability
        Version introducedIn = UNKNOWN_VERSION;
        Version obsoletedIn = UNKNOWN_VERSION;
        Version deprecatedIn = UNKNOWN_VERSION;

        clang::Decl *declaration;

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
        MethodMeta() : Meta() {
            this->type = MetaType::Method;
        }

        std::string selector;
        std::string typeEncoding;
        std::vector<Type> signature;

        //virtual void serialize(utils::Serializer* serializer) override;
    };

    class PropertyMeta : public Meta {
    public:
        PropertyMeta() : Meta() {
            this->type = MetaType::Property;
        }

        std::shared_ptr<MethodMeta> getter;
        std::shared_ptr<MethodMeta> setter;

        //virtual void serialize(utils::Serializer* serializer) override;
    };

    class BaseClassMeta : public Meta {
    public:
        std::vector<std::shared_ptr<MethodMeta>> instanceMethods;
        std::vector<std::shared_ptr<MethodMeta>> staticMethods;
        std::vector<std::shared_ptr<PropertyMeta>> properties;
        std::vector<FQName> protocols;
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

    class RecordMeta : public Meta {
    public:
        std::vector<RecordField> fields;
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
        std::vector<Type> signature;

        //virtual void serialize(utils::Serializer* serializer) override;
    };

    class JsCodeMeta : public Meta {
    public:
        JsCodeMeta() {
            this->type = MetaType::JsCode;
        }
        std::string jsCode;

        //virtual void serialize(utils::Serializer* serializer) override;
    };

    class VarMeta : public Meta {
    public:
        VarMeta() {
            this->type = MetaType::Var;
        }
        Type signature;

        //virtual void serialize(utils::Serializer* serializer) override;
    };

    class Module {
    public:
        typedef std::vector<std::shared_ptr<Meta>>::iterator iterator;
        typedef std::vector<std::shared_ptr<Meta>>::const_iterator const_iterator;
        typedef std::vector<std::shared_ptr<Meta>>::size_type size_type;

        Module(std::string name)
                : _name(name) {}

        Module(std::string name, std::vector<std::shared_ptr<Meta>>& declarations)
                : _name(name),
                  _declarations(declarations) {}

        Module::iterator begin() { return _declarations.begin(); }
        Module::const_iterator begin() const { return _declarations.begin(); }
        Module::iterator end() { return _declarations.end(); }
        Module::const_iterator end() const { return _declarations.end(); }

        std::string getName() { return _name; }
        std::vector<std::shared_ptr<Meta>> getDeclarations() { return _declarations; }
        Module::size_type size() { return _declarations.size(); }
        void push_back(std::shared_ptr<Meta> meta) { _declarations.push_back(meta); }

    private:
        std::string _name;
        std::vector<std::shared_ptr<Meta>> _declarations;
    };
}