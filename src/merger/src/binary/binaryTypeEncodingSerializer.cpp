#include "binaryTypeEncodingSerializer.h"
#include "../typeEncoding/typeEncoding.h"

binary::MetaFileOffset binary::BinaryTypeEncodingSerializer::serialize(std::vector<typeEncoding::TypeEncoding *>& encodings) {
    binary::MetaFileOffset offset = 0;
    if (encodings.size() > 0) {
        vector<unique_ptr<binary::TypeEncoding>> binaryEncodings;
        for (typeEncoding::TypeEncoding *encoding : encodings) {
            unique_ptr<binary::TypeEncoding> binaryEncoding = encoding->serialize(this);
            binaryEncodings.push_back(std::move(binaryEncoding));
        }

        offset = this->_heapWriter->push_arrayCount(encodings.size());
        for (unique_ptr<binary::TypeEncoding>& binaryEncoding : binaryEncodings) {
            binaryEncoding->save(this->_heapWriter.get());
        }
    }
    return offset;
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::UnknownEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Unknown));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::VoidEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Void));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::BoolEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Bool));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::ShortEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Short));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::UShortEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::UShort));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::IntEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Int));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::UIntEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::UInt));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::LongEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Long));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::ULongEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::ULong));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::LongLongEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::LongLong));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::ULongLongEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::ULongLong));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::SignedCharEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Char));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::UnsignedCharEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::UChar));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::UnicharEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Unichar));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::CStringEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::CString));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::FloatEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Float));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::DoubleEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Double));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::VaListEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::VaList));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::SelectorEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Selector));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::InstancetypeEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::InstanceType));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::ClassEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Class));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::ProtocolEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Protocol));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::IdEncoding *encoding) {
    return unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Id));
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::ConstantArrayEncoding *encoding) {
    binary::ConstantArrayEncoding* s = new binary::ConstantArrayEncoding();
    s->_size = encoding->size;
    s->_elementType = encoding->elementType->serialize(this);
    return unique_ptr<binary::TypeEncoding>(s);
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::IncompleteArrayEncoding *encoding) {
    binary::IncompleteArrayEncoding* s = new binary::IncompleteArrayEncoding();
    s->_elementType = encoding->elementType->serialize(this);
    return unique_ptr<binary::TypeEncoding>(s);
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::InterfaceEncoding *encoding) {
    binary::DeclarationReferenceEncoding* s = new binary::DeclarationReferenceEncoding(BinaryTypeEncodingType::InterfaceDeclarationReference);
    s->_name = this->_heapWriter->push_string(encoding->name.name);
    return unique_ptr<binary::TypeEncoding>(s);
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::PointerEncoding *encoding) {
    binary::PointerEncoding* s = new binary::PointerEncoding();
    s->_target = encoding->target->serialize(this);
    return unique_ptr<binary::TypeEncoding>(s);
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::BlockEncoding *encoding) {
    binary::BlockEncoding* s = new binary::BlockEncoding();
    s->_encodingsCount = (uint8_t)encoding->blockCall.size();
    for (std::unique_ptr<typeEncoding::TypeEncoding>& blockEncoding : encoding->blockCall) {
        s->_encodings.push_back(blockEncoding->serialize(this));
    }
    return unique_ptr<binary::TypeEncoding>(s);
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::FunctionEncoding *encoding) {
    binary::FunctionEncoding* s = new binary::FunctionEncoding();
    s->_encodingsCount = (uint8_t)encoding->functionCall.size();
    for (std::unique_ptr<typeEncoding::TypeEncoding>& blockEncoding : encoding->functionCall) {
        s->_encodings.push_back(blockEncoding->serialize(this));
    }
    return unique_ptr<binary::TypeEncoding>(s);
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::StructEncoding *encoding) {
    binary::DeclarationReferenceEncoding* s = new binary::DeclarationReferenceEncoding(BinaryTypeEncodingType::StructDeclarationReference);
    s->_name = this->_heapWriter->push_string(encoding->name.name);
    return unique_ptr<binary::TypeEncoding>(s);
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::UnionEncoding *encoding) {
    binary::DeclarationReferenceEncoding* s = new binary::DeclarationReferenceEncoding(BinaryTypeEncodingType::UnionDeclarationReference);
    s->_name = this->_heapWriter->push_string(encoding->name.name);
    return unique_ptr<binary::TypeEncoding>(s);
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::InterfaceDeclarationEncoding *encoding) {
    binary::InterfaceDeclarationEncoding* s = new binary::InterfaceDeclarationEncoding();
    s->_name = this->_heapWriter->push_string(encoding->name.name);
    return unique_ptr<binary::TypeEncoding>(s);
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::AnonymousStructEncoding *encoding) {
    return this->serializeRecordEncoding(binary::BinaryTypeEncodingType::AnonymousStruct, encoding);
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serialize(typeEncoding::AnonymousUnionEncoding *encoding) {
    return this->serializeRecordEncoding(binary::BinaryTypeEncodingType::AnonymousUnion, encoding);
}

unique_ptr<binary::TypeEncoding> binary::BinaryTypeEncodingSerializer::serializeRecordEncoding(binary::BinaryTypeEncodingType encodingType, typeEncoding::AnonymousRecordEncoding *encoding) {
    binary::AnonymousRecordEncoding* s = new binary::AnonymousRecordEncoding(encodingType);
    s->_fieldsCount = (uint8_t)encoding->fieldNames.size();

    for (string& fieldName : encoding->fieldNames) {
        s->_fieldNames.push_back(this->_heapWriter->push_string(fieldName));
    }

    for (std::unique_ptr<typeEncoding::TypeEncoding>& fieldEncoding : encoding->fieldEncodings) {
        s->_fieldEncodings.push_back(fieldEncoding->serialize(this));
    }
    return unique_ptr<binary::TypeEncoding>(s);
}
