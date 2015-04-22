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
    struct Identifier {
    public:
        Identifier()
                : Identifier("", "", "", "") {}

        Identifier(std::string name, std::string jsName, std::string fullModule, std::string fileName)
                : name(name),
                  jsName(jsName),
                  fullModule(fullModule),
                  fileName(fileName) {
            std::size_t dotIndex = fullModule.find(".");
            this->topLevelModule = (dotIndex == std::string::npos) ? fullModule : fullModule.substr(0, dotIndex);
        }

        bool operator==(const Identifier& other) const {
            return (name == other.name && jsName == other.jsName && fullModule == other.fullModule && topLevelModule == other.topLevelModule && fileName == other.fileName);
        }

        bool operator!=(const Identifier& other) const {
            return !(*this == other);
        }

        std::string name;
        std::string jsName;
        std::string fullModule;
        std::string topLevelModule;
        std::string fileName;
    };

    class IdentifierFactory {
    public:
        static std::map<clang::Decl::Kind, std::vector<std::string>>& getIosSdkNamesToRecalculate();

        IdentifierFactory(clang::SourceManager& sourceManager, clang::HeaderSearch& headerSearch, std::map<clang::Decl::Kind, std::vector<std::string>>& namesToRecalculate)
            : _sourceManager(sourceManager),
              _headerSearch(headerSearch),
              _namesToRecalculate(namesToRecalculate) {}

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
        std::string whatAsString() const { return _message + " Decl: \"" + _id.jsName + "\"" + " Module: " + _id.fullModule + " File: " +  _id.fileName; }
        Identifier getId() const { return this->_id; }
        std::string getMessage() const { return this-> _message; }

    private:
        Identifier _id;
        std::string _message;
    };
}