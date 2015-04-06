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

    struct FQName {
    public:
        std::string jsName;
        std::string module;

        bool isEmpty() const { return this->jsName.empty(); }

        bool operator==(const FQName& other) const {
            return (jsName == other.jsName && module == other.module);
        }

        bool operator!=(const FQName& other) const {
            return !(*this == other);
        }
    };

    struct Identifier {
    public:
        Identifier()
                : Identifier("", "", "") {}

        Identifier(std::string name, std::string module, std::string fileName)
                : jsName(name),
                  module(module),
                  fileName(fileName) {}

        FQName toFQName() {
            FQName fqName = FQName { .jsName = jsName, .module = module };
            return fqName;
        }

        std::string jsName;
        std::string module;
        std::string fileName;
    };

    class IdentifierGenerator {
    public:
        static std::map<clang::Decl::Kind, std::vector<std::string>>& getIosSdkNamesToRecalculate();

        IdentifierGenerator(clang::SourceManager& sourceManager, clang::HeaderSearch& headerSearch, std::map<clang::Decl::Kind, std::vector<std::string>>& namesToRecalculate)
            : _sourceManager(sourceManager),
              _headerSearch(headerSearch),
              _namesToRecalculate(namesToRecalculate) {}

        std::string getJsName(const clang::Decl& decl, bool throwIfEmpty);

        std::string getModule(const clang::Decl& decl, bool throwIfEmpty);

        std::string getFileName(const clang::Decl& decl, bool throwIfEmpty);

        Identifier getIdentifier(const clang::Decl& decl, bool throwIfEmpty);

    private:
        std::string calculateOriginalName(const clang::Decl& decl);

        std::string calculateJsName(const clang::Decl& decl, std::string originalName);

        std::string recalculateJsName(const clang::Decl& decl, std::string calculatedJsName);

        clang::SourceManager& _sourceManager;
        clang::HeaderSearch& _headerSearch;
        std::map<clang::Decl::Kind, std::vector<std::string>> _namesToRecalculate;
        std::unordered_map<const clang::Decl*, Meta::Identifier> _cache;
    };

    class IdentifierCreationException : public std::exception
    {
    public:
        IdentifierCreationException(Identifier id, std::string message)
                : _id(id),
                  _message(message) {}

        virtual const char* what() const throw() { return this->whatAsString().c_str(); }
        std::string whatAsString() const { return _message + " Decl: \"" + _id.jsName + "\"" + " Module: " + _id.module + " File: " +  _id.fileName; }
        Identifier getId() const { return this->_id; }
        std::string getMessage() const { return this-> _message; }

    private:
        Identifier _id;
        std::string _message;
    };
}