#pragma once

#include <string>
#include <sstream>
#include <unordered_set>
#include "Meta/MetaEntities.h"

namespace TypeScript {
class DefinitionWriter : Meta::MetaVisitor {
public:
    DefinitionWriter(std::pair<clang::Module*, std::vector<Meta::Meta*> >& module)
        : _module(module)
    {
    }

    std::string write();

    virtual void visit(Meta::InterfaceMeta* meta) override;

    virtual void visit(Meta::ProtocolMeta* meta) override;

    virtual void visit(Meta::CategoryMeta* meta) override;

    virtual void visit(Meta::FunctionMeta* meta) override;

    virtual void visit(Meta::StructMeta* meta) override;

    virtual void visit(Meta::UnionMeta* meta) override;

    virtual void visit(Meta::JsCodeMeta* meta) override;

    virtual void visit(Meta::VarMeta* meta) override;

private:
    template <class Member>
    using CompoundMemberMap = std::map<std::string, std::pair<Meta::BaseClassMeta*, Member*> >;

    void getMembersRecursive(Meta::ProtocolMeta* protocol,
                             CompoundMemberMap<Meta::MethodMeta>& staticMethods,
                             CompoundMemberMap<Meta::PropertyMeta>& properties,
                             CompoundMemberMap<Meta::MethodMeta>& instanceMethods,
                             std::set<Meta::ProtocolMeta*>& visitedProtocols);

    std::string writeMethod(Meta::MethodMeta* meta, Meta::BaseClassMeta* owner);
    std::string writeMethod(CompoundMemberMap<Meta::MethodMeta>::value_type& method, Meta::BaseClassMeta* owner,
                            const std::set<Meta::ProtocolMeta*>& protocols);
    std::string writeProperty(Meta::PropertyMeta* meta, Meta::BaseClassMeta* owner);

    std::string writeFunctionProto(const std::vector<Meta::Type*>& signature);

    void writeMembers(const std::vector<Meta::RecordField>& fields);

    std::string localizeReference(const std::string& jsName, std::string moduleName);
    std::string localizeReference(const Meta::Meta& meta);
    std::string tsifyType(const Meta::Type& type);

    std::pair<clang::Module*, std::vector<Meta::Meta*> >& _module;
    std::unordered_set<std::string> _importedModules;
    std::ostringstream _buffer;
};
}
