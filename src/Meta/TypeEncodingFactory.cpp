#include <Tcl/tcl.h>
#include "TypeEncodingFactory.h"

using namespace clang;
using namespace std;
using namespace Meta;

TypeEncoding TypeEncodingFactory::create(const Type* type) {
    try {
        if (const ConstantArrayType *concreteType = dyn_cast<ConstantArrayType>(type))
            return createFromConstantArrayType(concreteType);
        if (const IncompleteArrayType *concreteType = dyn_cast<IncompleteArrayType>(type))
            return createFromIncompleteArrayType(concreteType);
        if (const PointerType *concreteType = dyn_cast<PointerType>(type))
            return createFromPointerType(concreteType);
        if (const BlockPointerType *concreteType = dyn_cast<BlockPointerType>(type))
            return createFromBlockPointerType(concreteType);
        if (const BuiltinType *concreteType = dyn_cast<BuiltinType>(type))
            return createFromBuiltinType(concreteType);
        if (const ObjCObjectPointerType *concreteType = dyn_cast<ObjCObjectPointerType>(type))
            return createFromObjCObjectPointerType(concreteType);
        if (const RecordType *concreteType = dyn_cast<RecordType>(type))
            return createFromRecordType(concreteType);
        if (const EnumType *concreteType = dyn_cast<EnumType>(type))
            return createFromEnumType(concreteType);
        if (const VectorType *concreteType = dyn_cast<VectorType>(type))
            return createFromVectorType(concreteType);
        if (const TypedefType *concreteType = dyn_cast<TypedefType>(type))
            return createFromTypedefType(concreteType);
        if (const ElaboratedType *concreteType = dyn_cast<ElaboratedType>(type))
            return createFromElaboratedType(concreteType);
        throw TypeEncodingCreationException(type->getTypeClassName(), "Unable to create encoding for this type.", true);
    }
    catch(IdentifierCreationException& e) {
        throw TypeEncodingCreationException(type->getTypeClassName(), string("Identifier Error [") + e.whatAsString() + "]", true);
    }
}

TypeEncoding TypeEncodingFactory::create(const QualType& type) {
    const Type *typePtr = type.getTypePtrOrNull();
    if(typePtr)
        return this->create(typePtr);
    throw TypeEncodingCreationException(type->getTypeClassName(), "Unable to get the inner type of qualified type.", true);
}

TypeEncoding TypeEncodingFactory::createFromConstantArrayType(const ConstantArrayType* type) {
    return ConstantArrayEncoding(this->create(type->getElementType()), (int)type->getSize().roundToDouble());
}

TypeEncoding TypeEncodingFactory::createFromIncompleteArrayType(const IncompleteArrayType* type) {
    return IncompleteArrayEncoding(this->create(type->getElementType()));
}

TypeEncoding TypeEncodingFactory::createFromBlockPointerType(const BlockPointerType* type) {
    const Type *canonicalPointee = type->getPointeeType().getTypePtr()->getCanonicalTypeUnqualified().getTypePtr();
    if(const FunctionProtoType *functionType = dyn_cast<FunctionProtoType>(canonicalPointee)) {
        std::vector<TypeEncoding> signature;
        this->getSignatureOfFunctionProtoType(functionType, signature);
        return BlockEncoding(signature);
    }
    throw TypeEncodingCreationException(type->getTypeClassName(), "Unable to parse a block type.", true);
}

TypeEncoding TypeEncodingFactory::createFromBuiltinType(const BuiltinType* type) {
    switch (type->getKind()) {
        case BuiltinType::Kind::Void:
            return VoidEncoding();
        case BuiltinType::Kind::Bool:
            return BoolEncoding();
        case BuiltinType::Kind::Char_S:
            return SignedCharEncoding();
        case BuiltinType::Kind::Char_U:
            return UnsignedCharEncoding();
        case BuiltinType::Kind::SChar:
            return SignedCharEncoding();
        case BuiltinType::Kind::Short:
            return ShortEncoding();
        case BuiltinType::Kind::Int:
            return IntEncoding();
        case BuiltinType::Kind::Long:
            return LongEncoding();
        case BuiltinType::Kind::LongLong:
            return LongLongEncoding();
        case BuiltinType::Kind::UChar:
            return UnsignedCharEncoding();
        case BuiltinType::Kind::UShort:
            return UShortEncoding();
        case BuiltinType::Kind::UInt:
            return UIntEncoding();
        case BuiltinType::Kind::ULong:
            return ULongEncoding();
        case BuiltinType::Kind::ULongLong:
            return ULongLongEncoding();
        case BuiltinType::Kind::Float:
            return FloatEncoding();
        case BuiltinType::Kind::Double:
            return DoubleEncoding();
            // Objective-C does not support the long double type. @encode(long double) returns d, which is the same encoding as for double.
        case BuiltinType::Kind::LongDouble:
            return DoubleEncoding();
        case BuiltinType::Kind::ObjCSel:
            return SelectorEncoding();

        // ObjCId and ObjCClass builtin types should never enter in this method because these types should be handled on upper level.
        // The 'id' type is actually represented by clang as TypedefType to ObjCObjectPointerType whose pointee is an ObjCObjectType with base BuiltinType::ObjCIdType.
        // This is also valid for ObjCClass type.
        case BuiltinType::Kind::ObjCId:
        case BuiltinType::Kind::ObjCClass:

        // Not supported types
        case BuiltinType::Kind::Int128:
        case BuiltinType::Kind::UInt128:
        case BuiltinType::Kind::Half:
        case BuiltinType::Kind::WChar_S:
        case BuiltinType::Kind::WChar_U:
        case BuiltinType::Kind::Char16:
        case BuiltinType::Kind::Char32:
        case BuiltinType::Kind::NullPtr:
        case BuiltinType::Kind::Overload:
        case BuiltinType::Kind::BoundMember:
        case BuiltinType::Kind::PseudoObject:
        case BuiltinType::Kind::Dependent:
        case BuiltinType::Kind::UnknownAny:
        case BuiltinType::Kind::ARCUnbridgedCast:
        case BuiltinType::Kind::BuiltinFn:
        case BuiltinType::Kind::OCLImage1d:
        case BuiltinType::Kind::OCLImage1dArray:
        case BuiltinType::Kind::OCLImage1dBuffer:
        case BuiltinType::Kind::OCLImage2d:
        case BuiltinType::Kind::OCLImage2dArray:
        case BuiltinType::Kind::OCLImage3d:
        case BuiltinType::Kind::OCLSampler:
        case BuiltinType::Kind::OCLEvent:
            throw TypeEncodingCreationException(type->getName(PrintingPolicy(LangOptions())).str(), string("Not supported builtin type."), true);
        default:
            llvm_unreachable("Invalid builtin type.");
    }
}

TypeEncoding TypeEncodingFactory::createFromObjCObjectPointerType(const ObjCObjectPointerType* type) {
    vector<FQName> protocols;
    for (ObjCObjectPointerType::qual_iterator it = type->qual_begin(); it != type->qual_end(); ++it) {
        protocols.push_back(_identifierGenerator.getFqName(**it));
    }
    if(type->isObjCIdType() || type->isObjCQualifiedIdType()) {
        return IdEncoding(protocols);
    }
    if(type->isObjCClassType() || type->isObjCQualifiedClassType()) {
        return ClassEncoding(protocols);
    }
    if(const ObjCInterfaceType *interfaceType = dyn_cast<ObjCInterfaceType>(type->getObjectType())) {
        ObjCInterfaceDecl *interface = interfaceType->getDecl();
        // TODO: Make the check for Protocol more precise (e. g. check the module of interface if is equal to the
        // module of Protocol interface (and maybe file in which is defined, but it may be different in future SDK))
        if(interface->getNameAsString() == "Protocol")
            return ProtocolEncoding();

        return InterfaceEncoding(_identifierGenerator.getFqName(*interface), protocols);
    }

    throw TypeEncodingCreationException(type->getTypeClassName(), "Invalid interface pointer type.", true);
}

TypeEncoding TypeEncodingFactory::createFromPointerType(const PointerType* type) {
    QualType qualPointee = type->getPointeeType();
    const Type *pointee = qualPointee.getTypePtr();
    const Type *canonicalPointee = pointee->getCanonicalTypeUnqualified().getTypePtr();

    if(const FunctionProtoType *functionType = dyn_cast<FunctionProtoType>(canonicalPointee)) {
        std::vector<TypeEncoding> signature;
        this->getSignatureOfFunctionProtoType(functionType, signature);
        return FunctionPointerEncoding(signature);
    }
    // TODO: Maybe we can cast canonicalPointee instead of pointee
    if(const BuiltinType *builtinType = dyn_cast<BuiltinType>(pointee)) {
        if(builtinType->getKind() == BuiltinType::Kind::Char_S || builtinType->getKind() == BuiltinType::Kind::UChar)
            return CStringEncoding();
    }

    return PointerEncoding(this->create(type->getPointeeType()));
}

TypeEncoding TypeEncodingFactory::createFromEnumType(const EnumType* type) {
    return this->create(type->getDecl()->getIntegerType());
}

TypeEncoding TypeEncodingFactory::createFromRecordType(const RecordType* type) {
    RecordDecl *record = type->getDecl()->getDefinition();
    if(!record) // The record is opaque
        return VoidEncoding();
    if(record->isUnion())
        throw TypeEncodingCreationException(type->getTypeClassName(), "The record is union.", true);
    if(!record->isStruct())
        throw TypeEncodingCreationException(type->getTypeClassName(), "The record is not a struct.", true);

    try {
        FQName recordName = this->_identifierGenerator.getFqName(*record);
        return StructEncoding(recordName);
    } catch(IdentifierCreationException& e) {
        // The record is anonymous
        std::vector<std::string> fieldNames;
        std::vector<TypeEncoding> fieldEncodings;
        for(RecordDecl::field_iterator it = record->field_begin(); it != record->field_end(); ++it) {
            FieldDecl *field = *it;
            fieldNames.push_back(field->getNameAsString());
            fieldEncodings.push_back(this->create(field->getType()));
        }
        return AnonymousStructEncoding(fieldNames, fieldEncodings);
    }
}

static std::vector<std::string> tollFreeBridgedTypes = { "CFArrayRef", "CFAttributedStringRef", "CFCalendarRef", "CFCharacterSetRef", "CFDataRef", "CFDateRef", "CFDictionaryRef",
        "CFErrorRef", "CFLocaleRef", "CFMutableArrayRef", "CFMutableAttributedStringRef", "CFMutableCharacterSetRef", "CFMutableDataRef", "CFMutableDictionaryRef", "CFMutableSetRef",
        "CFMutableStringRef", "CFNumberRef", "CFReadStreamRef", "CFRunLoopTimerRef", "CFSetRef", "CFStringRef", "CFTimeZoneRef", "CFURLRef", "CFWriteStreamRef" };

TypeEncoding TypeEncodingFactory::createFromTypedefType(const TypedefType* type) {
    std::vector<string> boolTypedefs { "BOOL", "Boolean" };
    if(isSpecificTypedefType(type, boolTypedefs))
        return BoolEncoding();
    if(isSpecificTypedefType(type, "unichar"))
        return UnicharEncoding();
    if(const PointerType *pointerType = type->getCanonicalTypeUnqualified().getTypePtr()->getAs<PointerType>()) {
        if(const RecordType *record = pointerType->getPointeeType().getTypePtr()->getAs<RecordType>()) {
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

TypeEncoding TypeEncodingFactory::createFromVectorType(const VectorType* type) {
    throw TypeEncodingCreationException(type->getTypeClassName(), "Vector type is not supported.", true);
}

TypeEncoding TypeEncodingFactory::createFromElaboratedType(const clang::ElaboratedType *type) {
    return this->create(type->getNamedType());
}

void TypeEncodingFactory::getSignatureOfFunctionProtoType(const FunctionProtoType* type, vector<TypeEncoding>& signature) {
    signature.push_back(this->create(type->getReturnType()));
    for (FunctionProtoType::param_type_iterator it = type->param_type_begin(); it != type->param_type_end(); ++it)
        signature.push_back(this->create(*it));
}

bool TypeEncodingFactory::isSpecificTypedefType(const TypedefType* type, const std::string& typedefName) {
    const std::vector<std::string> typedefNames { typedefName };
    return this->isSpecificTypedefType(type, typedefNames);
}

bool TypeEncodingFactory::isSpecificTypedefType(const TypedefType* type, const std::vector<std::string>& typedefNames) {
    TypedefNameDecl *decl = type->getDecl();
    while(decl) {
        if (std::find(typedefNames.begin(), typedefNames.end(), decl->getNameAsString()) != typedefNames.end()) {
            return true;
        }

        Type const *innerType = decl->getUnderlyingType().getTypePtr();
        if (const TypedefType *innerTypedef = dyn_cast<TypedefType>(innerType)) {
            decl = innerTypedef->getDecl();
        }
        else {
            return false;
        }
    }
    return false;
}