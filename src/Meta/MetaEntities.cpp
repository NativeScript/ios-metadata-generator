#include "MetaEntities.h"



namespace Meta {
    
Version Version::UnknownVersion;

static void visitBaseClass(MetaVisitor* visitor, BaseClassMeta* baseClass)
{
    for (MethodMeta* method : baseClass->staticMethods) {
        method->visit(visitor);
    }

    for (MethodMeta* method : baseClass->instanceMethods) {
        method->visit(visitor);
    }

    for (PropertyMeta* property : baseClass->instanceProperties) {
        property->visit(visitor);
    }

    for (PropertyMeta* property : baseClass->staticProperties) {
        property->visit(visitor);
    }
}

void MethodMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void PropertyMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void EnumConstantMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void CategoryMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
    visitBaseClass(visitor, this);
}

void InterfaceMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
    visitBaseClass(visitor, this);
}

void ProtocolMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
    visitBaseClass(visitor, this);
}

void StructMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void UnionMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void FunctionMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void EnumMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void VarMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

} // namespace Meta
