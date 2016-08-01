#pragma once

#include "CreationException.h"
#include "MetaFactory.h"
#include <clang/AST/RecursiveASTVisitor.h>
#include <clang/Frontend/ASTUnit.h>
#include <clang/Lex/HeaderSearch.h>
#include <clang/Lex/Preprocessor.h>
#include <iostream>

namespace Meta {
class DeclarationConverterVisitor : public clang::RecursiveASTVisitor<DeclarationConverterVisitor> {
public:
    explicit DeclarationConverterVisitor(clang::SourceManager& sourceManager, clang::HeaderSearch& headerSearch)
        : _metaContainer()
        , _metaFactory(sourceManager, headerSearch)
    {
    }

    std::list<Meta*>& generateMetadata(clang::TranslationUnitDecl* translationUnit)
    {
        this->TraverseDecl(translationUnit);
        return _metaContainer;
    }

    MetaFactory& getMetaFactory()
    {
        return this->_metaFactory;
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

private:
    template <class T>
    bool Visit(T* decl)
    {
        try {
            Meta* meta = this->_metaFactory.create(*decl);
            _metaContainer.push_back(meta);
            //std::cout << "Included: " << meta->jsName << " from " << meta->module->getFullModuleName() << std::endl;
        } catch (MetaCreationException& e) {
            //if(e.isError())
            //std::cout << e.getDetailedMessage() << std::endl;
        }
        return true;
    }

    std::list<Meta*> _metaContainer;
    MetaFactory _metaFactory;
};
}