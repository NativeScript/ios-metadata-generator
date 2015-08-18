#pragma once

#include <clang/Frontend/ASTUnit.h>
#include <clang/Lex/Preprocessor.h>
#include <clang/Lex/HeaderSearch.h>
#include <clang/AST/RecursiveASTVisitor.h>
#include "MetaFactory.h"
#include <iostream>

namespace Meta {
class DeclarationConverterVisitor : public clang::RecursiveASTVisitor<DeclarationConverterVisitor>, public MetaFactoryDelegate, public TypeFactoryDelegate {
public:
    explicit DeclarationConverterVisitor(clang::SourceManager& sourceManager, clang::HeaderSearch& headerSearch)
        : _result()
        , _idFactory(sourceManager, headerSearch, IdentifierFactory::getIosSdkNamesToRecalculate())
        , _metaFactory(this)
        , _typeFactory(this)
    {
    }

    void resolveUnresolvedBridgedInterfaces()
    {
        for (const Type& type : _unresolvedBridgedInterfaces) {
            std::shared_ptr<InterfaceMeta> interface = _result.getInterface(type.getDetailsAs<BridgedInterfaceTypeDetails>().id.name);
            // TODO: Handle the case when there is no interface found, instead of setting empty DeclId
            type.getDetailsAs<BridgedInterfaceTypeDetails>().id = (interface ? interface->id : DeclId());
        }
    }

    MetaContainer& getMetaContainer()
    {
        return _result;
    }

    // RecursiveASTVisitor methods
    bool VisitFunctionDecl(clang::FunctionDecl* function);

    bool VisitVarDecl(clang::VarDecl* var);

    bool VisitEnumDecl(clang::EnumDecl* enumDecl);

    bool VisitEnumConstantDecl(clang::EnumConstantDecl* enumConstant);

    bool VisitRecordDecl(clang::RecordDecl* record);

    bool VisitObjCInterfaceDecl(clang::ObjCInterfaceDecl* interface);

    bool VisitObjCProtocolDecl(clang::ObjCProtocolDecl* protocol);

    bool VisitObjCCategoryDecl(clang::ObjCCategoryDecl* protocol);

    // MetaFactoryDelegate methods
    DeclId getId(const clang::Decl& decl, bool throwIfEmpty) override { return _idFactory.getIdentifier(decl, throwIfEmpty); }

    Type getType(const clang::Type* type) override { return _typeFactory.create(type); }

    Type getType(const clang::QualType& type) override { return _typeFactory.create(type); }

    // TypeFactoryDelegate methods
    virtual DeclId getDeclId(const clang::Decl& decl, bool throwIfEmpty) override { return _idFactory.getIdentifier(decl, throwIfEmpty); }

    virtual clang::Decl& validate(clang::Decl& decl) override
    {
        _metaFactory.ensureCanBeCreated(decl);
        return decl;
    }

    virtual void registerUnresolvedBridgedType(Type& type) override { _unresolvedBridgedInterfaces.push_back(type); }

private:
    template <class T>
    bool Visit(T* decl)
    {
        try {
            std::shared_ptr<Meta> meta = this->_metaFactory.create(*decl);
            addToResult(meta);
            //std::cout << "Included: " << meta->id.jsName << " from " << meta->id.fullModule << std::endl;
        }
        catch (MetaCreationException& e) {
            //if(e.isError())
            //    std::cout << e.whatAsString() << std::endl;
        }
        return true;
    }

    void addToResult(std::shared_ptr<Meta> meta)
    {
        _result.add(meta);
    }

    MetaContainer _result;
    IdentifierFactory _idFactory;
    MetaFactory _metaFactory;
    TypeFactory _typeFactory;

    std::vector<Type> _unresolvedBridgedInterfaces;
};
}