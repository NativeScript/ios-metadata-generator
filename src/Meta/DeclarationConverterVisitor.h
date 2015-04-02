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
                  _metaFactory(astUnit)  { }

        MetaContainer& Traverse() {
            this->_result.clear();
            this->TraverseDecl(this->_astUnit->getASTContext().getTranslationUnitDecl());
            return this->_result;
        }

        bool VisitFunctionDecl(clang::FunctionDecl *function);

        bool VisitVarDecl(clang::VarDecl *var);

        bool VisitEnumDecl(clang::EnumDecl *enumDecl);

        bool VisitEnumConstantDecl(clang::EnumConstantDecl *enumConstant);

        bool VisitRecordDecl(clang::RecordDecl *record);

        bool VisitObjCInterfaceDecl(clang::ObjCInterfaceDecl *interface);

        bool VisitObjCProtocolDecl(clang::ObjCProtocolDecl *protocol);

        bool VisitObjCCategoryDecl(clang::ObjCCategoryDecl *protocol);

    private:
        template<class T>
        bool Visit(T *decl) {
            try {
                addToResult(this->_metaFactory.create(*decl));
            } catch(MetaCreationException& e) {
//                if(e.isError())
//                    std::cout << e.whatAsString() << std::endl;
            }
            return true;
        }

        void addToResult(std::shared_ptr<Meta> meta) {
            _result.add(meta);
        }

        clang::ASTUnit *_astUnit;
        MetaFactory _metaFactory;
        MetaContainer _result;
    };
}