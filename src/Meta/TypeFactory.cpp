#include "TypeFactory.h"
#include <iostream>

using namespace std;
using namespace Meta;

static std::vector<std::string> tollFreeBridgedTypes = { "CFArrayRef", "CFAttributedStringRef", "CFCalendarRef", "CFCharacterSetRef", "CFDataRef", "CFDateRef", "CFDictionaryRef",
        "CFErrorRef", "CFLocaleRef", "CFMutableArrayRef", "CFMutableAttributedStringRef", "CFMutableCharacterSetRef", "CFMutableDataRef", "CFMutableDictionaryRef", "CFMutableSetRef",
        "CFMutableStringRef", "CFNumberRef", "CFReadStreamRef", "CFRunLoopTimerRef", "CFSetRef", "CFStringRef", "CFTimeZoneRef", "CFURLRef", "CFWriteStreamRef" };

Type TypeFactory::create(const clang::Type* type) {
    try {
        if (const clang::ConstantArrayType *concreteType = clang::dyn_cast<clang::ConstantArrayType>(type))
            return createFromConstantArrayType(concreteType);
        if (const clang::IncompleteArrayType *concreteType = clang::dyn_cast<clang::IncompleteArrayType>(type))
            return createFromIncompleteArrayType(concreteType);
        if (const clang::PointerType *concreteType = clang::dyn_cast<clang::PointerType>(type))
            return createFromPointerType(concreteType);
        if (const clang::BlockPointerType *concreteType = clang::dyn_cast<clang::BlockPointerType>(type))
            return createFromBlockPointerType(concreteType);
        if (const clang::BuiltinType *concreteType = clang::dyn_cast<clang::BuiltinType>(type))
            return createFromBuiltinType(concreteType);
        if (const clang::ObjCObjectPointerType *concreteType = clang::dyn_cast<clang::ObjCObjectPointerType>(type))
            return createFromObjCObjectPointerType(concreteType);
        if (const clang::RecordType *concreteType = clang::dyn_cast<clang::RecordType>(type))
            return createFromRecordType(concreteType);
        if (const clang::EnumType *concreteType = clang::dyn_cast<clang::EnumType>(type))
            return createFromEnumType(concreteType);
        if (const clang::VectorType *concreteType = clang::dyn_cast<clang::VectorType>(type))
            return createFromVectorType(concreteType);
        if (const clang::TypedefType *concreteType = clang::dyn_cast<clang::TypedefType>(type))
            return createFromTypedefType(concreteType);
        if (const clang::ElaboratedType *concreteType = clang::dyn_cast<clang::ElaboratedType>(type))
            return createFromElaboratedType(concreteType);
        if (const clang::AdjustedType *concreteType = clang::dyn_cast<clang::AdjustedType>(type))
            return createFromAdjustedType(concreteType);
        if(const clang::FunctionProtoType *concreteType = clang::dyn_cast<clang::FunctionProtoType>(type))
            return createFromFunctionProtoType(concreteType);
        if(const clang::FunctionNoProtoType *concreteType = clang::dyn_cast<clang::FunctionNoProtoType>(type))
            return createFromFunctionNoProtoType(concreteType);
        if(const clang::ParenType *concreteType = clang::dyn_cast<clang::ParenType>(type))
            return createFromParenType(concreteType);
        throw TypeCreationException(type->getTypeClassName(), "Unable to create encoding for this type.", true);
    }
    catch(IdentifierCreationException& e) {
        throw TypeCreationException(type->getTypeClassName(), string("Identifier Error [") + e.whatAsString() + "]", true);
    }
}

Type TypeFactory::create(const clang::QualType& type) {
    const clang::Type *typePtr = type.getTypePtrOrNull();
    if(typePtr)
        return this->create(typePtr);
    throw TypeCreationException(type->getTypeClassName(), "Unable to get the inner type of qualified type.", true);
}

Type TypeFactory::createFromConstantArrayType(const clang::ConstantArrayType* type) {
    return Type::ConstantArray(this->create(type->getElementType()), (int)type->getSize().roundToDouble());
}

Type TypeFactory::createFromIncompleteArrayType(const clang::IncompleteArrayType* type) {
    return Type::IncompleteArray(this->create(type->getElementType()));
}

Type TypeFactory::createFromBlockPointerType(const clang::BlockPointerType* type) {
    const clang::Type *pointee = type->getPointeeType().getTypePtr();
    Type pointeeType = this->create(pointee);
    if(pointeeType.getType() == TypeType::TypeFunctionPointer) {
        return Type::Block(pointeeType.getDetailsAs<FunctionPointerTypeDetails>().signature);
    }

    throw TypeCreationException(type->getTypeClassName(), "Unable to parse a block type.", true);
}

Type TypeFactory::createFromBuiltinType(const clang::BuiltinType* type) {
    switch (type->getKind()) {
        case clang::BuiltinType::Kind::Void:
            return Type::Void();
        case clang::BuiltinType::Kind::Bool:
            return Type::Bool();
        case clang::BuiltinType::Kind::Char_S:
        case clang::BuiltinType::Kind::Char_U:
        case clang::BuiltinType::Kind::SChar:
            return Type::SignedChar();
        case clang::BuiltinType::Kind::Short:
            return Type::Short();
        case clang::BuiltinType::Kind::Int:
            return Type::Int();
        case clang::BuiltinType::Kind::Long:
            return Type::Long();
        case clang::BuiltinType::Kind::LongLong:
            return Type::LongLong();
        case clang::BuiltinType::Kind::UChar:
            return Type::UnsignedChar();
        case clang::BuiltinType::Kind::UShort:
            return Type::UShort();
        case clang::BuiltinType::Kind::UInt:
            return Type::UInt();
        case clang::BuiltinType::Kind::ULong:
            return Type::ULong();
        case clang::BuiltinType::Kind::ULongLong:
            return Type::ULongLong();
        case clang::BuiltinType::Kind::Float:
            return Type::Float();
        case clang::BuiltinType::Kind::Double:
            return Type::Double();
            // Objective-C does not support the long double type. @encode(long double) returns d, which is the same encoding as for double.
        case clang::BuiltinType::Kind::LongDouble:
            return Type::Double();
            return Type::Selector();

        // ObjCSel, ObjCId and ObjCClass builtin types should never enter in this method because these types should be handled on upper level.
        // The 'SEL' type is represented as pointer to BuiltinType of kind ObjCSel.
        // The 'id' type is actually represented by clang as TypedefType to ObjCObjectPointerType whose pointee is an ObjCObjectType with base BuiltinType::ObjCIdType.
        // This is also valid for ObjCClass type.
        case clang::BuiltinType::Kind::ObjCSel:
        case clang::BuiltinType::Kind::ObjCId:
        case clang::BuiltinType::Kind::ObjCClass:

        // Not supported types
        case clang::BuiltinType::Kind::Int128:
        case clang::BuiltinType::Kind::UInt128:
        case clang::BuiltinType::Kind::Half:
        case clang::BuiltinType::Kind::WChar_S:
        case clang::BuiltinType::Kind::WChar_U:
        case clang::BuiltinType::Kind::Char16:
        case clang::BuiltinType::Kind::Char32:
        case clang::BuiltinType::Kind::NullPtr:
        case clang::BuiltinType::Kind::Overload:
        case clang::BuiltinType::Kind::BoundMember:
        case clang::BuiltinType::Kind::PseudoObject:
        case clang::BuiltinType::Kind::Dependent:
        case clang::BuiltinType::Kind::UnknownAny:
        case clang::BuiltinType::Kind::ARCUnbridgedCast:
        case clang::BuiltinType::Kind::BuiltinFn:
        case clang::BuiltinType::Kind::OCLImage1d:
        case clang::BuiltinType::Kind::OCLImage1dArray:
        case clang::BuiltinType::Kind::OCLImage1dBuffer:
        case clang::BuiltinType::Kind::OCLImage2d:
        case clang::BuiltinType::Kind::OCLImage2dArray:
        case clang::BuiltinType::Kind::OCLImage3d:
        case clang::BuiltinType::Kind::OCLSampler:
        case clang::BuiltinType::Kind::OCLEvent:
            throw TypeCreationException(type->getName(clang::PrintingPolicy(clang::LangOptions())).str(), string("Not supported builtin type."), true);
        default:
            llvm_unreachable("Invalid builtin type.");
    }
}

Type TypeFactory::createFromObjCObjectPointerType(const clang::ObjCObjectPointerType* type) {
    vector<FQName> protocols;
    for (clang::ObjCObjectPointerType::qual_iterator it = type->qual_begin(); it != type->qual_end(); ++it) {
        protocols.push_back(_identifierGenerator.getFqName(**it));
    }
    if(type->isObjCIdType() || type->isObjCQualifiedIdType()) {
        return Type::Id(protocols);
    }
    if(type->isObjCClassType() || type->isObjCQualifiedClassType()) {
        return Type::ClassType(protocols);
    }


    if(clang::ObjCInterfaceDecl *interface = type->getObjectType()->getInterface()) {
        // TODO: Make the check for Protocol more precise (e. g. check the module of interface if is equal to the
        // module of Protocol interface (and maybe file in which is defined, but it may be different in future SDK))
        if (interface->getNameAsString() == "Protocol")
            return Type::ProtocolType();

        return Type::Interface(_identifierGenerator.getFqName(*interface), protocols);
    }

    throw TypeCreationException(type->getObjectType()->getTypeClassName(), "Invalid interface pointer type.", true);
}

Type TypeFactory::createFromPointerType(const clang::PointerType* type) {
    clang::QualType qualPointee = type->getPointeeType();
    const clang::Type *pointee = qualPointee.getTypePtr();

    const clang::Type *canonicalPointee = pointee->getCanonicalTypeInternal().getTypePtr();
    if(const clang::BuiltinType *builtinType = clang::dyn_cast<clang::BuiltinType>(canonicalPointee)) {
        if(builtinType->getKind() == clang::BuiltinType::Kind::ObjCSel)
            return Type::Selector();
        if(builtinType->getKind() == clang::BuiltinType::Kind::Char_S || builtinType->getKind() == clang::BuiltinType::Kind::UChar)
            return Type::CString();
    }

    if(clang::isa<clang::ParenType>(pointee)) {
        // if is a FunctionPointerType don't wrap the type in another pointer type
        return this->create(qualPointee);
    }
    return Type::Pointer(this->create(qualPointee));
}

Type TypeFactory::createFromEnumType(const clang::EnumType* type) {
    return this->create(type->getDecl()->getIntegerType());
}

Type TypeFactory::createFromRecordType(const clang::RecordType* type) {
    clang::RecordDecl *record = type->getDecl()->getDefinition();
    if(!record) // The record is opaque
        return Type::Void();
    if(record->isUnion())
        throw TypeCreationException(type->getTypeClassName(), "The record is union.", true);
    if(!record->isStruct())
        throw TypeCreationException(type->getTypeClassName(), "The record is not a struct.", true);

    try {
        FQName recordName = this->_identifierGenerator.getFqName(*record);
        return Type::Struct(recordName);
    } catch(IdentifierCreationException& e) {
        // The record is anonymous
        std::vector<Meta::RecordField> fields;
        for(clang::RecordDecl::field_iterator it = record->field_begin(); it != record->field_end(); ++it) {
            clang::FieldDecl *field = *it;
            RecordField fieldMeta(_identifierGenerator.getJsName(*field), this->create(field->getType()));
            fields.push_back(fieldMeta);
        }
        return Type::AnonymousStruct(fields);
    }
}

Type TypeFactory::createFromTypedefType(const clang::TypedefType* type) {
    std::vector<string> boolTypedefs { "BOOL", "Boolean" };
    if(isSpecificTypedefType(type, boolTypedefs))
        return Type::Bool();
    if(isSpecificTypedefType(type, "unichar"))
        return Type::Unichar();
    if(isSpecificTypedefType(type, "__builtin_va_list"))
        throw TypeCreationException(type->getTypeClassName(), "VaList type is not supported.", true);
    if(const clang::PointerType *pointerType = type->getCanonicalTypeUnqualified().getTypePtr()->getAs<clang::PointerType>()) {
        if(const clang::RecordType *record = pointerType->getPointeeType().getTypePtr()->getAs<clang::RecordType>()) {
            string recordName = record->getDecl()->getNameAsString();
            if(std::find(tollFreeBridgedTypes.begin(), tollFreeBridgedTypes.end(), recordName) != tollFreeBridgedTypes.end()) {
                // We have found a toll free bridged structure. For now do nothing here.

                // TODO: Maybe there is better way to recognize toll free bridged types (e.g. type->isObjCARCBridgableType()) and get the corresponding type object.
                // Don't forget to change not only the name of the type but its module.
            }
        }
    }

    return this->create(type->getDecl()->getUnderlyingType());
}

Type TypeFactory::createFromVectorType(const clang::VectorType* type) {
    throw TypeCreationException(type->getTypeClassName(), "Vector type is not supported.", true);
}

Type TypeFactory::createFromElaboratedType(const clang::ElaboratedType *type) {
    return this->create(type->getNamedType());
}

Type TypeFactory::createFromAdjustedType(const clang::AdjustedType *type) {
    return this->create(type->getOriginalType());
}

Type TypeFactory::createFromFunctionProtoType(const clang::FunctionProtoType *type) {
    std::vector<Type> signature;
    signature.push_back(this->create(type->getReturnType()));
    for (clang::FunctionProtoType::param_type_iterator it = type->param_type_begin(); it != type->param_type_end(); ++it)
        signature.push_back(this->create(*it));
    return Type::FunctionPointer(signature);
}

Type TypeFactory::createFromFunctionNoProtoType(const clang::FunctionNoProtoType *type) {
    std::vector<Type> signature;
    signature.push_back(this->create(type->getReturnType()));
    return Type::FunctionPointer(signature);
}

Type TypeFactory::createFromParenType(const clang::ParenType *type) {
    return this->create(type->desugar().getTypePtr());
}

bool TypeFactory::isSpecificTypedefType(const clang::TypedefType* type, const std::string& typedefName) {
    const std::vector<std::string> typedefNames { typedefName };
    return this->isSpecificTypedefType(type, typedefNames);
}

bool TypeFactory::isSpecificTypedefType(const clang::TypedefType* type, const std::vector<std::string>& typedefNames) {
    clang::TypedefNameDecl *decl = type->getDecl();
    while(decl) {
        if (std::find(typedefNames.begin(), typedefNames.end(), decl->getNameAsString()) != typedefNames.end()) {
            return true;
        }

        clang::Type const *innerType = decl->getUnderlyingType().getTypePtr();
        if (const clang::TypedefType *innerTypedef = clang::dyn_cast<clang::TypedefType>(innerType)) {
            decl = innerTypedef->getDecl();
        }
        else {
            return false;
        }
    }
    return false;
}