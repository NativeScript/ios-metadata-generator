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
        explicit DeclarationConverterVisitor(clang::ASTUnit *astUnit)
                : _astUnit(astUnit),
                  _idFactory(astUnit->getSourceManager(), astUnit->getPreprocessor().getHeaderSearchInfo(), IdentifierFactory::getIosSdkNamesToRecalculate()),
                  _metaFactory(this),
                  _typeFactory(this) { }

        MetaContainer& Traverse() {
            this->_result.clear();
            this->TraverseDecl(this->_astUnit->getASTContext().getTranslationUnitDecl());
            for(std::vector<Type>::iterator it = _unresolvedBridgedInterfaces.begin(); it != _unresolvedBridgedInterfaces.end(); ++it) {
                Type type = *it;
                std::shared_ptr<InterfaceMeta> interface = _result.getInterface(type.getDetailsAs<BridgedInterfaceTypeDetails>().id.name);
                // TODO: Instead of setting empty identifier, handle the case when there is no interface found
                type.getDetailsAs<BridgedInterfaceTypeDetails>().id = (interface ? interface->id : Identifier());
            }
            return this->_result;
        }

        // RecursiveASTVisitor methods
        bool VisitFunctionDecl(clang::FunctionDecl *function);

        bool VisitVarDecl(clang::VarDecl *var);

        bool VisitEnumDecl(clang::EnumDecl *enumDecl);

        bool VisitEnumConstantDecl(clang::EnumConstantDecl *enumConstant);

        bool VisitRecordDecl(clang::RecordDecl *record);

        bool VisitObjCInterfaceDecl(clang::ObjCInterfaceDecl *interface);

        bool VisitObjCProtocolDecl(clang::ObjCProtocolDecl *protocol);

        bool VisitObjCCategoryDecl(clang::ObjCCategoryDecl *protocol);

        // MetaFactoryDelegate methods
        Identifier getId(const clang::Decl& decl, bool throwIfEmpty) override { return _idFactory.getIdentifier(decl, throwIfEmpty); }

        Type getType(const clang::Type* type) override { return _typeFactory.create(type); }

        Type getType(const clang::QualType& type) override { return _typeFactory.create(type); }

        // TypeFactoryDelegate methods
        virtual Identifier getDeclId(const clang::Decl& decl, bool throwIfEmpty) override { return _idFactory.getIdentifier(decl, throwIfEmpty); }

        virtual clang::Decl& validate(clang::Decl& decl) override { _metaFactory.ensureCanBeCreated(decl); return decl; }

        virtual void registerUnresolvedBridgedType(Type& type) override { _unresolvedBridgedInterfaces.push_back(type); }

    private:
        template<class T>
        bool Visit(T *decl) {
            try {
                std::shared_ptr<Meta> meta = this->_metaFactory.create(*decl);
                addToResult(meta);
                //std::cout << "Included: " << meta->jsName << " from " << meta->module << std::endl;
            } catch(MetaCreationException& e) {
                //if(e.isError())
                //    std::cout << e.whatAsString() << std::endl;
            }
            return true;
        }

        void addToResult(std::shared_ptr<Meta> meta) {
            _result.add(meta);
        }

        clang::ASTUnit *_astUnit;
        MetaContainer _result;
        IdentifierFactory _idFactory;
        MetaFactory _metaFactory;
        TypeFactory _typeFactory;

        std::vector<Type> _unresolvedBridgedInterfaces;
    };
}