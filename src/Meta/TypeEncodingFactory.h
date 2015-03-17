#pragma once

#include <clang/AST/RecursiveASTVisitor.h>
#include "TypeEncodingEntities.h"

namespace Meta {
    class TypeEncodingFactory {
    public:
        TypeEncodingFactory() {}

        TypeEncoding createFromType(clang::Type& type);

        TypeEncoding createFromConstantArrayType(clang::ConstantArrayType& type);

        TypeEncoding createFromDependentSizedArrayType(clang::DependentSizedArrayType& type);

        TypeEncoding createFromIncompleteArrayType(clang::IncompleteArrayType& type);

        TypeEncoding createFromVariableArrayType(clang::VariableArrayType& type);

        TypeEncoding createFromArrayType(clang::ArrayType& type);

        TypeEncoding createFromAttributedType(clang::AttributedType& type);

        TypeEncoding createFromBlockPointerType(clang::BlockPointerType& type);

        TypeEncoding createFromBuiltinType(clang::BuiltinType& type);

        TypeEncoding createFromComplexType(clang::ComplexType& type);

        TypeEncoding createFromFunctionNoProtoType(clang::FunctionNoProtoType& type);

        TypeEncoding createFromFunctionProtoType(clang::FunctionProtoType& type);

        TypeEncoding createFromFunctionType(clang::FunctionType& type);

        TypeEncoding createFromObjCObjectPointerType(clang::ObjCObjectPointerType& type);

        TypeEncoding createFromObjCInterfaceType(clang::ObjCInterfaceType& type);

        TypeEncoding createFromObjCObjectTypeImpl(clang::ObjCObjectTypeImpl& type);

        TypeEncoding createFromObjCObjectType(clang::ObjCObjectType& type);

        TypeEncoding createFromPointerType(clang::PointerType& type);

        TypeEncoding createFromEnumType(clang::EnumType& type);

        TypeEncoding createFromRecordType(clang::RecordType& type);

        TypeEncoding createFromTagType(clang::TagType& type);

        TypeEncoding createFromTypedefType(clang::TypedefType& type);

        TypeEncoding createFromVectorType(clang::VectorType& type);
    };
}