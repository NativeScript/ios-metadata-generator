#pragma once

#include <string>
#include <clang/Basic/Module.h>

namespace Meta {

struct LinkLib {
    LinkLib()
        : LinkLib("", false)
    {
    }

    LinkLib(const std::string& library, bool isFramework)
        : library(library)
        , isFramework(isFramework)
    {
    }

    std::string library;
    bool isFramework;
};

struct DeclId {
public:
    DeclId()
        : DeclId("", "", "", nullptr)
    {
    }

    DeclId(std::string name, std::string jsName, std::string fileName, clang::Module* module)
        : name(name)
        , jsName(jsName)
        , fileName(fileName)
        , module(module)
    {
    }

    std::string moduleNameOrEmpty() const
    {
        return this->module == nullptr ? "" : this->module->getFullModuleName();
    }

    std::string topLevelModuleNameOrEmpty() const
    {
        return this->module == nullptr ? "" : this->module->getTopLevelModule()->getFullModuleName();
    }

    bool operator==(const DeclId& other) const
    {
        return (name == other.name && jsName == other.jsName && fileName == other.fileName && module == other.module);
    }

    bool operator!=(const DeclId& other) const
    {
        return !(*this == other);
    }

    std::string name;
    std::string jsName;
    std::string fileName;
    clang::Module* module;
};

class IdentifierCreationException : public std::exception {
public:
    IdentifierCreationException(DeclId id, std::string message)
        : _id(id)
        , _message(message)
    {
    }

    std::string whatAsString() const
    {
        return _message + " Decl: \"" + _id.jsName + "\"" + " Module: " + _id.moduleNameOrEmpty() + " File: " + _id.fileName;
    }

    DeclId getId() const
    {
        return this->_id;
    }

    std::string getMessage() const
    {
        return this->_message;
    }

private:
    DeclId _id;
    std::string _message;
};
}