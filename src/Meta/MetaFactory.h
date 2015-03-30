#pragma once

#include <clang/Frontend/ASTUnit.h>
#include <clang/Lex/Preprocessor.h>
#include <clang/Lex/HeaderSearch.h>
#include <clang/AST/RecursiveASTVisitor.h>
#include "MetaEntities.h"
#include "TypeFactory.h"
#include "IdentifierGenerator.h"

namespace Meta {

    class MetaFactory {
    public:
        MetaFactory(clang::ASTUnit *astUnit)
                : _astUnit(astUnit),
                  _identifierGenerator(astUnit, IdentifierGenerator::getIosSdkNamesToRecalculate()),
                  _typeFactory(_astUnit, _identifierGenerator) {}

        std::shared_ptr<Meta> create(clang::Decl& decl);

        std::shared_ptr<FunctionMeta> createFromFunction(clang::FunctionDecl& function);

        std::shared_ptr<RecordMeta> createFromRecord(clang::RecordDecl& record);

        std::shared_ptr<VarMeta> createFromVar(clang::VarDecl& var);

        std::shared_ptr<JsCodeMeta> createFromEnum(clang::EnumDecl& enumeration);

        std::shared_ptr<JsCodeMeta> createFromEnumConstant(clang::EnumConstantDecl& enumConstant);

        std::shared_ptr<InterfaceMeta> createFromInterface(clang::ObjCInterfaceDecl& interface);

        std::shared_ptr<ProtocolMeta> createFromProtocol(clang::ObjCProtocolDecl& protocol);

        std::shared_ptr<CategoryMeta> createFromCategory(clang::ObjCCategoryDecl& category);

    private:
        std::shared_ptr<MethodMeta> createFromMethod(clang::ObjCMethodDecl& method);

        std::shared_ptr<PropertyMeta> createFromProperty(clang::ObjCPropertyDecl& property);

        void populateMetaFields(clang::NamedDecl& decl, Meta& meta);
        void populateBaseClassMetaFields(clang::ObjCContainerDecl& decl, BaseClassMeta& meta);
        Version convertVersion(clang::VersionTuple clangVersion);
        llvm::iterator_range<clang::ObjCProtocolList::iterator> getProtocols(clang::ObjCContainerDecl* objCContainer);
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

        clang::ASTUnit *_astUnit;
        IdentifierGenerator _identifierGenerator;
        TypeFactory _typeFactory;
    };

    class MetaCreationException : public std::exception
    {
    public:
        MetaCreationException(Identifier id, std::string message, bool isError)
                : _id(id),
                  _message(message),
                  _isError(isError) {}

        virtual const char* what() const throw() { return this->whatAsString().c_str(); }
        std::string whatAsString() const { return _message + " Decl: " + _id.name + "(" + _id.fileName + ") -> " + (this->isError() ? std::string("error") : std::string("notice")); }
        Identifier getIdentifier() const { return this->_id; }
        std::string getMessage() const { return this-> _message; }
        bool isError() const { return this->_isError; }

    private:
        Identifier _id;
        std::string _message;
        bool _isError;
    };
}