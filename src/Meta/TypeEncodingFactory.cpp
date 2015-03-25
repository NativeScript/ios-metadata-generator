#include "TypeEncodingFactory.h"

using namespace std;
using namespace Meta;

Type TypeEncodingFactory::create(const clang::Type* type) {
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
        throw TypeEncodingCreationException(type->getTypeClassName(), "Unable to create encoding for this type.", true);
    }
    catch(IdentifierCreationException& e) {
        throw TypeEncodingCreationException(type->getTypeClassName(), string("Identifier Error [") + e.whatAsString() + "]", true);
    }
}

Type TypeEncodingFactory::create(const clang::QualType& type) {
    const clang::Type *typePtr = type.getTypePtrOrNull();
    if(typePtr)
        return this->create(typePtr);
    throw TypeEncodingCreationException(type->getTypeClassName(), "Unable to get the inner type of qualified type.", true);
}

Type TypeEncodingFactory::createFromConstantArrayType(const clang::ConstantArrayType* type) {
    return Type::ConstantArray(this->create(type->getElementType()), (int)type->getSize().roundToDouble());
}

Type TypeEncodingFactory::createFromIncompleteArrayType(const clang::IncompleteArrayType* type) {
    return Type::IncompleteArray(this->create(type->getElementType()));
}

Type TypeEncodingFactory::createFromBlockPointerType(const clang::BlockPointerType* type) {
    const clang::Type *canonicalPointee = type->getPointeeType().getTypePtr()->getCanonicalTypeUnqualified().getTypePtr();
    if(const clang::FunctionProtoType *functionType = clang::dyn_cast<clang::FunctionProtoType>(canonicalPointee)) {
        std::vector<Type> signature;
        this->getSignatureOfFunctionProtoType(functionType, signature);
        return Type::Block(signature);
    }
    throw TypeEncodingCreationException(type->getTypeClassName(), "Unable to parse a block type.", true);
}

Type TypeEncodingFactory::createFromBuiltinType(const clang::BuiltinType* type) {
    switch (type->getKind()) {
        case clang::BuiltinType::Kind::Void:
            return Type::Void();
        case clang::BuiltinType::Kind::Bool:
            return Type::Bool();
        case clang::BuiltinType::Kind::Char_S:
            return Type::SignedChar();
        case clang::BuiltinType::Kind::Char_U:
            return Type::UnsignedChar();
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
        case clang::BuiltinType::Kind::ObjCSel:
            return Type::Selector();

        // ObjCId and ObjCClass builtin types should never enter in this method because these types should be handled on upper level.
        // The 'id' type is actually represented by clang as TypedefType to ObjCObjectPointerType whose pointee is an ObjCObjectType with base BuiltinType::ObjCIdType.
        // This is also valid for ObjCClass type.
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
            throw TypeEncodingCreationException(type->getName(clang::PrintingPolicy(clang::LangOptions())).str(), string("Not supported builtin type."), true);
        default:
            llvm_unreachable("Invalid builtin type.");
    }
}

Type TypeEncodingFactory::createFromObjCObjectPointerType(const clang::ObjCObjectPointerType* type) {
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
    if(const clang::ObjCInterfaceType *interfaceType = clang::dyn_cast<clang::ObjCInterfaceType>(type->getObjectType())) {
        clang::ObjCInterfaceDecl *interface = interfaceType->getDecl();
        // TODO: Make the check for Protocol more precise (e. g. check the module of interface if is equal to the
        // module of Protocol interface (and maybe file in which is defined, but it may be different in future SDK))
        if(interface->getNameAsString() == "Protocol")
            return Type::ProtocolType();

        return Type::Interface(_identifierGenerator.getFqName(*interface), protocols);
    }

    throw TypeEncodingCreationException(type->getTypeClassName(), "Invalid interface pointer type.", true);
}

Type TypeEncodingFactory::createFromPointerType(const clang::PointerType* type) {
    clang::QualType qualPointee = type->getPointeeType();
    const clang::Type *pointee = qualPointee.getTypePtr();
    const clang::Type *canonicalPointee = pointee->getCanonicalTypeUnqualified().getTypePtr();

    if(const clang::FunctionProtoType *functionType = clang::dyn_cast<clang::FunctionProtoType>(canonicalPointee)) {
        std::vector<Type> signature;
        this->getSignatureOfFunctionProtoType(functionType, signature);
        return Type::FunctionPointer(signature);
    }
    // TODO: Maybe we can cast canonicalPointee instead of pointee
    if(const clang::BuiltinType *builtinType = clang::dyn_cast<clang::BuiltinType>(pointee)) {
        if(builtinType->getKind() == clang::BuiltinType::Kind::Char_S || builtinType->getKind() == clang::BuiltinType::Kind::UChar)
            return Type::CString();
    }

    return Type::Pointer(this->create(type->getPointeeType()));
}

Type TypeEncodingFactory::createFromEnumType(const clang::EnumType* type) {
    return this->create(type->getDecl()->getIntegerType());
}

Type TypeEncodingFactory::createFromRecordType(const clang::RecordType* type) {
    clang::RecordDecl *record = type->getDecl()->getDefinition();
    if(!record) // The record is opaque
        return Type::Void();
    if(record->isUnion())
        throw TypeEncodingCreationException(type->getTypeClassName(), "The record is union.", true);
    if(!record->isStruct())
        throw TypeEncodingCreationException(type->getTypeClassName(), "The record is not a struct.", true);

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

static std::vector<std::string> tollFreeBridgedTypes = { "CFArrayRef", "CFAttributedStringRef", "CFCalendarRef", "CFCharacterSetRef", "CFDataRef", "CFDateRef", "CFDictionaryRef",
        "CFErrorRef", "CFLocaleRef", "CFMutableArrayRef", "CFMutableAttributedStringRef", "CFMutableCharacterSetRef", "CFMutableDataRef", "CFMutableDictionaryRef", "CFMutableSetRef",
        "CFMutableStringRef", "CFNumberRef", "CFReadStreamRef", "CFRunLoopTimerRef", "CFSetRef", "CFStringRef", "CFTimeZoneRef", "CFURLRef", "CFWriteStreamRef" };

Type TypeEncodingFactory::createFromTypedefType(const clang::TypedefType* type) {
    std::vector<string> boolTypedefs { "BOOL", "Boolean" };
    if(isSpecificTypedefType(type, boolTypedefs))
        return Type::Bool();
    if(isSpecificTypedefType(type, "unichar"))
        return Type::Unichar();
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

Type TypeEncodingFactory::createFromVectorType(const clang::VectorType* type) {
    throw TypeEncodingCreationException(type->getTypeClassName(), "Vector type is not supported.", true);
}

Type TypeEncodingFactory::createFromElaboratedType(const clang::ElaboratedType *type) {
    return this->create(type->getNamedType());
}

void TypeEncodingFactory::getSignatureOfFunctionProtoType(const clang::FunctionProtoType* type, vector<Type>& signature) {
    signature.push_back(this->create(type->getReturnType()));
    for (clang::FunctionProtoType::param_type_iterator it = type->param_type_begin(); it != type->param_type_end(); ++it)
        signature.push_back(this->create(*it));
}

bool TypeEncodingFactory::isSpecificTypedefType(const clang::TypedefType* type, const std::string& typedefName) {
    const std::vector<std::string> typedefNames { typedefName };
    return this->isSpecificTypedefType(type, typedefNames);
}

bool TypeEncodingFactory::isSpecificTypedefType(const clang::TypedefType* type, const std::vector<std::string>& typedefNames) {
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