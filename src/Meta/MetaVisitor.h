#pragma once

namespace Meta {

class InterfaceMeta;
class ProtocolMeta;
class CategoryMeta;
class FunctionMeta;
class StructMeta;
class UnionMeta;
class JsCodeMeta;
class VarMeta;

class MetaVisitor {
public:
    virtual void visit(InterfaceMeta* meta) = 0;

    virtual void visit(ProtocolMeta* meta) = 0;

    virtual void visit(CategoryMeta* meta) = 0;

    virtual void visit(FunctionMeta* meta) = 0;

    virtual void visit(StructMeta* meta) = 0;

    virtual void visit(UnionMeta* meta) = 0;

    virtual void visit(JsCodeMeta* meta) = 0;

    virtual void visit(VarMeta* meta) = 0;
};
}