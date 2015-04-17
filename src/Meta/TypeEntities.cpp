#include "TypeEntities.h"

using namespace Meta;

Type Type::ClassType(std::vector<FQName> protocols){
    return Type(TypeType::TypeClass, new ClassTypeDetails(protocols));
}

Type Type::Id(std::vector<FQName> protocols) {
    return Type(TypeType::TypeId, new IdTypeDetails(protocols));
}

Type Type::ConstantArray(Type innerType, int size) {
    return Type(TypeType::TypeConstantArray, new ConstantArrayTypeDetails(innerType, size));
}

Type Meta::Type::IncompleteArray(Type innerType) {
    return Type(TypeType::TypeIncompleteArray, new IncompleteArrayTypeDetails(innerType));
}

Type Meta::Type::Interface(FQName name, std::vector<FQName> protocols) {
    return Type(TypeType::TypeInterface, new InterfaceTypeDetails(name, protocols));
}

Type Meta::Type::BridgedInterface(FQName name) {
    return Type(TypeType::TypeBridgedInterface, new BridgedInterfaceTypeDetails(name));
}

Type Meta::Type::Pointer(Type innerType) {
    return Type(TypeType::TypePointer, new PointerTypeDetails(innerType));
}

Type Meta::Type::Block(std::vector<Type>& signature) {
    return Type(TypeType::TypeBlock, new BlockTypeDetails(signature));
}

Type Meta::Type::FunctionPointer(std::vector<Type>& signature) {
    return Type(TypeType::TypeFunctionPointer, new FunctionPointerTypeDetails(signature));
}

Type Meta::Type::Struct(FQName name) {
    return Type(TypeType::TypeStruct, new StructTypeDetails(name));
}

Type Meta::Type::Union(FQName name) {
    return Type(TypeType::TypeUnion, new UnionTypeDetails(name));
}

// TODO: Remove this method
Type Meta::Type::PureInterface(FQName name) {
    return Type(TypeType::TypePureInterface, new PureInterfaceTypeDetails(name));
}

Type Meta::Type::AnonymousStruct(std::vector<RecordField> fields) {
    return Type(TypeType::TypeAnonymousStruct, new AnonymousStructTypeDetails(fields));
}

Type Meta::Type::AnonymousUnion(std::vector<RecordField> fields) {
    return Type(TypeType::TypeAnonymousUnion, new AnonymousUnionTypeDetails(fields));
}

/*
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
*/