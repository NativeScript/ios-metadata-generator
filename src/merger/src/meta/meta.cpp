#include "meta.h"

using std::swap;

void meta::MethodMeta::serialize(utils::Serializer* serializer) {
}

void meta::PropertyMeta::serialize(utils::Serializer* serializer) {
}

void meta::CategoryMeta::serialize(utils::Serializer* serializer) {
    serializer->serialize(this);
}

void meta::InterfaceMeta::serialize(utils::Serializer* serializer) {
    serializer->serialize(this);
}

void meta::ProtocolMeta::serialize(utils::Serializer* serializer) {
    serializer->serialize(this);
}

void meta::StructMeta::serialize(utils::Serializer* serializer) {
    serializer->serialize(this);
}

void meta::UnionMeta::serialize(utils::Serializer* serializer) {
    serializer->serialize(this);
}

void meta::FunctionMeta::serialize(utils::Serializer* serializer) {
    serializer->serialize(this);
}

void meta::JsCodeMeta::serialize(utils::Serializer* serializer) {
    serializer->serialize(this);
}

void meta::VarMeta::serialize(utils::Serializer* serializer) {
    serializer->serialize(this);
}

void meta::swapMeta(meta::Meta& lhs, meta::Meta& rhs)
{
    swap(lhs.type, rhs.type);
    swap(lhs.flags, rhs.flags);
    swap(lhs.name, rhs.name);
    swap(lhs.jsName, rhs.jsName);
    swap(lhs.module, rhs.module);
    swap(lhs.introducedIn, rhs.introducedIn);
    swap(lhs.obsoletedIn, rhs.obsoletedIn);
    swap(lhs.deprecatedIn, rhs.deprecatedIn);
}

void meta::swapMethodMeta(meta::MethodMeta& lhs, meta::MethodMeta& rhs)
{
    swapMeta(lhs, rhs);
    swap(lhs.selector, rhs.selector);
    swap(lhs.typeEncoding, rhs.typeEncoding);
    swap(lhs.signature, rhs.signature);
}

void meta::swapPropertyMeta(meta::PropertyMeta &lhs, meta::PropertyMeta &rhs) {
    swapMeta(lhs, rhs);
    swap(lhs.getter, rhs.getter);
    swap(lhs.setter, rhs.setter);
}
