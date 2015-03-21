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
                  _resultMap(nullptr) { }

        void Traverse(std::map<std::string, std::shared_ptr<Module>>& modules) {
            this->_resultMap = &modules;
            this->TraverseDecl(this->_astUnit->getASTContext().getTranslationUnitDecl());
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
            std::size_t dotIndex = meta->module.find(".");
            std::string topLevelModuleName = (dotIndex == std::string::npos) ? meta->module : meta->module.substr(0, dotIndex);
            if(_resultMap->find(topLevelModuleName) == _resultMap->end())
                _resultMap->insert(std::pair<std::string, std::shared_ptr<Module>>(topLevelModuleName, std::make_shared<Module>(topLevelModuleName)));
            (*_resultMap)[topLevelModuleName]->push_back(meta);
        }

        clang::ASTUnit *_astUnit;
        MetaFactory _metaFactory;
        std::map<std::string, std::shared_ptr<Module>>* _resultMap;
    };
}