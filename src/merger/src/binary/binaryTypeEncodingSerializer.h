#ifndef BINARYTYPEENCODINGSERIALIZER_H
#define BINARYTYPEENCODINGSERIALIZER_H

#include "../utils/serializer.h"
#include "binaryStructures.h"
#include "binaryWriter.h"
#include <vector>

using namespace std;

namespace typeEncoding {
    class TypeEncoding;
    class AnonymousRecordEncoding;
}

namespace binary {
    /*
     * \class BinaryTypeEncodingSerializer
     * \brief Applies the Visitor pattern for serializing \c typeEncoding::TypeEncoding objects in binary format.
     */
    class BinaryTypeEncodingSerializer : public utils::TypeEncodingSerializer<unique_ptr<binary::TypeEncoding>> {
    private:
        BinaryWriter _heapWriter;

        unique_ptr<TypeEncoding> serializeRecordEncoding(binary::BinaryTypeEncodingType encodingType, typeEncoding::AnonymousRecordEncoding *encoding);

    public:
        BinaryTypeEncodingSerializer(BinaryWriter& heapWriter) : _heapWriter(heapWriter) { }

        MetaFileOffset serialize(std::vector<typeEncoding::TypeEncoding*>& encodings);

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::UnknownEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::VoidEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::BoolEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::ShortEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::UShortEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::IntEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::UIntEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::LongEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::ULongEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::LongLongEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::ULongLongEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::SignedCharEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::UnsignedCharEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::UnicharEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::CStringEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::FloatEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::DoubleEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::VaListEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::SelectorEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::InstancetypeEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::ClassEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::ProtocolEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::IdEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::ConstantArrayEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::IncompleteArrayEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::InterfaceEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::PointerEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::BlockEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::FunctionEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::StructEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::UnionEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::InterfaceDeclarationEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::AnonymousStructEncoding *encoding) override;

        virtual unique_ptr<TypeEncoding> serialize(typeEncoding::AnonymousUnionEncoding *encoding) override;
    };
}

#endif
