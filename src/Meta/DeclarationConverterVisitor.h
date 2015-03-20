#pragma once

#include <clang/Frontend/ASTUnit.h>
#include <clang/Lex/Preprocessor.h>
#include <clang/Lex/HeaderSearch.h>
#include <clang/AST/RecursiveASTVisitor.h>
#include "MetaFactory.h"
#include <iostream>

namespace Meta {
    class DeclarationConverterVisitor : public clang::RecursiveASTVisitor<DeclarationConverterVisitor> {
    public:
        explicit DeclarationConverterVisitor(clang::ASTUnit *astUnit)
                : _astUnit(astUnit),
                  _metaFactory(astUnit),
                  _resultList(nullptr) { }

        std::vector<std::shared_ptr<Meta>>& Traverse(std::vector<std::shared_ptr<Meta>>& resultList) {
            this->_resultList = &resultList;
            this->TraverseDecl(this->_astUnit->getASTContext().getTranslationUnitDecl());
            return *this->_resultList;
        }

        bool VisitFunctionDecl(clang::FunctionDecl *function);

        bool VisitVarDecl(clang::VarDecl *var);

        bool VisitEnumDecl(clang::EnumDecl *enumDecl);

        bool VisitRecordDecl(clang::RecordDecl *record);

        bool VisitObjCInterfaceDecl(clang::ObjCInterfaceDecl *interface);

        bool VisitObjCProtocolDecl(clang::ObjCProtocolDecl *protocol);

    private:
        template<class T>
        bool Visit(T *decl) {
            if(decl->isThisDeclarationADefinition()) {
                try {
                    addToResult(this->_metaFactory.create(*decl));
                } catch(MetaCreationException& e) {
                    std::cout << e.whatAsString() << std::endl;
                }
            }
            return true;
        }

        void addToResult(std::shared_ptr<Meta> meta) {
            //std::cout << "Type: " << meta->type << " Name: " << meta->name << " JS Name: " << meta->jsName << " Module: " << meta->module << " Flags: " << meta->flags << std::endl;
            _resultList->push_back(meta);
        }

        clang::ASTUnit *_astUnit;
        MetaFactory _metaFactory;
        std::vector<std::shared_ptr<Meta>>* _resultList;
    };
}