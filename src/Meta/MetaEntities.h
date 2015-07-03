#pragma once

#include <string>
#include <vector>
#include <map>
#include <unordered_map>
#include <iostream>
#include "TypeEntities.h"
#include "MetaVisitor.h"

#define UNKNOWN_VERSION \
    {                   \
        -1, -1, -1      \
    }

namespace Meta {
struct Version {
    int Major;
    int Minor;
    int SubMinor;
};

enum MetaFlags : uint8_t {
    // Common
    None = 0,
    IsIosAppExtensionAvailable = 1 << 0,
    // Function
    FunctionIsVariadic = 1 << 1,
    FunctionOwnsReturnedCocoaObject = 1 << 2,
    // Method
    MethodIsVariadic = 1 << 3,
    MethodIsNullTerminatedVariadic = 1 << 4,
    MethodOwnsReturnedCocoaObject = 1 << 5
};

enum MetaType {
    Undefined = 0,
    Struct,
    Union,
    Function,
    JsCode,
    Var,
    Interface,
    Protocol,
    Category,
    Method,
    Property
};

class Meta {
public:
    MetaType type = MetaType::Undefined;
    MetaFlags flags = MetaFlags::None;
    DeclId id;

    // Availability
    Version introducedIn = UNKNOWN_VERSION;
    Version obsoletedIn = UNKNOWN_VERSION;
    Version deprecatedIn = UNKNOWN_VERSION;

    clang::Decl* declaration;

    // visitors
    virtual void visit(MetaVisitor* serializer) = 0;

    bool is(MetaType type) { return this->type == type; }

    bool getFlags(MetaFlags flags)
    {
        return (this->flags & flags) == flags;
    }

    void setFlags(MetaFlags flags, bool value)
    {
        value ? this->flags = (MetaFlags)(this->flags | flags) : this->flags = (MetaFlags)(this->flags & ~flags);
    }
};

class MethodMeta : public Meta {
public:
    MethodMeta()
        : Meta()
    {
        this->type = MetaType::Method;
    }

    // just a more convenient way to get the selector of method
    std::string& getSelector() { return this->id.name; }

    std::vector<Type> signature;

    virtual void visit(MetaVisitor* visitor) override;
};

class PropertyMeta : public Meta {
public:
    PropertyMeta()
        : Meta()
    {
        this->type = MetaType::Property;
    }

    std::shared_ptr<MethodMeta> getter;
    std::shared_ptr<MethodMeta> setter;

    virtual void visit(MetaVisitor* visitor) override;
};

class BaseClassMeta : public Meta {
public:
    std::vector<std::shared_ptr<MethodMeta> > instanceMethods;
    std::vector<std::shared_ptr<MethodMeta> > staticMethods;
    std::vector<std::shared_ptr<PropertyMeta> > properties;
    std::vector<DeclId> protocols;
};

class CategoryMeta : public BaseClassMeta {
public:
    CategoryMeta()
    {
        this->type = MetaType::Category;
    }

    DeclId extendedInterface;

    virtual void visit(MetaVisitor* visitor) override;
};

class InterfaceMeta : public BaseClassMeta {
public:
    InterfaceMeta()
    {
        this->type = MetaType::Interface;
    }

    DeclId base;

    virtual void visit(MetaVisitor* visitor) override;
};

class ProtocolMeta : public BaseClassMeta {
public:
    ProtocolMeta()
    {
        this->type = MetaType::Protocol;
    }

    virtual void visit(MetaVisitor* visitor) override;
};

class RecordMeta : public Meta {
public:
    std::vector<RecordField> fields;
};

class StructMeta : public RecordMeta {
public:
    StructMeta()
    {
        this->type = MetaType::Struct;
    }

    virtual void visit(MetaVisitor* visitor) override;
};

class UnionMeta : public RecordMeta {
public:
    UnionMeta()
    {
        this->type = MetaType::Union;
    }

    virtual void visit(MetaVisitor* visitor) override;
};

class FunctionMeta : public Meta {
public:
    FunctionMeta()
    {
        this->type = MetaType::Function;
    }
    std::vector<Type> signature;

    virtual void visit(MetaVisitor* visitor) override;
};

class JsCodeMeta : public Meta {
public:
    JsCodeMeta()
    {
        this->type = MetaType::JsCode;
    }
    std::string jsCode;

    virtual void visit(MetaVisitor* visitor) override;
};

class VarMeta : public Meta {
public:
    VarMeta()
    {
        this->type = MetaType::Var;
    }
    Type signature;

    virtual void visit(MetaVisitor* visitor) override;
};

class ModuleMeta {
public:
    typedef std::map<std::string, std::shared_ptr<Meta> >::iterator iterator;
    typedef std::map<std::string, std::shared_ptr<Meta> >::const_iterator const_iterator;
    typedef std::map<std::string, std::shared_ptr<Meta> >::size_type size_type;

    ModuleMeta(clang::Module* module)
        : _module(module)
    {
    }

    ModuleMeta(clang::Module* module, std::vector<std::shared_ptr<Meta> >& declarations)
        : _module(module)
    {
        for (std::vector<std::shared_ptr<Meta> >::iterator it = declarations.begin(); it != declarations.end(); ++it)
            this->add(*it);
    }

    std::shared_ptr<Meta> getMeta(const std::string& jsName)
    {
        std::map<std::string, std::shared_ptr<Meta> >::iterator it = _declarations.find(jsName);
        if (it != _declarations.end())
            return it->second;
        return nullptr;
    }

    template <class T>
    std::shared_ptr<T> getMetaAs(const std::string& jsName)
    {
        std::shared_ptr<Meta> meta = getMeta(jsName);
        return std::static_pointer_cast<T>(meta);
    }

    void add(std::shared_ptr<Meta> meta)
    {
        if (_declarations.find(meta->id.jsName) == _declarations.end())
            _declarations.insert(std::pair<std::string, std::shared_ptr<Meta> >(meta->id.jsName, meta));
        //else
        //    std::cerr << "The declaration with name '" << meta->jsName << "' already exists in module '" << _name << "'." <<  std::endl; // TODO: research why there are conflicts
    }

    ModuleMeta::iterator begin() { return _declarations.begin(); }
    ModuleMeta::const_iterator begin() const { return _declarations.begin(); }
    ModuleMeta::iterator end() { return _declarations.end(); }
    ModuleMeta::const_iterator end() const { return _declarations.end(); }

    std::string getFullName() const { return _module->getFullModuleName(); }
    clang::Module* getClangModule() const { return _module; }
    ModuleMeta::size_type size() const { return _declarations.size(); }

private:
    clang::Module* _module;
    std::map<std::string, std::shared_ptr<Meta> > _declarations;
};

class MetaContainer {

public:
    typedef std::vector<ModuleMeta>::iterator top_level_modules_iterator;
    typedef std::vector<ModuleMeta>::const_iterator const_top_level_modules_iterator;

    typedef std::vector<std::shared_ptr<CategoryMeta> >::iterator categories_iterator;
    typedef std::vector<std::shared_ptr<CategoryMeta> >::const_iterator categories_const_iterator;
    typedef std::vector<ModuleMeta>::size_type size_type;

    MetaContainer() {}

    void add(std::shared_ptr<Meta> meta)
    {
        if (meta->is(MetaType::Category)) {
            std::shared_ptr<CategoryMeta> category = std::static_pointer_cast<CategoryMeta>(meta);
            this->_categories.push_back(category);
            this->_categoryIsMerged.push_back(false);
        }
        else {
            std::string moduleName = meta->id.module->getTopLevelModule()->getFullModuleName();
            ModuleMeta* module = getTopLevelModule(moduleName);
            if (module == nullptr) {
                ModuleMeta newModule = ModuleMeta(meta->id.module->getTopLevelModule());
                newModule.add(meta);
                this->_topLevelModules.push_back(newModule);
            }
            else {
                module->add(meta);
            }
        }

        if (meta->is(MetaType::Interface)) {
            std::shared_ptr<InterfaceMeta> interface = std::static_pointer_cast<InterfaceMeta>(meta);
            this->_interfaces.insert(std::pair<std::string, std::shared_ptr<InterfaceMeta> >(meta->id.name, interface));
        }
    }

    void removeCategories(bool (*predicate)(std::shared_ptr<CategoryMeta>&))
    {
        for (std::vector<std::shared_ptr<CategoryMeta> >::size_type i = 0; i < _categories.size(); i++) {
            std::shared_ptr<CategoryMeta> cat = _categories[i];
            if (predicate(cat)) {
                _categories.erase(_categories.begin() + i);
                _categoryIsMerged.erase(_categoryIsMerged.begin() + i);
                i--;
            }
        }
    }

    size_type topLevelMetasCount() const
    {
        size_type size = 0;
        for (std::vector<ModuleMeta>::const_iterator it = _topLevelModules.begin(); it != _topLevelModules.end(); ++it)
            size += it->size();
        return size + _categories.size();
    }

    int topLevelModulesCount() { return _topLevelModules.size(); }

    int categoriesCount() { return _categories.size(); }

    bool containsModule(const std::string& moduleName)
    {
        return getModuleIndex(moduleName) != -1;
    }

    bool containsMeta(const std::string& moduleName, const std::string& jsName)
    {
        return getMetaAs<Meta>(moduleName, jsName) != nullptr;
    }

    void clear()
    {
        _topLevelModules.clear();
        _categories.clear();
        _interfaces.clear();
        _categoryIsMerged.clear();
    }

    ModuleMeta* getTopLevelModule(const std::string& moduleName)
    {
        int index = getModuleIndex(moduleName);
        return (index == -1) ? nullptr : &_topLevelModules[index];
    }

    std::shared_ptr<Meta> getMeta(const std::string& topLevelModule, const std::string& jsName)
    {
        ModuleMeta* theModule = getTopLevelModule(topLevelModule);
        return (theModule == nullptr) ? nullptr : theModule->getMeta(jsName);
    }

    template <class T>
    std::shared_ptr<T> getMetaAs(const std::string& topLevelModule, const std::string& jsName)
    {
        return std::static_pointer_cast<T>(getMeta(topLevelModule, jsName));
    }

    template <class T>
    std::shared_ptr<T> getMetaAs(const DeclId& id)
    {
        return getMetaAs<T>(id.module->getTopLevelModule()->getFullModuleName(), id.jsName);
    }

    std::shared_ptr<InterfaceMeta> getInterface(std::string name)
    {
        std::unordered_map<std::string, std::shared_ptr<InterfaceMeta> >::iterator interface = _interfaces.find(name);
        return interface == _interfaces.end() ? nullptr : interface->second;
    }

    ModuleMeta* operator[](std::string module)
    {
        return getTopLevelModule(module);
    }

    int mergeCategoriesInInterfaces()
    {
        int mergedCategories = 0;

        for (int i = 0; i < _categories.size(); i++) {
            if (!_categoryIsMerged[i]) {
                std::shared_ptr<CategoryMeta> category = _categories[i];
                std::shared_ptr<InterfaceMeta> extendedInterface = this->getMetaAs<InterfaceMeta>(category->extendedInterface);
                if (!extendedInterface) {
                    std::cerr << "Extended interface for category '" << category->id.name << "' not found." << std::endl;
                    continue;
                }

                for (auto& method : category->instanceMethods) {
                    extendedInterface->instanceMethods.push_back(method);
                }

                for (auto& method : category->staticMethods) {
                    extendedInterface->staticMethods.push_back(method);
                }

                for (auto& property : category->properties) {
                    extendedInterface->properties.push_back(property);
                }

                for (auto& protocol : category->protocols) {
                    extendedInterface->protocols.push_back(protocol);
                }

                _categoryIsMerged[i] = true;
                mergedCategories++;
            }
        }

        std::cout << "Merged " << mergedCategories << " categories." << std::endl;
        return mergedCategories;
    }

    top_level_modules_iterator top_level_modules_begin() { return _topLevelModules.begin(); }
    const_top_level_modules_iterator top_level_modules_begin() const { return _topLevelModules.begin(); }
    top_level_modules_iterator top_level_modules_end() { return _topLevelModules.end(); }
    const_top_level_modules_iterator top_level_modules_end() const { return _topLevelModules.end(); }

    categories_iterator categories_begin() { return _categories.begin(); }
    categories_const_iterator categories_begin() const { return _categories.begin(); }
    categories_iterator categories_end() { return _categories.end(); }
    categories_const_iterator categories_end() const { return _categories.end(); }

    template <class T>
    void filter(T filter) { filter.filter(*this); }

private:
    int getModuleIndex(const std::string& moduleName)
    {
        for (int i = 0; i < _topLevelModules.size(); i++) {
            if (_topLevelModules[i].getFullName() == moduleName)
                return i;
        }
        return -1;
    }

    std::vector<ModuleMeta> _topLevelModules;
    std::unordered_map<std::string, std::shared_ptr<InterfaceMeta> > _interfaces;
    std::vector<std::shared_ptr<CategoryMeta> > _categories;
    std::vector<bool> _categoryIsMerged;
};
}