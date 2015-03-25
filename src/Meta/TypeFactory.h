#pragma once

#include <clang/AST/RecursiveASTVisitor.h>
#include "TypeEntities.h"
#include "IdentifierGenerator.h"

namespace Meta {
    class TypeFactory {
    public:
        TypeFactory(clang::ASTUnit *astUnit, IdentifierGenerator identifierGenerator)
        : _astUnit(astUnit),
          _identifierGenerator(identifierGenerator) {}

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

        // helper methods
        void getSignatureOfFunctionProtoType(const clang::FunctionProtoType* type, std::vector<Type>& signature);

        bool isSpecificTypedefType(const clang::TypedefType* type, const std::string& typedefName);

        bool isSpecificTypedefType(const clang::TypedefType* type, const std::vector<std::string>& typedefNames);

        clang::ASTUnit *_astUnit;
        IdentifierGenerator _identifierGenerator;
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