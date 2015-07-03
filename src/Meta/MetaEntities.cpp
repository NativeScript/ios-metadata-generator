#include "MetaEntities.h"

void Meta::MethodMeta::visit(MetaVisitor* visitor)
{
}

void Meta::PropertyMeta::visit(MetaVisitor* visitor)
{
}

void Meta::CategoryMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void Meta::InterfaceMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void Meta::ProtocolMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
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

void Meta::JsCodeMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}

void Meta::VarMeta::visit(MetaVisitor* visitor)
{
    visitor->visit(this);
}