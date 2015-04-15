#pragma once

#include <string>
#include <vector>
#include "IdentifierGenerator.h"
#include "TypeVisitor.h"

namespace Meta {

    struct RecordField;
    struct TypeDetails;

    enum TypeType {
        TypeUnknown,
        TypeVoid,
        TypeBool,
        TypeShort,
        TypeUShort,
        TypeInt,
        TypeUInt,
        TypeLong,
        TypeULong,
        TypeLongLong,
        TypeULongLong,
        TypeSignedChar,
        TypeUnsignedChar,
        TypeUnichar,
        TypeCString,
        TypeFloat,
        TypeDouble,
        TypeVaList,
        TypeSelector,
        TypeInstancetype,
        TypeProtocol,
        TypeClass,
        TypeId,
        TypeConstantArray,
        TypeIncompleteArray,
        TypePointer,
        TypeBlock,
        TypeFunctionPointer,
        TypeInterface,
        TypeStruct,
        TypeUnion,
        TypePureInterface, // TODO: Remove this type. It is redundant and is never used.
        TypeAnonymousStruct,
        TypeAnonymousUnion
    };

    class Type {
    public:
        Type() : Type(TypeType::TypeUnknown , nullptr) {}

        Type(TypeType type)
                : Type(type, nullptr) {}

        Type(TypeType type, TypeDetails *details)
                : type(type), details(details) {}

        static Type Unknown() { return Type(TypeType::TypeUnknown); }
        static Type Void() { return Type(TypeType::TypeVoid); }
        static Type Bool() { return Type(TypeType::TypeBool); }
        static Type Short() { return Type(TypeType::TypeShort); }
        static Type UShort() { return Type(TypeType::TypeUShort); }
        static Type Int() { return Type(TypeType::TypeInt); }
        static Type UInt() { return Type(TypeType::TypeUInt); }
        static Type Long() { return Type(TypeType::TypeLong); }
        static Type ULong() { return Type(TypeType::TypeULong); }
        static Type LongLong() { return Type(TypeType::TypeLongLong); }
        static Type ULongLong() { return Type(TypeType::TypeULongLong); }
        static Type SignedChar() { return Type(TypeType::TypeSignedChar); }
        static Type UnsignedChar() { return Type(TypeType::TypeUnsignedChar); }
        static Type Unichar() { return Type(TypeType::TypeUnichar); }
        static Type CString() { return Type(TypeType::TypeCString); }
        static Type Float() { return Type(TypeType::TypeFloat); }
        static Type Double() { return Type(TypeType::TypeDouble); }
        static Type VaList() { return Type(TypeType::TypeVaList); }
        static Type Selector() { return Type(TypeType::TypeSelector); }
        static Type Instancetype() { return Type(TypeType::TypeInstancetype); }
        static Type ProtocolType() { return Type(TypeType::TypeProtocol); }

        static Type ClassType(std::vector<FQName> protocols);
        static Type Id(std::vector<FQName> protocols);
        static Type ConstantArray(Type innerType, int size);
        static Type IncompleteArray(Type innerType);
        static Type Interface(FQName name, std::vector<FQName> protocols);
        static Type Pointer(Type innerType);
        static Type Block(std::vector<Type>& signature);
        static Type FunctionPointer(std::vector<Type>& signature);
        static Type Struct(FQName name);
        static Type Union(FQName name);
        static Type PureInterface(FQName name); // TODO: Remove this method
        static Type AnonymousStruct(std::vector<RecordField> fields);
        static Type AnonymousUnion(std::vector<RecordField> fields);

        TypeType getType() const { return type; }

        template<class T>
        T& getDetailsAs() const { return *std::static_pointer_cast<T>(details).get(); }

        bool is(TypeType type) { return this->type == type; }

        template<class T>
        T visit(TypeVisitor<T>& visitor) {
            switch(this->type) {
                case TypeUnknown :
                    return visitor.visitUnknown();
                case TypeVoid :
                    return visitor.visitVoid();
                case TypeBool :
                    return visitor.visitBool();
                case TypeShort :
                    return visitor.visitShort();
                case TypeUShort :
                    return visitor.visitUShort();
                case TypeInt :
                    return visitor.visitInt();
                case TypeUInt :
                    return visitor.visitUInt();
                case TypeLong :
                    return visitor.visitLong();
                case TypeULong :
                    return visitor.visitUlong();
                case TypeLongLong :
                    return visitor.visitLongLong();
                case TypeULongLong :
                    return visitor.visitULongLong();
                case TypeSignedChar :
                    return visitor.visitSignedChar();
                case TypeUnsignedChar :
                    return visitor.visitUnsignedChar();
                case TypeUnichar :
                    return visitor.visitUnichar();
                case TypeCString :
                    return visitor.visitCString();
                case TypeFloat :
                    return visitor.visitFloat();
                case TypeDouble :
                    return visitor.visitDouble();
                case TypeVaList :
                    return visitor.visitVaList();
                case TypeSelector :
                    return visitor.visitSelector();
                case TypeInstancetype :
                    return visitor.visitInstancetype();
                case TypeProtocol :
                    return visitor.visitProtocol();
                case TypeClass :
                    return visitor.visitClass(getDetailsAs<ClassTypeDetails>());
                case TypeId :
                    return visitor.visitId(getDetailsAs<IdTypeDetails>());
                case TypeConstantArray :
                    return visitor.visitConstantArray(getDetailsAs<ConstantArrayTypeDetails>());
                case TypeIncompleteArray :
                    return visitor.visitIncompleteArray(getDetailsAs<IncompleteArrayTypeDetails>());
                case TypePointer :
                    return visitor.visitPointer(getDetailsAs<PointerTypeDetails>());
                case TypeBlock :
                    return visitor.visitBlock(getDetailsAs<BlockTypeDetails>());
                case TypeFunctionPointer :
                    return visitor.visitFunctionPointer(getDetailsAs<FunctionPointerTypeDetails>());
                case TypeInterface :
                    return visitor.visitInterface(getDetailsAs<InterfaceTypeDetails>());
                case TypeStruct :
                    return visitor.visitStruct(getDetailsAs<StructTypeDetails>());
                case TypeUnion :
                    return visitor.visitUnion(getDetailsAs<UnionTypeDetails>());
                case TypePureInterface : // TODO: Remove this type. It is redundant and is never used.
                    return visitor.visitPureInterface(getDetailsAs<PureInterfaceTypeDetails>());
                case TypeAnonymousStruct :
                    return visitor.visitAnonymousStruct(getDetailsAs<AnonymousStructTypeDetails>());
                case TypeAnonymousUnion :
                    return visitor.visitAnonymousUnion(getDetailsAs<AnonymousUnionTypeDetails>());
            }
        }

    private:
        TypeType type;
        std::shared_ptr<TypeDetails> details;
    };

    struct RecordField {
        RecordField() : RecordField("", Type()) {}

        RecordField(std::string name, Type encoding)
                : name(name),
                  encoding(encoding) {}

        std::string name;
        Type encoding;
    };

    struct TypeDetails {};

    struct IdTypeDetails : TypeDetails {
        IdTypeDetails(std::vector<FQName>& protocols)
                : protocols(protocols) {}

        std::vector<FQName> protocols;
    };

    struct ClassTypeDetails : TypeDetails {
        ClassTypeDetails(std::vector<FQName>& protocols)
                : protocols(protocols) {}

        std::vector<FQName> protocols;
    };

    struct InterfaceTypeDetails : TypeDetails {
        InterfaceTypeDetails(FQName name, std::vector<FQName>& protocols)
                : name(name),
                  protocols(protocols) {}

        FQName name;
        std::vector<FQName> protocols;
    };

    struct IncompleteArrayTypeDetails : TypeDetails {
        IncompleteArrayTypeDetails(Type innerType)
                : innerType(innerType) {}

        Type innerType;
    };

    struct ConstantArrayTypeDetails : TypeDetails {
        ConstantArrayTypeDetails(Type innerType, int size)
                : innerType(innerType),
                  size(size) {}

        Type innerType;
        int size;
    };

    struct PointerTypeDetails : TypeDetails {
        PointerTypeDetails(Type innerType)
                : innerType(innerType) {}

        Type innerType;
    };

    struct BlockTypeDetails : TypeDetails {
        BlockTypeDetails(std::vector<Type> signature)
                : signature(signature) {}

        std::vector<Type> signature;
    };

    struct FunctionPointerTypeDetails : TypeDetails {
        FunctionPointerTypeDetails(std::vector<Type> signature)
                : signature(signature) {}

        std::vector<Type> signature;
    };

    struct StructTypeDetails : TypeDetails {
        StructTypeDetails(FQName name)
                : name(name) {}

        FQName name;
    };

    struct UnionTypeDetails : TypeDetails {
        UnionTypeDetails(FQName name)
                : name(name) {}

        FQName name;
    };

    // TODO: Remove this type. It is redundant and is never used.
    struct PureInterfaceTypeDetails : TypeDetails {
        PureInterfaceTypeDetails(FQName name)
                : name(name) {}

        FQName name;
    };

    struct AnonymousStructTypeDetails : TypeDetails {
        AnonymousStructTypeDetails(std::vector<RecordField>& fields)
                : fields(fields) {}

        std::vector<RecordField> fields;
    };

    struct AnonymousUnionTypeDetails : TypeDetails {
        AnonymousUnionTypeDetails(std::vector<RecordField>& fields)
                : fields(fields) {}

        std::vector<RecordField> fields;
    };
}
