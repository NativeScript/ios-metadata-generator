#pragma once

#include <clang/Frontend/ASTUnit.h>
#include <clang/Lex/Preprocessor.h>
#include <clang/Lex/HeaderSearch.h>
#include <clang/AST/RecursiveASTVisitor.h>
#include "MetaEntities.h"
#include "TypeFactory.h"
#include "IdentifierFactory.h"

namespace Meta {

    class MetaFactoryDelegate
    {
    public:
        virtual Identifier getId(const clang::Decl& decl, bool throwIfEmpty) = 0;

        virtual Type getType(const clang::Type* type) = 0;

        virtual Type getType(const clang::QualType& type) = 0;
    };

    class MetaFactory {
    public:
        MetaFactory(MetaFactoryDelegate *delegate)
                : _delegate(delegate) {}

        std::shared_ptr<Meta> create(clang::Decl& decl);

        clang::Decl& ensureCanBeCreated(clang::Decl& decl);

    private:
        std::shared_ptr<FunctionMeta> createFromFunction(clang::FunctionDecl& function);

        std::shared_ptr<RecordMeta> createFromRecord(clang::RecordDecl& record);

        std::shared_ptr<VarMeta> createFromVar(clang::VarDecl& var);

        std::shared_ptr<JsCodeMeta> createFromEnum(clang::EnumDecl& enumeration);

        std::shared_ptr<JsCodeMeta> createFromEnumConstant(clang::EnumConstantDecl& enumConstant);

        std::shared_ptr<InterfaceMeta> createFromInterface(clang::ObjCInterfaceDecl& interface);

        std::shared_ptr<ProtocolMeta> createFromProtocol(clang::ObjCProtocolDecl& protocol);

        std::shared_ptr<CategoryMeta> createFromCategory(clang::ObjCCategoryDecl& category);

        std::shared_ptr<MethodMeta> createFromMethod(clang::ObjCMethodDecl& method);

        std::shared_ptr<PropertyMeta> createFromProperty(clang::ObjCPropertyDecl& property);

        void populateMetaFields(clang::NamedDecl& decl, Meta& meta);
        void populateBaseClassMetaFields(clang::ObjCContainerDecl& decl, BaseClassMeta& meta);
        Version convertVersion(clang::VersionTuple clangVersion);
        llvm::iterator_range<clang::ObjCProtocolList::iterator> getProtocols(clang::ObjCContainerDecl* objCContainer);

        std::unordered_map<const clang::Decl*, std::shared_ptr<Meta>> _cache;
        std::vector<const clang::Decl*> _metaCreationStack;
        MetaFactoryDelegate *_delegate;
    };

    class MetaCreationException : public std::exception {
    public:
        MetaCreationException(Identifier id, std::string message, bool isError)
                : _id(id),
                  _message(message),
                  _isError(isError) {}

        virtual const char* what() const throw() { return this->whatAsString().c_str(); }
        std::string whatAsString() const { return _message + " Decl: " + _id.jsName + "(" + _id.fileName + ") -> " + (this->isError() ? std::string("error") : std::string("notice")); }
        Identifier getIdentifier() const { return this->_id; }
        std::string getMessage() const { return this-> _message; }
        bool isError() const { return this->_isError; }

    private:
        Identifier _id;
        std::string _message;
        bool _isError;
    };
}