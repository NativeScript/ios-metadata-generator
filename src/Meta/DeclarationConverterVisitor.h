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
                  _lastTraverseResult() { }

        std::vector<std::shared_ptr<Meta>> Traverse() {
            this->TraverseDecl(this->_astUnit->getASTContext().getTranslationUnitDecl());
            return this->_lastTraverseResult;
        }

        bool VisitFunctionDecl(clang::FunctionDecl *function);

        bool VisitVarDecl(clang::VarDecl *var);

        bool VisitEnumDecl(clang::EnumDecl *enumDecl);

        bool VisitRecordDecl(clang::RecordDecl *record);

        bool VisitObjCInterfaceDecl(clang::ObjCInterfaceDecl *interface);

        bool VisitObjCProtocolDecl(clang::ObjCProtocolDecl *protocol);

    private:
        void addToResult(std::shared_ptr<Meta> meta) {
            _lastTraverseResult.push_back(meta);
            //std::cout << "Type: " << meta->type << " Name: " << meta->name << " JS Name: " << meta->jsName << " Module: " << meta->module << " Flags: " << meta->flags << std::endl;
        }

        clang::ASTUnit *_astUnit;
        MetaFactory _metaFactory;
        std::vector<std::shared_ptr<Meta>> _lastTraverseResult;
    };
}