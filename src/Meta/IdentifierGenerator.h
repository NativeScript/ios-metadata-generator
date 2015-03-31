#pragma once

#include <string>
#include <map>
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
    };

    struct Identifier {
    public:
        Identifier()
                : Identifier("", "", "") {}

        Identifier(std::string name, std::string module, std::string fileName)
                : name(name),
                  module(module),
                  fileName(fileName) {}

        std::string name;
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

        std::string getJsName(const clang::Decl& decl);

        std::string getJsNameOrEmpty(const clang::Decl& decl);

        std::string getModuleName(const clang::Decl& decl);

        std::string getModuleNameOrEmpty(const clang::Decl& decl);

        clang::Module *getModule(const clang::Decl& decl);

        clang::Module *getModuleOrNull(const clang::Decl& decl);

        const clang::FileEntry *getFileEntry(const clang::Decl& decl);

        const clang::FileEntry *getFileEntryOrNull(const clang::Decl& decl);

        std::string getFileName(const clang::Decl& decl);

        std::string getFileNameOrEmpty(const clang::Decl& decl);

        FQName getFqName(const clang::Decl& decl);

        FQName getFqNameOrEmpty(const clang::Decl& decl);

        Identifier getIdentifier(const clang::Decl& decl);

        Identifier getIdentifierOrEmpty(const clang::Decl& decl);

    private:
        std::string calculateOriginalName(const clang::Decl& decl);

        std::string calculateJsName(const clang::Decl& decl, std::string originalName);

        std::string recalculateJsName(const clang::Decl& decl, std::string calculatedJsName);

        clang::SourceManager& _sourceManager;
        clang::HeaderSearch& _headerSearch;
        std::map<clang::Decl::Kind, std::vector<std::string>> _namesToRecalculate;
    };

    class IdentifierCreationException : public std::exception
    {
    public:
        IdentifierCreationException(std::string name, std::string fileName, std::string message)
                : _name(name),
                  _fileName(fileName),
                  _message(message) {}

        virtual const char* what() const throw() { return this->whatAsString().c_str(); }
        std::string whatAsString() const { return _message + " Decl: \"" + _name + "\"" + "(" + _fileName + ")"; }
        std::string getName() const { return this->_name; }
        std::string getFilename() const { return this->_fileName; }
        std::string getMessage() const { return this-> _message; }

    private:
        std::string _name;
        std::string _fileName;
        std::string _message;
    };
}