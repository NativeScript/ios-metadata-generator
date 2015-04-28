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
    struct ModuleId {
        std::string fullName;
        bool isPartOfFramework;
        bool isSystemModule;

        ModuleId(std::string fullName, bool isPartOfFramework, bool isSystemModule)
                : fullName(fullName),
                  isPartOfFramework(isPartOfFramework),
                  isSystemModule(isSystemModule) { }

        std::string topLevelModuleName() {
            std::size_t dotIndex = fullName.find(".");
            return (dotIndex == std::string::npos) ? fullName : fullName.substr(0, dotIndex);
        }

        bool operator==(const ModuleId& other) const {
            return (fullName == other.fullName && isPartOfFramework == other.isPartOfFramework && isSystemModule == other.isSystemModule);
        }

        bool operator!=(const ModuleId& other) const { return !(*this == other); }
    };

    struct HeaderFileId {
        std::string fullPath;
        std::shared_ptr<ModuleId> module;

        HeaderFileId(std::string fullPath, std::shared_ptr<ModuleId> module)
                : fullPath(fullPath),
                  module(module) {}

        bool operator==(const HeaderFileId & other) const { return fullPath == other.fullPath && (*module) == (*other.module); }

        bool operator!=(const HeaderFileId & other) const { return !(*this == other); }
    };

    struct DeclId {
    public:
        DeclId() : DeclId("", "", nullptr) {}

        DeclId(std::string name, std::string jsName, std::shared_ptr<HeaderFileId> file)
                : name(name),
                  jsName(jsName),
                  file(file) { }

        std::string filePathOrEmpty() const { return this->file == nullptr ? "" : this->file->fullPath; }

        std::string moduleNameOrEmpty() const { return this->file == nullptr ? "" : (this->file->module == nullptr ? "" : this->file->module->fullName ); }

        std::string topLevelModuleNameOrEmpty() const { return this->file == nullptr ? "" : (this->file->module == nullptr ? "" : this->file->module->topLevelModuleName() ); }

        bool operator==(const DeclId & other) const { return (name == other.name && jsName == other.jsName && (*file) == (*other.file)); }

        bool operator!=(const DeclId & other) const { return !(*this == other); }

        std::string name;
        std::string jsName;
        std::shared_ptr<HeaderFileId> file;
    };

    class IdentifierFactory {
    public:
        static std::map<clang::Decl::Kind, std::vector<std::string>>& getIosSdkNamesToRecalculate();

        IdentifierFactory(clang::SourceManager& sourceManager, clang::HeaderSearch& headerSearch, std::map<clang::Decl::Kind, std::vector<std::string>>& namesToRecalculate)
            : _sourceManager(sourceManager),
              _headerSearch(headerSearch),
              _namesToRecalculate(namesToRecalculate) { }

        DeclId getIdentifier(const clang::Decl& decl, bool throwIfEmpty);

        std::shared_ptr<Meta::HeaderFileId> getHeaderFile(const clang::FileEntry* entry);

        std::shared_ptr<ModuleId> getModule(const clang::Module* module);

    private:
        std::string calculateOriginalName(const clang::Decl& decl);

        std::string calculateJsName(const clang::Decl& decl, std::string originalName);

        std::string recalculateJsName(const clang::Decl& decl, std::string calculatedJsName);

        clang::SourceManager& _sourceManager;
        clang::HeaderSearch& _headerSearch;
        std::map<clang::Decl::Kind, std::vector<std::string>> _namesToRecalculate;

        // cache
        std::unordered_map<const clang::Decl*, Meta::DeclId> _declCache;
        std::unordered_map<const clang::FileEntry*, std::shared_ptr<HeaderFileId>> _fileCache;
        std::unordered_map<const clang::Module*, std::shared_ptr<ModuleId>> _moduleCache;
    };

    class IdentifierCreationException : public std::exception
    {
    public:
        IdentifierCreationException(DeclId id, std::string message)
                : _id(id),
                  _message(message) {}

        virtual const char* what() const throw() { return this->whatAsString().c_str(); }
        std::string whatAsString() const { return _message + " Decl: \"" + _id.jsName + "\"" + " Module: " + _id.moduleNameOrEmpty() + " File: " +  _id.filePathOrEmpty(); }
        DeclId getId() const { return this->_id; }
        std::string getMessage() const { return this-> _message; }

    private:
        DeclId _id;
        std::string _message;
    };
}