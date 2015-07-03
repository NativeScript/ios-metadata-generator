#pragma once

#include <vector>
#include "../Meta/TypeEntities.h"
#include "binaryStructures.h"
#include "binaryWriter.h"

using namespace std;

namespace binary {
/*
     * \class BinaryTypeEncodingSerializer
     * \brief Applies the Visitor pattern for serializing \c typeEncoding::TypeEncoding objects in binary format.
     */
class BinaryTypeEncodingSerializer : public ::Meta::TypeVisitor<unique_ptr<binary::TypeEncoding> > {
private:
    BinaryWriter _heapWriter;

    unique_ptr<TypeEncoding> serializeRecordEncoding(binary::BinaryTypeEncodingType encodingType, std::vector< ::Meta::RecordField>& fields);

public:
    BinaryTypeEncodingSerializer(BinaryWriter& heapWriter)
        : _heapWriter(heapWriter)
    {
    }

    MetaFileOffset visit(std::vector< ::Meta::Type>& types);

    virtual unique_ptr<TypeEncoding> visitUnknown() override;

    virtual unique_ptr<TypeEncoding> visitVoid() override;

    virtual unique_ptr<TypeEncoding> visitBool() override;

    virtual unique_ptr<TypeEncoding> visitShort() override;

    virtual unique_ptr<TypeEncoding> visitUShort() override;

    virtual unique_ptr<TypeEncoding> visitInt() override;

    virtual unique_ptr<TypeEncoding> visitUInt() override;

    virtual unique_ptr<TypeEncoding> visitLong() override;

    virtual unique_ptr<TypeEncoding> visitUlong() override;

    virtual unique_ptr<TypeEncoding> visitLongLong() override;

    virtual unique_ptr<TypeEncoding> visitULongLong() override;

    virtual unique_ptr<TypeEncoding> visitSignedChar() override;

    virtual unique_ptr<TypeEncoding> visitUnsignedChar() override;

    virtual unique_ptr<TypeEncoding> visitUnichar() override;

    virtual unique_ptr<TypeEncoding> visitCString() override;

    virtual unique_ptr<TypeEncoding> visitFloat() override;

    virtual unique_ptr<TypeEncoding> visitDouble() override;

    virtual unique_ptr<TypeEncoding> visitVaList() override;

    virtual unique_ptr<TypeEncoding> visitSelector() override;

    virtual unique_ptr<TypeEncoding> visitInstancetype() override;

    virtual unique_ptr<TypeEncoding> visitClass(::Meta::ClassTypeDetails& typeDetails) override;

    virtual unique_ptr<TypeEncoding> visitProtocol() override;

    virtual unique_ptr<TypeEncoding> visitId(::Meta::IdTypeDetails& typeDetails) override;

    virtual unique_ptr<TypeEncoding> visitConstantArray(::Meta::ConstantArrayTypeDetails& typeDetails) override;

    virtual unique_ptr<TypeEncoding> visitIncompleteArray(::Meta::IncompleteArrayTypeDetails& typeDetails) override;

    virtual unique_ptr<TypeEncoding> visitInterface(::Meta::InterfaceTypeDetails& typeDetails) override;

    virtual unique_ptr<TypeEncoding> visitBridgedInterface(::Meta::BridgedInterfaceTypeDetails& typeDetails) override;

    virtual unique_ptr<TypeEncoding> visitPointer(::Meta::PointerTypeDetails& typeDetails) override;

    virtual unique_ptr<TypeEncoding> visitBlock(::Meta::BlockTypeDetails& typeDetails) override;

    virtual unique_ptr<TypeEncoding> visitFunctionPointer(::Meta::FunctionPointerTypeDetails& typeDetails) override;

    virtual unique_ptr<TypeEncoding> visitStruct(::Meta::StructTypeDetails& typeDetails) override;

    virtual unique_ptr<TypeEncoding> visitUnion(::Meta::UnionTypeDetails& typeDetails) override;

    virtual unique_ptr<TypeEncoding> visitAnonymousStruct(::Meta::AnonymousStructTypeDetails& typeDetails) override;

    virtual unique_ptr<TypeEncoding> visitAnonymousUnion(::Meta::AnonymousUnionTypeDetails& typeDetails) override;
};
}
