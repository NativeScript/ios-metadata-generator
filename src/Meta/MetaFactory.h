#pragma once

#include <clang/Frontend/ASTUnit.h>
#include <clang/Lex/Preprocessor.h>
#include <clang/Lex/HeaderSearch.h>
#include <clang/AST/RecursiveASTVisitor.h>
#include <exception>
#include "MetaEntities.h"
#include "TypeEncodingFactory.h"
#include "JsNameGenerator.h"

namespace Meta {

    class MetaFactory {
    public:
        MetaFactory(clang::ASTUnit *astUnit)
                : _astUnit(astUnit),
                  _typeEncodingFactory(),
                  _jsNameGenerator(JsNameGenerator::getIosSdkNamesToRecalculate()) {}

        std::shared_ptr<FunctionMeta> createFunctionMeta(clang::FunctionDecl& function);

        shared_ptr<RecordMeta> createRecordMeta(clang::RecordDecl& record);

        std::shared_ptr<VarMeta> createVarMeta(clang::VarDecl& var);

        std::shared_ptr<JsCodeMeta> createJsCodeMeta(clang::EnumDecl& enumeration);

        std::shared_ptr<InterfaceMeta> createInterfaceMeta(clang::ObjCInterfaceDecl& interface);

        std::shared_ptr<ProtocolMeta> createProtocolMeta(clang::ObjCProtocolDecl& protocol);

    private:
        MethodMeta createMethodMeta(clang::ObjCMethodDecl& method);

        PropertyMeta createPropertyMeta(clang::ObjCPropertyDecl& property);

        clang::Module *getModule(clang::Decl& decl);
        void populateMetaFields(clang::NamedDecl& decl, Meta& meta);
        void populateBaseClassMetaFields(clang::ObjCContainerDecl& decl, BaseClassMeta& meta);
        Version convertVersion(clang::VersionTuple clangVersion);

        template<class T>
        std::vector<T*> getAttributes(clang::Decl& decl){
            std::vector<T*> attributes;
            for (clang::Decl::attr_iterator i = decl.attr_begin(); i != decl.attr_end(); ++i) {
                clang::Attr *attribute = *i;
                if(T *typedAttribute = clang::dyn_cast<T>(attribute)) {
                    attributes.push_back(typedAttribute);
                }
            }
            return attributes;
        }

        llvm::iterator_range<clang::ObjCProtocolList::iterator> getProtocols(clang::ObjCContainerDecl* objCContainer);

        clang::ASTUnit *_astUnit;
        TypeEncodingFactory _typeEncodingFactory;
        JsNameGenerator _jsNameGenerator;
    };

    class EntityCreationException : public exception
    {
    public:
        EntityCreationException(std::string message, bool isError)
                : _message(message),
                  _isError(isError) {}

        virtual const char* what() const throw()
        {
            return this->_message.c_str();
        }

        std::string getMessage() {
            return this->_message;
        }

        bool isError() {
            return this->_isError;
        }

    private:
        std::string _message;
        bool _isError;
    };
}