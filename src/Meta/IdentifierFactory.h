#pragma once

#include <string>
#include <unordered_map>
#include <vector>
#include <exception>
#include <clang/AST/DeclBase.h>
#include <clang/Frontend/ASTUnit.h>
#include <clang/Lex/Preprocessor.h>
#include <clang/Lex/HeaderSearch.h>
#include "Identifier.h"

namespace Meta {
class IdentifierFactory {
public:
    IdentifierFactory(clang::SourceManager& sourceManager, clang::HeaderSearch& headerSearch)
        : _sourceManager(sourceManager)
        , _headerSearch(headerSearch)
    {
    }

    std::shared_ptr<DeclId> getIdentifier(const clang::Decl& decl, bool throwIfEmpty);

private:
    std::string calculateName(const clang::Decl& decl);

    std::string calculateOriginalName(const clang::Decl& decl);

    std::string calculateJsName(const clang::Decl& decl, std::string originalName);

    clang::SourceManager& _sourceManager;
    clang::HeaderSearch& _headerSearch;

    // cache
    std::unordered_map<const clang::Decl*, std::shared_ptr<DeclId> > _declCache;
};
}
