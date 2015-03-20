#pragma once

#include <string>
#include <vector>
#include "IdentifierGenerator.h"

namespace Meta {

    class TypeEncoding { };

    // primitive types
    class UnknownEncoding : public TypeEncoding { };
    class VoidEncoding : public TypeEncoding { };
    class BoolEncoding : public TypeEncoding { };
    class ShortEncoding : public TypeEncoding { };
    class UShortEncoding : public TypeEncoding { };
    class IntEncoding : public TypeEncoding { };
    class UIntEncoding : public TypeEncoding { };
    class LongEncoding : public TypeEncoding { };
    class ULongEncoding : public TypeEncoding { };
    class LongLongEncoding : public TypeEncoding { };
    class ULongLongEncoding : public TypeEncoding { };
    class SignedCharEncoding : public TypeEncoding { };
    class UnsignedCharEncoding : public TypeEncoding { };
    class UnicharEncoding : public TypeEncoding { };
    class CStringEncoding : public TypeEncoding { };
    class FloatEncoding : public TypeEncoding { };
    class DoubleEncoding : public TypeEncoding { };

    class VaListEncoding : public TypeEncoding { };
    class SelectorEncoding : public TypeEncoding { };
    class InstancetypeEncoding : public TypeEncoding { };
    class ProtocolEncoding : public TypeEncoding { };

    class IdEncoding : public TypeEncoding {
    public:
        IdEncoding() : IdEncoding(std::vector<FQName>()) {}

        IdEncoding(const std::vector<FQName>& protocols)
        : protocols(protocols) {}

        std::vector<FQName> protocols;
    };

    class ClassEncoding : public TypeEncoding {
    public:
        ClassEncoding() : ClassEncoding(std::vector<FQName>()) {}

        ClassEncoding(const std::vector<FQName>& protocols)
                : protocols(protocols) {}

        std::vector<FQName> protocols;
    };

    class ArrayEncoding : public TypeEncoding {
    public:
        ArrayEncoding(const TypeEncoding& elementType)
                : elementType(elementType) {}

        TypeEncoding elementType;
    };

    class ConstantArrayEncoding : public ArrayEncoding {
    public:
        ConstantArrayEncoding(const TypeEncoding& elementType, int size)
                : ArrayEncoding(elementType),
                  size(size) {}
        int size;
    };

    class IncompleteArrayEncoding : public ArrayEncoding {
    public:
        IncompleteArrayEncoding(const TypeEncoding& elementType)
                : ArrayEncoding(elementType) {}
    };

    class InterfaceEncoding : public TypeEncoding {
    public:
        InterfaceEncoding(const FQName& name)
                : InterfaceEncoding(name, std::vector<FQName>()) {}

        InterfaceEncoding(const FQName& name, const std::vector<FQName>& protocols)
                : name(name),
                  protocols(protocols) {}

        FQName name;
        std::vector<FQName> protocols;
    };

    class PointerEncoding : public TypeEncoding {
    public:
        PointerEncoding(const TypeEncoding& target)
                : target(target) {}

        TypeEncoding target;
    };

    class BlockEncoding : public TypeEncoding {
    public:
        BlockEncoding(std::vector<TypeEncoding>& signature)
                : signature(signature) {}

        std::vector<TypeEncoding> signature;
    };

    class FunctionPointerEncoding : public TypeEncoding {
    public:
        FunctionPointerEncoding(std::vector<TypeEncoding>& signature)
                : signature(signature) {}

        std::vector<TypeEncoding> signature;
    };

    class StructEncoding : public TypeEncoding {
    public:
        StructEncoding(const FQName& name)
                : name(name) {}

        FQName name;
    };

    class UnionEncoding : public TypeEncoding {
    public:
        UnionEncoding(const FQName& name)
                : name(name) {}

        FQName name;
    };

    // TODO: Remove this type. It is redundant and is never used.
    class InterfaceDeclarationEncoding : public TypeEncoding {
    public:
        InterfaceDeclarationEncoding(const FQName& name)
                : name(name) {}

        FQName name;
    };

    class AnonymousRecordEncoding : public TypeEncoding {
    public:
        AnonymousRecordEncoding(const std::vector<std::string>& fieldNames, const std::vector<TypeEncoding>& fieldEncodings)
                : fieldNames(fieldNames),
                  fieldEncodings(fieldEncodings) {}

        std::vector<std::string> fieldNames;
        std::vector<TypeEncoding> fieldEncodings;
    };

    class AnonymousStructEncoding : public AnonymousRecordEncoding {
    public:
        AnonymousStructEncoding(const std::vector<std::string>& fieldNames, const std::vector<TypeEncoding>& fieldEncodings)
                : AnonymousRecordEncoding(fieldNames, fieldEncodings) {}
    };

    class AnonymousUnionEncoding : public AnonymousRecordEncoding {
    public:
        AnonymousUnionEncoding(const std::vector<std::string>& fieldNames, const std::vector<TypeEncoding>& fieldEncodings)
                : AnonymousRecordEncoding(fieldNames, fieldEncodings) {}
    };
}
