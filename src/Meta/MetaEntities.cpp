#include "MetaEntities.h"
#include <llvm/Support/Debug.h>

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

void Meta::ModuleMeta::add(std::shared_ptr<Meta> newMeta)
{
    if (_ambiguousDeclarations.find(newMeta->id->jsName) != _ambiguousDeclarations.end()) {
        DEBUG_WITH_TYPE("meta", llvm::dbgs() << "The declaration with name '" << newMeta->id->jsName << "' already exists in module '" << _module->Name.c_str() << "'."
                                             << "\n");
        return;
    }

    ModuleMeta::iterator currentMetaIt = _declarations.find(newMeta->id->jsName);
    if (currentMetaIt != _declarations.end()) {
        std::shared_ptr<Meta> currentMeta = (*currentMetaIt).second;

        if (newMeta->is(MetaType::Interface)) {
            assert(currentMeta->is(MetaType::Protocol));

            _declarations.erase(currentMeta->id->jsName);
            currentMeta->id->jsName += "Protocol";
            _declarations.insert({ currentMeta->id->jsName, currentMeta });

            DEBUG_WITH_TYPE("meta", llvm::dbgs() << "Renaming " << currentMeta->id->jsName << "\n");
        }
        else if (newMeta->is(MetaType::Protocol)) {
            assert(currentMeta->is(MetaType::Interface));

            newMeta->id->jsName += "Protocol";
            DEBUG_WITH_TYPE("meta", llvm::dbgs() << "Renaming " << newMeta->id->jsName << "\n");
        }
        else {
            _ambiguousDeclarations.insert(newMeta->id->jsName);
            _declarations.erase(currentMetaIt);

            assert(!currentMeta->is(MetaType::Protocol) && !currentMeta->is(MetaType::Interface));

            DEBUG_WITH_TYPE("meta", llvm::dbgs() << "The declaration with name '" << newMeta->id->jsName << "' already exists"
                                                 << "\n");
            return;
        }
    }

    _declarations.insert({ newMeta->id->jsName, newMeta });
}
