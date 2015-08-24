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
    static std::map<clang::Decl::Kind, std::vector<std::string> >& getIosSdkNamesToRecalculate();

    IdentifierFactory(clang::SourceManager& sourceManager, clang::HeaderSearch& headerSearch, std::map<clang::Decl::Kind, std::vector<std::string> >& namesToRecalculate)
        : _sourceManager(sourceManager)
        , _headerSearch(headerSearch)
        , _namesToRecalculate(namesToRecalculate)
    {
    }

    DeclId getIdentifier(const clang::Decl& decl, bool throwIfEmpty);

private:
    std::string calculateName(const clang::Decl &decl);

    std::string calculateOriginalName(const clang::Decl& decl);

    std::string calculateJsName(const clang::Decl& decl, std::string originalName);

    std::string recalculateJsName(const clang::Decl& decl, std::string calculatedJsName);

    clang::SourceManager& _sourceManager;
    clang::HeaderSearch& _headerSearch;
    std::map<clang::Decl::Kind, std::vector<std::string> > _namesToRecalculate;

    // cache
    std::unordered_map<const clang::Decl*, ::Meta::DeclId> _declCache;
};
}