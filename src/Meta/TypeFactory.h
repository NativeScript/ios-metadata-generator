#pragma once

#include <clang/AST/RecursiveASTVisitor.h>
#include "TypeEntities.h"
#include "IdentifierGenerator.h"

namespace Meta {

    class MetaFactory;

    class TypeFactory {
    public:
        TypeFactory(MetaFactory& metaFactory, IdentifierGenerator& idGenerator)
        : _metaFactory(metaFactory),
          _idGenerator(idGenerator) {}

        Type create(const clang::Type* type);

        Type create(const clang::QualType& type);

    private:
        Type createFromConstantArrayType(const clang::ConstantArrayType* type);

        Type createFromIncompleteArrayType(const clang::IncompleteArrayType* type);

        Type createFromBlockPointerType(const clang::BlockPointerType* type);

        Type createFromBuiltinType(const clang::BuiltinType* type);

        Type createFromObjCObjectPointerType(const clang::ObjCObjectPointerType* type);

        Type createFromPointerType(const clang::PointerType* type);

        Type createFromEnumType(const clang::EnumType* type);

        Type createFromRecordType(const clang::RecordType* type);

        Type createFromTypedefType(const clang::TypedefType* type);

        Type createFromVectorType(const clang::VectorType* type);

        Type createFromElaboratedType(const clang::ElaboratedType *type);

        Type createFromAdjustedType(const clang::AdjustedType *type);

        Type createFromFunctionProtoType(const clang::FunctionProtoType *type);

        Type createFromFunctionNoProtoType(const clang::FunctionNoProtoType *type);

        Type createFromParenType(const clang::ParenType *type);

        // helper methods
        bool isSpecificTypedefType(const clang::TypedefType* type, const std::string& typedefName);

        bool isSpecificTypedefType(const clang::TypedefType* type, const std::vector<std::string>& typedefNames);

        MetaFactory& _metaFactory;
        IdentifierGenerator _idGenerator;
    };

    class TypeCreationException : public std::exception
    {
    public:
        TypeCreationException(std::string typeName, std::string message, bool isError)
                : _typeName(typeName),
                  _message(message),
                  _isError(isError) {}

        virtual const char* what() const throw() { return this->whatAsString().c_str(); }
        std::string whatAsString() const { return _message + " Type: \"" + _typeName + "\"" + "(" + (_isError ? "error" : "notice" ) + ")"; }
        std::string getTypeName() const { return this->_typeName; }
        std::string getMessage() const { return this-> _message; }
        bool isError() const { return this-> _isError; }

    private:
        std::string _typeName;
        std::string _message;
        bool _isError;
    };
}