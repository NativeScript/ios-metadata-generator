#ifndef TYPEENCODING_H
#define TYPEENCODING_H

#include <string>
#include <vector>

using namespace std;

namespace Meta {
    struct FQName {
    public:
        string name;
        string module;

        bool isEmpty() const {
            return this->name.empty();
        }
    };

    class TypeEncoding {
    public:
        virtual ~TypeEncoding() { }

        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer* s) = 0;
    };

    // primitive types

    class UnknownEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class VoidEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class BoolEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class ShortEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class UShortEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class IntEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class UIntEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class LongEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class ULongEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class LongLongEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class ULongLongEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class SignedCharEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class UnsignedCharEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class UnicharEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class CStringEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class FloatEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class DoubleEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };

    // objc types

    class VaListEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class SelectorEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class InstancetypeEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class ClassEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
    class ProtocolEncoding : public TypeEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };

    class IdEncoding : public TypeEncoding {
    public:
        vector<FQName> protocols;

        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };

    class ArrayEncoding : public TypeEncoding {
    public:
        unique_ptr<TypeEncoding> elementType;
    };

    class ConstantArrayEncoding : public ArrayEncoding {
    public:
        int size;

        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };

    class IncompleteArrayEncoding : public ArrayEncoding{
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };

    class InterfaceEncoding : public TypeEncoding {
    public:
        FQName name;

        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };

    class PointerEncoding : public TypeEncoding {
    public:
        unique_ptr<TypeEncoding> target;

        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };

    class BlockEncoding : public TypeEncoding {
    public:
        vector<unique_ptr<TypeEncoding>> blockCall;

        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };

    class FunctionEncoding : public TypeEncoding {
    public:
        vector<unique_ptr<TypeEncoding>> functionCall;

        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };

    class StructEncoding : public TypeEncoding {
    public:
        FQName name;

        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };

    class UnionEncoding : public TypeEncoding {
    public:
        FQName name;

        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };

    class InterfaceDeclarationEncoding : public TypeEncoding {
    public:
        FQName name;

        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };

    class AnonymousRecordEncoding : public TypeEncoding {
    public:
        vector<string> fieldNames;
        vector<unique_ptr<TypeEncoding>> fieldEncodings;
    };

    class AnonymousStructEncoding : public AnonymousRecordEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };

    class AnonymousUnionEncoding : public AnonymousRecordEncoding {
    public:
        virtual std::unique_ptr<binary::TypeEncoding> serialize(binary::BinaryTypeEncodingSerializer *s) override;
    };
}

#endif
