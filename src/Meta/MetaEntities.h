#pragma once

#include <string>
#include <vector>
#include <map>
#include <iostream>
#include "TypeEntities.h"
#include "MetaVisitor.h"

#define UNKNOWN_VERSION { -1, -1, -1 }

namespace Meta {

    std::string topLevelModuleOf(const std::string& fullModuleName);

    struct Version {
        int Major;
        int Minor;
        int SubMinor;
    };

    // TODO: Change values (and maybe rename) some of the flag values
    // TODO: Change binary conversation of the flags not to depend on the actual integral value of the flags.
    enum MetaFlags : uint16_t {
        // Common
        None                                  = 0,
        HasName                               = 1 << 1, // TODO: this should be determined when serializing to binary format and this flag should be removed from here
        IsIosAppExtensionAvailable            = 1 << 2,
        // Function
        FunctionIsVariadic                    = 1 << 3,
        FunctionOwnsReturnedCocoaObject       = 1 << 4,
        // Method
        MethodIsVariadic                      = 1 << 5,
        MethodIsNullTerminatedVariadic        = 1 << 6,
        MethodOwnsReturnedCocoaObject         = 1 << 7,
        // Property
        PropertyHasGetter                     = 1 << 8, // TODO: this should be determined when serializing to binary format and this flag should be removed from here
        PropertyHasSetter                     = 1 << 9 // TODO: this should be determined when serializing to binary format and this flag should be removed from here
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

        std::string name;
        std::string jsName;
        std::string module;

        // Availability
        Version introducedIn = UNKNOWN_VERSION;
        Version obsoletedIn = UNKNOWN_VERSION;
        Version deprecatedIn = UNKNOWN_VERSION;

        clang::Decl *declaration;

        // visitors
        virtual void visit(MetaVisitor* serializer) = 0;

        bool is(MetaType type) { return this->type == type; }

        bool getFlags(MetaFlags flags) {
            return (this->flags & flags) == flags;
        }

        void setFlags(MetaFlags flags, bool value) {
            value ? this->flags = (MetaFlags)(this->flags | flags) : this->flags = (MetaFlags)(this->flags & ~flags);
        }

        std::string getTopLevelModule() {
            return topLevelModuleOf(module);
        }
    };

    class MethodMeta : public Meta {
    public:
        MethodMeta() : Meta() {
            this->type = MetaType::Method;
        }

        std::string selector;
        std::string typeEncoding;
        std::vector<Type> signature;

        virtual void visit(MetaVisitor* visitor) override;
    };

    class PropertyMeta : public Meta {
    public:
        PropertyMeta() : Meta() {
            this->type = MetaType::Property;
        }

        std::shared_ptr<MethodMeta> getter;
        std::shared_ptr<MethodMeta> setter;

        virtual void visit(MetaVisitor* visitor) override;
    };

    class BaseClassMeta : public Meta {
    public:
        std::vector<std::shared_ptr<MethodMeta>> instanceMethods;
        std::vector<std::shared_ptr<MethodMeta>> staticMethods;
        std::vector<std::shared_ptr<PropertyMeta>> properties;
        std::vector<FQName> protocols;
    };

    class CategoryMeta : public BaseClassMeta {
    public:
        CategoryMeta() {
            this->type = MetaType::Category;
        }

        FQName extendedInterface;

        virtual void visit(MetaVisitor* visitor) override;
    };

    class InterfaceMeta : public BaseClassMeta {
    public:
        InterfaceMeta() {
            this->type = MetaType::Interface;
        }

        FQName baseName;

        virtual void visit(MetaVisitor* visitor) override;
    };

    class ProtocolMeta : public BaseClassMeta {
    public:
        ProtocolMeta() {
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
        StructMeta() {
            this->type = MetaType::Struct;
        }

        virtual void visit(MetaVisitor* visitor) override;
    };

    class UnionMeta : public RecordMeta {
    public:
        UnionMeta() {
            this->type = MetaType::Union;
        }

        virtual void visit(MetaVisitor* visitor) override;
    };

    class FunctionMeta : public Meta {
    public:
        FunctionMeta() {
            this->type = MetaType::Function;
        }
        std::vector<Type> signature;

        virtual void visit(MetaVisitor* visitor) override;
    };

    class JsCodeMeta : public Meta {
    public:
        JsCodeMeta() {
            this->type = MetaType::JsCode;
        }
        std::string jsCode;

        virtual void visit(MetaVisitor* visitor) override;
    };

    class VarMeta : public Meta {
    public:
        VarMeta() {
            this->type = MetaType::Var;
        }
        Type signature;

        virtual void visit(MetaVisitor* visitor) override;
    };

    class Module {
    public:
        typedef std::map<std::string, std::shared_ptr<Meta>>::iterator iterator;
        typedef std::map<std::string, std::shared_ptr<Meta>>::const_iterator const_iterator;
        typedef std::map<std::string, std::shared_ptr<Meta>>::size_type size_type;

        Module(std::string name)
                : _name(name) {}

        Module(std::string name, std::vector<std::shared_ptr<Meta>>& declarations)
                : _name(name) {
            for(std::vector<std::shared_ptr<Meta>>::iterator it = declarations.begin(); it != declarations.end(); ++it) {
                this->add(*it);
            }
        }

        bool isTopLevelModule() const {
            return topLevelModuleOf(this->_name) == _name;
        }

        std::shared_ptr<Meta> getMeta(const std::string& jsName) {
            std::map<std::string, std::shared_ptr<Meta>>::iterator it = _declarations.find(jsName);
            if(it != _declarations.end())
                return it->second;
            return nullptr;
        }

        template<class T>
        std::shared_ptr<T> getMetaAs(const std::string& jsName) {
            std::shared_ptr<Meta> meta = getMeta(jsName);
            return std::static_pointer_cast<T>(meta);
        }

        void add(std::shared_ptr<Meta> meta) {
            if(_declarations.find(meta->jsName) == _declarations.end())
                _declarations.insert(std::pair<std::string, std::shared_ptr<Meta>>(meta->jsName, meta));
            //else
            //    std::cerr << "The declaration with name '" << meta->jsName << "' already exists in module '" << _name << "'." <<  std::endl; // TODO: research why there are conflicts
        }

        bool remove(std::string& jsName) {
            return _declarations.erase(jsName) == 1;
        }

        Module::iterator begin() { return _declarations.begin(); }
        Module::const_iterator begin() const { return _declarations.begin(); }
        Module::iterator end() { return _declarations.end(); }
        Module::const_iterator end() const { return _declarations.end(); }

        std::string getName() const { return _name; }
        Module::size_type size() const { return _declarations.size(); }

    private:
        std::string _name;
        std::map<std::string, std::shared_ptr<Meta>> _declarations;
    };

    class MetaContainer {

    public:
        typedef std::vector<Module>::iterator top_level_modules_iterator;
        typedef std::vector<Module>::const_iterator const_top_level_modules_iterator;

        typedef std::set<std::string>::iterator all_modules_iterator;
        typedef std::set<std::string>::const_iterator const_all_modules_iterator;

        typedef std::vector<std::shared_ptr<CategoryMeta>>::iterator categories_iterator;
        typedef std::vector<std::shared_ptr<CategoryMeta>>::const_iterator categories_const_iterator;
        typedef std::vector<Module>::size_type size_type;

        void add(std::shared_ptr<Meta> meta) {
            _allModules.insert(meta->module);
            if(meta->is(MetaType::Category)) {
                std::shared_ptr<CategoryMeta> category = std::static_pointer_cast<CategoryMeta>(meta);
                this->_categories.push_back(category);
                this->_categoryIsMerged.push_back(false);
            }
            else {
                std::string moduleName = meta->getTopLevelModule();
                getModule(moduleName, true)->add(meta);
            }
        }

        size_type topLevelMetasCount() const {
            size_type size = 0;
            for(std::vector<Module>::const_iterator it = _topLevelModules.begin(); it != _topLevelModules.end(); ++it)
                size += it->size();
            return size + _categories.size();
        }

        int topLevelModulesCount() { return _topLevelModules.size(); }

        int allModulesCount() { return _allModules.size(); }

        int categoriesCount() { return _categories.size(); }

        bool contains(const std::string& moduleName) {
            return getModuleIndex(moduleName, false) != -1;
        }

        bool contains(const std::string& moduleName, const std::string& jsName) {
            return getMetaAs<Meta>(moduleName, jsName) != nullptr;
        }

        void clear() {
            _topLevelModules.clear();
            _allModules.clear();
            _categories.clear();
            _categoryIsMerged.clear();
        }

        Module *getModule(const std::string& moduleName, bool addIfNotExists = false) {
            int index = getModuleIndex(moduleName, addIfNotExists);
            if(index == -1)
                return nullptr;
            return &_topLevelModules[index];
        }

        std::shared_ptr<Meta> getMeta(const std::string& module, const std::string& jsName) {
            std::string topLevelModule = topLevelModuleOf(module);
            Module *theModule = getModule(topLevelModule, false);
            if(theModule != nullptr) {
                return theModule->getMeta(jsName);
            }
            return nullptr;
        }

        template<class T>
        std::shared_ptr<T> getMetaAs(const std::string& module, const std::string& jsName) {
            return std::static_pointer_cast<T>(getMeta(module, jsName));
        }

        template<class T>
        std::shared_ptr<T> getMetaAs(const FQName& name) {
            return getMetaAs<T>(name.module, name.jsName);
        }

        Module *operator[](std::string module) {
            return getModule(module, false);
        }

        int mergeCategoriesInInterfaces() {
            int mergedCategories = 0;

            for (int i = 0; i < _categories.size(); i++) {
                if(!_categoryIsMerged[i]) {
                    std::shared_ptr<CategoryMeta> category = _categories[i];
                    std::shared_ptr<InterfaceMeta> extendedInterface = this->getMetaAs<InterfaceMeta>(category->extendedInterface);
                    if (!extendedInterface) {
                        std::cerr << "Extended interface for category '" << category->name << "' not found." << std::endl;
                        continue;
                    }

                    for (auto &method : category->instanceMethods) {
                        extendedInterface->instanceMethods.push_back(method);
                    }

                    for (auto &method : category->staticMethods) {
                        extendedInterface->staticMethods.push_back(method);
                    }

                    for (auto &property : category->properties) {
                        extendedInterface->properties.push_back(property);
                    }

                    for (auto &protocol : category->protocols) {
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

        all_modules_iterator all_modules_begin() { return _allModules.begin(); }
        const_all_modules_iterator all_modules_begin() const { return _allModules.begin(); }
        all_modules_iterator all_modules_end() { return _allModules.end(); }
        const_all_modules_iterator all_modules_end() const { return _allModules.end(); }

        categories_iterator categories_begin() { return _categories.begin(); }
        categories_const_iterator categories_begin() const { return _categories.begin(); }
        categories_iterator categories_end() { return _categories.end(); }
        categories_const_iterator categories_end() const { return _categories.end(); }

        template<class T>
        void filter(T filter) { filter.filter(*this); }

    private:
        int getModuleIndex(const std::string& moduleName, bool addIfNotExists = false) {
            for(int i = 0; i < _topLevelModules.size(); i++) {
                if(_topLevelModules[i].getName() == moduleName)
                    return i;
            }
            if(addIfNotExists) {
                _topLevelModules.push_back(Module(moduleName));
                return (_topLevelModules.size() - 1);
            }
            return -1;
        }

        std::vector<Module> _topLevelModules;
        std::set<std::string> _allModules;
        std::vector<std::shared_ptr<CategoryMeta>> _categories;
        std::vector<bool> _categoryIsMerged;
    };
}