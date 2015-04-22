#include "TypeEntities.h"

using namespace Meta;

Type Type::ClassType(std::vector<Identifier> protocols) {
    return Type(TypeType::TypeClass, new ClassTypeDetails(protocols));
}

Type Type::Id(std::vector<Identifier> protocols) {
    return Type(TypeType::TypeId, new IdTypeDetails(protocols));
}

Type Type::ConstantArray(Type innerType, int size) {
    return Type(TypeType::TypeConstantArray, new ConstantArrayTypeDetails(innerType, size));
}

Type Meta::Type::IncompleteArray(Type innerType) {
    return Type(TypeType::TypeIncompleteArray, new IncompleteArrayTypeDetails(innerType));
}

Type Meta::Type::Interface(Identifier id, std::vector<Identifier> protocols) {
    return Type(TypeType::TypeInterface, new InterfaceTypeDetails(id, protocols));
}

Type Meta::Type::BridgedInterface(Identifier id) {
    return Type(TypeType::TypeBridgedInterface, new BridgedInterfaceTypeDetails(id));
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

Type Meta::Type::Struct(Identifier id) {
    return Type(TypeType::TypeStruct, new StructTypeDetails(id));
}

Type Meta::Type::Union(Identifier id) {
    return Type(TypeType::TypeUnion, new UnionTypeDetails(id));
}

// TODO: Remove this method
Type Meta::Type::PureInterface(Identifier id) {
    return Type(TypeType::TypePureInterface, new PureInterfaceTypeDetails(id));
}

Type Meta::Type::AnonymousStruct(std::vector<RecordField> fields) {
    return Type(TypeType::TypeAnonymousStruct, new AnonymousStructTypeDetails(fields));
}

Type Meta::Type::AnonymousUnion(std::vector<RecordField> fields) {
    return Type(TypeType::TypeAnonymousUnion, new AnonymousUnionTypeDetails(fields));
}