#pragma once

#include <stdint.h>
#include <memory>
#include <vector>

namespace binary {
    typedef int32_t MetaFileOffset;
    typedef int32_t MetaArrayCount;

    class MetaFile;
    class BinaryWriter;

    enum BinaryTypeEncodingType : uint8_t
    {
        Unknown,
        Void,
        Bool,
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,
        LongLong,
        ULongLong,
        Char,
        UChar,
        Unichar,
        CharS,
        CString,
        Float,
        Double,
        InterfaceDeclarationReference,
        StructDeclarationReference,
        UnionDeclarationReference,
        InterfaceDeclaration, // NSString* -> DeclarationReference, NSString -> InterfaceDeclaration
        Pointer,
        VaList,
        Selector,
        Class,
        Protocol,
        InstanceType,
        Id,
        ConstantArray,
        IncompleteArray,
        FunctionPointer,
        Block,
        AnonymousStruct,
        AnonymousUnion
    };

    enum BinaryFlags : uint8_t {
        // Common
        HasName = 1 << 7,
        // Function
        FunctionIsVariadic = 1 << 5,
        FunctionOwnsReturnedCocoaObject = 1 << 4,
        // Method
        MethodIsVariadic = 1 << 2,
        MethodIsNullTerminatedVariadic = 1 << 3,
        MethodOwnsReturnedCocoaObject = 1 << 4,
        // Property
        PropertyHasGetter = 1 << 2,
        PropertyHasSetter = 1 << 3
    };

#pragma pack(push, 1)
    struct Meta {
    public:
        MetaFileOffset _names = 0;
        MetaFileOffset _topLevelModule = 0;
        uint8_t _flags = 0;
        uint8_t _introduced_in_host = 0;
        uint8_t _introduced_in_extension = 0;

        virtual MetaFileOffset save(BinaryWriter& writer);
    };

    struct RecordMeta : Meta {
    public:
        MetaFileOffset _fieldNames = 0;
        MetaFileOffset _fieldsEncodings = 0;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct StructMeta : RecordMeta {
    };

    struct UnionMeta : RecordMeta {
    };

    struct FunctionMeta : Meta {
    public:
        MetaFileOffset _encoding = 0;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct JsCodeMeta : Meta {
    public:
        MetaFileOffset _jsCode = 0;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct VarMeta : Meta {
    public:
        MetaFileOffset _encoding = 0;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct MemberMeta : Meta {
    };

    struct MethodMeta : MemberMeta {
    public:
        MetaFileOffset _encoding = 0;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct PropertyMeta : MemberMeta {
        MetaFileOffset _getter = 0;
        MetaFileOffset _setter = 0;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct BaseClassMeta : Meta {
    public:
        MetaFileOffset _instanceMethods = 0;
        MetaFileOffset _staticMethods = 0;
        MetaFileOffset _properties = 0;
        MetaFileOffset _protocols = 0;
        int16_t _initializersStartIndex = -1;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct ProtocolMeta : BaseClassMeta {
    };

    struct InterfaceMeta : BaseClassMeta {
    public:
        MetaFileOffset _baseName = 0;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct ModuleMeta {
    public:
        int8_t _flags;
        MetaFileOffset _name;
        MetaFileOffset _libraries;

        virtual MetaFileOffset save(BinaryWriter& writer);
    };

    struct LibraryMeta {
    public:
        int8_t _flags;
        MetaFileOffset _name;

        virtual MetaFileOffset save(BinaryWriter& writer);
    };

#pragma pack(pop)

    // type encoding

    struct TypeEncoding {
    public:
        TypeEncoding(BinaryTypeEncodingType t) : _type(t) {}

        BinaryTypeEncodingType _type;

        virtual MetaFileOffset save(BinaryWriter& writer);
    };

    struct IncompleteArrayEncoding : public TypeEncoding {
    public:
        IncompleteArrayEncoding() : TypeEncoding(BinaryTypeEncodingType::IncompleteArray) {}

        std::unique_ptr<TypeEncoding> _elementType;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct ConstantArrayEncoding : public TypeEncoding {
    public:
        ConstantArrayEncoding() : TypeEncoding(BinaryTypeEncodingType::ConstantArray) {}

        int _size;
        std::unique_ptr<TypeEncoding> _elementType;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct DeclarationReferenceEncoding : public TypeEncoding {
    public:
        DeclarationReferenceEncoding(BinaryTypeEncodingType type) : TypeEncoding(type) {}

        MetaFileOffset _name;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct PointerEncoding : public TypeEncoding {
    public:
        PointerEncoding() : TypeEncoding(BinaryTypeEncodingType::Pointer) {}

        std::unique_ptr<TypeEncoding> _target;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct BlockEncoding : public TypeEncoding {
    public:
        BlockEncoding() : TypeEncoding(BinaryTypeEncodingType::Block) {}

        uint8_t _encodingsCount;
        std::vector<std::unique_ptr<TypeEncoding>> _encodings;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct FunctionEncoding : public TypeEncoding {
    public:
        FunctionEncoding() : TypeEncoding(BinaryTypeEncodingType::FunctionPointer) {}

        uint8_t _encodingsCount;
        std::vector<std::unique_ptr<TypeEncoding>> _encodings;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct InterfaceDeclarationEncoding : public TypeEncoding {
    public:
        InterfaceDeclarationEncoding() : TypeEncoding(BinaryTypeEncodingType::InterfaceDeclaration) {}

        MetaFileOffset _name;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };

    struct AnonymousRecordEncoding : public TypeEncoding {
    public:
        AnonymousRecordEncoding(BinaryTypeEncodingType t) : TypeEncoding(t) {}

        uint8_t _fieldsCount = 0;
        std::vector<MetaFileOffset> _fieldNames;
        std::vector<std::unique_ptr<TypeEncoding>> _fieldEncodings;

        virtual MetaFileOffset save(BinaryWriter& writer) override;
    };
}
