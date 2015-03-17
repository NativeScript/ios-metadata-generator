#include "TypeEncodingFactory.h"

Meta::TypeEncoding createFromType(clang::Type& type) {
    return Meta::BoolEncoding();
}

Meta::TypeEncoding createFromConstantArrayType(clang::ConstantArrayType& type) { return createFromType(type); }

Meta::TypeEncoding createFromDependentSizedArrayType(clang::DependentSizedArrayType& type) { return createFromType(type); }

Meta::TypeEncoding createFromIncompleteArrayType(clang::IncompleteArrayType& type) { return createFromType(type); }

Meta::TypeEncoding createFromVariableArrayType(clang::VariableArrayType& type) { return createFromType(type); }

Meta::TypeEncoding createFromArrayType(clang::ArrayType& type) { return createFromType(type); }

Meta::TypeEncoding createFromAttributedType(clang::AttributedType& type) { return createFromType(type); }

Meta::TypeEncoding createFromBlockPointerType(clang::BlockPointerType& type) { return createFromType(type); }

Meta::TypeEncoding createFromBuiltinType(clang::BuiltinType& type) { return createFromType(type); }

Meta::TypeEncoding createFromComplexType(clang::ComplexType& type) { return createFromType(type); }

Meta::TypeEncoding createFromFunctionNoProtoType(clang::FunctionNoProtoType& type) { return createFromType(type); }

Meta::TypeEncoding createFromFunctionProtoType(clang::FunctionProtoType& type) { return createFromType(type); }

Meta::TypeEncoding createFromFunctionType(clang::FunctionType& type) { return createFromType(type); }

Meta::TypeEncoding createFromObjCObjectPointerType(clang::ObjCObjectPointerType& type) { return createFromType(type); }

Meta::TypeEncoding createFromObjCInterfaceType(clang::ObjCInterfaceType& type) { return createFromType(type); }

Meta::TypeEncoding createFromObjCObjectTypeImpl(clang::ObjCObjectTypeImpl& type) { return createFromType(type); }

Meta::TypeEncoding createFromObjCObjectType(clang::ObjCObjectType& type) { return createFromType(type); }

Meta::TypeEncoding createFromPointerType(clang::PointerType& type) { return createFromType(type); }

Meta::TypeEncoding createFromEnumType(clang::EnumType& type) { return createFromType(type); }

Meta::TypeEncoding createFromRecordType(clang::RecordType& type) { return createFromType(type); }

Meta::TypeEncoding createFromTagType(clang::TagType& type) { return createFromType(type); }

Meta::TypeEncoding createFromTypedefType(clang::TypedefType& type) { return createFromType(type); }

Meta::TypeEncoding createFromVectorType(clang::VectorType& type) { return createFromType(type); }