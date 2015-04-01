#include "MetaEntities.h"

std::string Meta::topLevelModuleOf(const std::string& fullModuleName) {
    std::size_t dotIndex = fullModuleName.find(".");
    return (dotIndex == std::string::npos) ? fullModuleName : fullModuleName.substr(0, dotIndex);
}

void Meta::MethodMeta::visit(MetaVisitor* visitor) {
}

void Meta::PropertyMeta::visit(MetaVisitor* visitor) {
}

void Meta::CategoryMeta::visit(MetaVisitor* visitor) {
    visitor->visit(this);
}

void Meta::InterfaceMeta::visit(MetaVisitor* visitor) {
    visitor->visit(this);
}

void Meta::ProtocolMeta::visit(MetaVisitor* visitor) {
    visitor->visit(this);
}

void Meta::StructMeta::visit(MetaVisitor* visitor) {
    visitor->visit(this);
}

void Meta::UnionMeta::visit(MetaVisitor* visitor) {
    visitor->visit(this);
}

void Meta::FunctionMeta::visit(MetaVisitor* visitor) {
    visitor->visit(this);
}

void Meta::JsCodeMeta::visit(MetaVisitor* visitor) {
    visitor->visit(this);
}

void Meta::VarMeta::visit(MetaVisitor* visitor) {
    visitor->visit(this);
}