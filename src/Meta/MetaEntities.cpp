#include "MetaEntities.h"

static void visitBaseClass(Meta::MetaVisitor* visitor, Meta::BaseClassMeta* baseClass)
{
    for (Meta::MethodMeta* method : baseClass->staticMethods) {
        method->visit(visitor);
    }

    for (Meta::MethodMeta* method : baseClass->instanceMethods) {
        method->visit(visitor);
    }

    for (Meta::PropertyMeta* property : baseClass->instanceProperties) {
        property->visit(visitor);
    }

    for (Meta::PropertyMeta* property : baseClass->staticProperties) {
        property->visit(visitor);
    }
}

void Meta::MethodMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

std::vector<Meta::FFIType> Meta::MethodMeta::getFFISignature()
{
    std::vector<FFIType> types;
    
    for (size_t i = 0; i < this->signature.size(); i++) {
        types.push_back(signature[i]->toFFIType());
    }
    
    return types;
}

void Meta::PropertyMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void Meta::EnumConstantMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void Meta::CategoryMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
    visitBaseClass(visitor, this);
}

void Meta::InterfaceMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
    visitBaseClass(visitor, this);
}

void Meta::ProtocolMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
    visitBaseClass(visitor, this);
}

void Meta::StructMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void Meta::UnionMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void Meta::FunctionMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

std::vector<Meta::FFIType> Meta::FunctionMeta::getFFISignature()
{
    std::vector<FFIType> types;
    
    for (size_t i = 0; i < this->signature.size(); i++) {
        types.push_back(signature[i]->toFFIType());
    }
    
    return types;
}

void Meta::EnumMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void Meta::VarMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}
