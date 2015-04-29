#pragma once

#include <string>
#include <unordered_map>
#include <vector>
#include <exception>
#include <clang/AST/DeclBase.h>
#include <clang/Frontend/ASTUnit.h>
#include <clang/Lex/Preprocessor.h>
#include <clang/Lex/HeaderSearch.h>


namespace Meta {
    struct LinkLib {
        LinkLib() : isFramework(false) { }
        LinkLib(const std::string &library, bool isFramework)
                : library(library), isFramework(isFramework) { }

        std::string library;
        bool isFramework;
    };

    struct DeclId {
    public:
        DeclId() : DeclId("", "", "", nullptr) {}

        DeclId(std::string name, std::string jsName, std::string fileName, clang::Module* module)
                : name(name),
                  jsName(jsName),
                  fileName(fileName),
                  module(module) { }

        std::string moduleNameOrEmpty() const { return this->module == nullptr ? "" : this->module->getFullModuleName(); }

        std::string topLevelModuleNameOrEmpty() const { return this->module == nullptr ? "" : this->module->getTopLevelModule()->getFullModuleName(); }

        bool operator==(const DeclId & other) const { return (name == other.name && jsName == other.jsName && fileName == other.fileName && module == other.module); }

        bool operator!=(const DeclId & other) const { return !(*this == other); }

        std::string name;
        std::string jsName;
        std::string fileName;
        clang::Module* module;
    };

    class IdentifierFactory {
    public:
        static std::map<clang::Decl::Kind, std::vector<std::string>>& getIosSdkNamesToRecalculate();

        IdentifierFactory(clang::SourceManager& sourceManager, clang::HeaderSearch& headerSearch, std::map<clang::Decl::Kind, std::vector<std::string>>& namesToRecalculate)
            : _sourceManager(sourceManager),
              _headerSearch(headerSearch),
              _namesToRecalculate(namesToRecalculate) { }

        DeclId getIdentifier(const clang::Decl& decl, bool throwIfEmpty);

    private:
        std::string calculateOriginalName(const clang::Decl& decl);

        std::string calculateJsName(const clang::Decl& decl, std::string originalName);

        std::string recalculateJsName(const clang::Decl& decl, std::string calculatedJsName);

        clang::SourceManager& _sourceManager;
        clang::HeaderSearch& _headerSearch;
        std::map<clang::Decl::Kind, std::vector<std::string>> _namesToRecalculate;

        // cache
        std::unordered_map<const clang::Decl*, Meta::DeclId> _declCache;
    };

    class IdentifierCreationException : public std::exception {
    public:
        IdentifierCreationException(DeclId id, std::string message)
                : _id(id),
                  _message(message) {}

        virtual const char* what() const throw() { return this->whatAsString().c_str(); }
        std::string whatAsString() const { return _message + " Decl: \"" + _id.jsName + "\"" + " Module: " + _id.moduleNameOrEmpty() + " File: " +  _id.fileName; }
        DeclId getId() const { return this->_id; }
        std::string getMessage() const { return this-> _message; }

    private:
        DeclId _id;
        std::string _message;
    };
}