#include "typeEncoding.h"

std::unique_ptr<binary::TypeEncoding> typeEncoding::UnknownEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::VoidEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::BoolEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::ShortEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::UShortEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::IntEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::UIntEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::LongEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::ULongEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::LongLongEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::ULongLongEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::SignedCharEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::UnsignedCharEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::UnicharEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::CStringEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::FloatEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::DoubleEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::VaListEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::SelectorEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::InstancetypeEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::ClassEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::ProtocolEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::IdEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::ConstantArrayEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::IncompleteArrayEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::InterfaceEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::PointerEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::BlockEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::FunctionEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::StructEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::UnionEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::InterfaceDeclarationEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::AnonymousStructEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}

std::unique_ptr<binary::TypeEncoding> typeEncoding::AnonymousUnionEncoding::serialize(binary::BinaryTypeEncodingSerializer *s) {
    return s->serialize(this);
}
