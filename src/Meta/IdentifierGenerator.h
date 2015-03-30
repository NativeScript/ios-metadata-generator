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

        IdentifierGenerator(clang::ASTUnit *astUnit, std::map<clang::Decl::Kind, std::vector<std::string>>& namesToRecalculate)
            : _astUnit(astUnit),
              _namesToRecalculate(namesToRecalculate) {}

        std::string getJsName(clang::Decl& decl);

        std::string getJsNameOrEmpty(clang::Decl& decl);

        std::string getModuleName(clang::Decl& decl);

        std::string getModuleNameOrEmpty(clang::Decl& decl);

        clang::Module *getModule(clang::Decl& decl);

        clang::Module *getModuleOrNull(clang::Decl& decl);

        const clang::FileEntry *getFileEntry(clang::Decl& decl);

        const clang::FileEntry *getFileEntryOrNull(clang::Decl& decl);

        std::string getFileName(clang::Decl& decl);

        std::string getFileNameOrEmpty(clang::Decl& decl);

        FQName getFqName(clang::Decl& decl);

        FQName getFqNameOrEmpty(clang::Decl& decl);

        Identifier getIdentifier(clang::Decl& decl);

        Identifier getIdentifierOrEmpty(clang::Decl& decl);

    private:
        std::string calculateOriginalName(clang::Decl& decl);

        std::string calculateJsName(clang::Decl& decl, std::string originalName);

        std::string recalculateJsName(clang::Decl& decl, std::string calculatedJsName);

        clang::ASTUnit *_astUnit;
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