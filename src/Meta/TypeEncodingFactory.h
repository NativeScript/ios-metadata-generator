#pragma once

#include <clang/AST/RecursiveASTVisitor.h>
#include "TypeEncodingEntities.h"
#include "IdentifierGenerator.h"

namespace Meta {
    class TypeEncodingFactory {
    public:
        TypeEncodingFactory(clang::ASTUnit *astUnit, IdentifierGenerator identifierGenerator)
        : _astUnit(astUnit),
          _identifierGenerator(identifierGenerator) {}

        TypeEncoding create(const clang::Type* type);

        TypeEncoding create(const clang::QualType& type);

    private:
        TypeEncoding createFromConstantArrayType(const clang::ConstantArrayType* type);

        TypeEncoding createFromIncompleteArrayType(const clang::IncompleteArrayType* type);

        TypeEncoding createFromBlockPointerType(const clang::BlockPointerType* type);

        TypeEncoding createFromBuiltinType(const clang::BuiltinType* type);

        TypeEncoding createFromObjCObjectPointerType(const clang::ObjCObjectPointerType* type);

        TypeEncoding createFromPointerType(const clang::PointerType* type);

        TypeEncoding createFromEnumType(const clang::EnumType* type);

        TypeEncoding createFromRecordType(const clang::RecordType* type);

        TypeEncoding createFromTypedefType(const clang::TypedefType* type);

        TypeEncoding createFromVectorType(const clang::VectorType* type);

        TypeEncoding createFromElaboratedType(const clang::ElaboratedType *type);

        // helper methods
        void getSignatureOfFunctionProtoType(const clang::FunctionProtoType* type, std::vector<TypeEncoding>& signature);

        bool isSpecificTypedefType(const clang::TypedefType* type, const std::string& typedefName);

        bool isSpecificTypedefType(const clang::TypedefType* type, const std::vector<std::string>& typedefNames);

        clang::ASTUnit *_astUnit;
        IdentifierGenerator _identifierGenerator;
    };

    class TypeEncodingCreationException : public std::exception
    {
    public:
        TypeEncodingCreationException(std::string typeName, std::string message, bool isError)
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