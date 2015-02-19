#include "test.h"
#include "utils/metaReaderPrivate.h"

TEST (YamlTests, TestParseName) {
    YAML::Node node = YAML::Load("Module: Foundation.NSObject\nName: NSCoding");
    FQName name = parseName(node);
    EXPECT_FALSE(name.isEmpty());
    EXPECT_EQ("NSCoding", name.name);
    EXPECT_EQ("Foundation.NSObject", name.module);
}

TEST (YamlTests, TestParseEmptyName) {
    YAML::Node node = YAML::Load("Base: ");
    FQName name = parseName(node);
    EXPECT_TRUE(name.isEmpty());
    EXPECT_EQ("", name.name);
    EXPECT_EQ("", name.module);
}

TEST (YamlTests, TestParseEmptyModule) {
    YAML::Node node = YAML::Load("Module: \nName: NSCoding");
    FQName name = parseName(node);
    EXPECT_FALSE(name.isEmpty());
    EXPECT_EQ("NSCoding", name.name);
    EXPECT_EQ("", name.module);
}

/// Version

TEST (YamlTests, TestParseVersion_Major) {
    Version v = parseVersion("8");
    EXPECT_EQ(8, v.Major);
    EXPECT_EQ(-1, v.Minor);
    EXPECT_EQ(-1, v.SubMinor);
}

TEST (YamlTests, TestParseVersion_MajorMinor) {
    Version v = parseVersion("8.0");
    EXPECT_EQ(8, v.Major);
    EXPECT_EQ(0, v.Minor);
    EXPECT_EQ(-1, v.SubMinor);
}

TEST (YamlTests, TestParseVersion_MajorMinorSubMinor) {
    Version v = parseVersion("8.0.1");
    EXPECT_EQ(8, v.Major);
    EXPECT_EQ(0, v.Minor);
    EXPECT_EQ(1, v.SubMinor);
}

/// Flags

TEST (YamlTests, TestParseFlags_HasName) {
    YAML::Node node = YAML::Load("- HasName");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName, flags);
}

TEST (YamlTests, TestParseFlags_HasName_IsIosAppExtensionAvailable) {
    YAML::Node node = YAML::Load("- HasName\n- IsIosAppExtensionAvailable");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName | meta::MetaFlags::IsIosAppExtensionAvailable, flags);
}

TEST (YamlTests, TestParseFlags_HasName_IsIosAppExtensionAvailable_FunctionIsVariadic) {
    YAML::Node node = YAML::Load("- HasName\n"
            "- IsIosAppExtensionAvailable\n"
            "- FunctionIsVariadic");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName | meta::MetaFlags::IsIosAppExtensionAvailable |
            meta::MetaFlags::FunctionIsVariadic, flags);
}

TEST (YamlTests, TestParseFlags_HasName_IsIosAppExtensionAvailable_FunctionIsVariadic_FunctionOwnsReturnedCocoaObject) {
    YAML::Node node = YAML::Load("- HasName\n"
            "- IsIosAppExtensionAvailable\n"
            "- FunctionIsVariadic\n"
            "- FunctionOwnsReturnedCocoaObject");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName | meta::MetaFlags::IsIosAppExtensionAvailable |
            meta::MetaFlags::FunctionIsVariadic | meta::MetaFlags::FunctionOwnsReturnedCocoaObject, flags);
}

TEST (YamlTests, TestParseFlags_HasName_IsIosAppExtensionAvailable_FunctionIsVariadic_FunctionOwnsReturnedCocoaObject_MemberIsLocalJsNameDuplicate) {
    YAML::Node node = YAML::Load("- HasName\n"
            "- IsIosAppExtensionAvailable\n"
            "- FunctionIsVariadic\n"
            "- FunctionOwnsReturnedCocoaObject\n"
            "- MemberIsLocalJsNameDuplicate");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName | meta::MetaFlags::IsIosAppExtensionAvailable |
            meta::MetaFlags::FunctionIsVariadic | meta::MetaFlags::FunctionOwnsReturnedCocoaObject |
            meta::MetaFlags::MemberIsLocalJsNameDuplicate, flags);
}

TEST (YamlTests, TestParseFlags_HasName_IsIosAppExtensionAvailable_FunctionIsVariadic_FunctionOwnsReturnedCocoaObject_MemberIsLocalJsNameDuplicate_MemberHasJsNameDuplicateInHierarchy) {
    YAML::Node node = YAML::Load("- HasName\n"
            "- IsIosAppExtensionAvailable\n"
            "- FunctionIsVariadic\n"
            "- FunctionOwnsReturnedCocoaObject\n"
            "- MemberIsLocalJsNameDuplicate\n"
            "- MemberHasJsNameDuplicateInHierarchy");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName | meta::MetaFlags::IsIosAppExtensionAvailable |
            meta::MetaFlags::FunctionIsVariadic | meta::MetaFlags::FunctionOwnsReturnedCocoaObject |
            meta::MetaFlags::MemberIsLocalJsNameDuplicate | meta::MetaFlags::MemberHasJsNameDuplicateInHierarchy, flags);
}

TEST (YamlTests, TestParseFlags_HasName_IsIosAppExtensionAvailable_MethodIsVariadic) {
    YAML::Node node = YAML::Load("- HasName\n"
            "- IsIosAppExtensionAvailable\n"
            "- MethodIsVariadic");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName | meta::MetaFlags::IsIosAppExtensionAvailable |
            meta::MetaFlags::MethodIsVariadic, flags);
}

TEST (YamlTests, TestParseFlags_HasName_IsIosAppExtensionAvailable_MethodIsVariadic_MethodIsNullTerminatedVariadic) {
    YAML::Node node = YAML::Load("- HasName\n"
            "- IsIosAppExtensionAvailable\n"
            "- MethodIsVariadic\n"
            "- MethodIsNullTerminatedVariadic");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName | meta::MetaFlags::IsIosAppExtensionAvailable |
            meta::MetaFlags::MethodIsVariadic | meta::MetaFlags::MethodIsNullTerminatedVariadic, flags);
}

TEST (YamlTests, TestParseFlags_HasName_IsIosAppExtensionAvailable_MethodIsVariadic_MethodIsNullTerminatedVariadic_MethodOwnsReturnedCocoaObject) {
    YAML::Node node = YAML::Load("- HasName\n"
            "- IsIosAppExtensionAvailable\n"
            "- MethodIsVariadic\n"
            "- MethodIsNullTerminatedVariadic\n"
            "- MethodOwnsReturnedCocoaObject");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName | meta::MetaFlags::IsIosAppExtensionAvailable |
            meta::MetaFlags::MethodIsVariadic | meta::MetaFlags::MethodIsNullTerminatedVariadic |
            meta::MetaFlags::MethodOwnsReturnedCocoaObject, flags);
}

TEST (YamlTests, TestParseFlags_HasName_IsIosAppExtensionAvailable_MethodIsVariadic_MethodIsNullTerminatedVariadic_MethodOwnsReturnedCocoaObject_MemberIsLocalJsNameDuplicate) {
    YAML::Node node = YAML::Load("- HasName\n"
            "- IsIosAppExtensionAvailable\n"
            "- MethodIsVariadic\n"
            "- MethodIsNullTerminatedVariadic\n"
            "- MethodOwnsReturnedCocoaObject\n"
            "- MemberIsLocalJsNameDuplicate");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName | meta::MetaFlags::IsIosAppExtensionAvailable |
            meta::MetaFlags::MethodIsVariadic | meta::MetaFlags::MethodIsNullTerminatedVariadic |
            meta::MetaFlags::MethodOwnsReturnedCocoaObject | meta::MetaFlags::MemberIsLocalJsNameDuplicate , flags);
}

TEST (YamlTests, TestParseFlags_HasName_IsIosAppExtensionAvailable_MethodIsVariadic_MethodIsNullTerminatedVariadic_MethodOwnsReturnedCocoaObject_MemberIsLocalJsNameDuplicate_MemberHasJsNameDuplicateInHierarchy) {
    YAML::Node node = YAML::Load("- HasName\n"
            "- IsIosAppExtensionAvailable\n"
            "- MethodIsVariadic\n"
            "- MethodIsNullTerminatedVariadic\n"
            "- MethodOwnsReturnedCocoaObject\n"
            "- MemberIsLocalJsNameDuplicate\n"
            "- MemberHasJsNameDuplicateInHierarchy");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName | meta::MetaFlags::IsIosAppExtensionAvailable |
            meta::MetaFlags::MethodIsVariadic | meta::MetaFlags::MethodIsNullTerminatedVariadic |
            meta::MetaFlags::MethodOwnsReturnedCocoaObject | meta::MetaFlags::MemberIsLocalJsNameDuplicate |
            meta::MetaFlags::MemberHasJsNameDuplicateInHierarchy , flags);
}

TEST (YamlTests, TestParseFlags_HasName_IsIosAppExtensionAvailable_PropertyHasGetter) {
    YAML::Node node = YAML::Load("- HasName\n"
            "- IsIosAppExtensionAvailable\n"
            "- PropertyHasGetter");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName | meta::MetaFlags::IsIosAppExtensionAvailable |
            meta::MetaFlags::PropertyHasGetter , flags);
}

TEST (YamlTests, TestParseFlags_HasName_IsIosAppExtensionAvailable_PropertyHasGetter_PropertyHasSetter) {
    YAML::Node node = YAML::Load("- HasName\n"
            "- IsIosAppExtensionAvailable\n"
            "- PropertyHasGetter\n"
            "- PropertyHasSetter");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName | meta::MetaFlags::IsIosAppExtensionAvailable |
            meta::MetaFlags::PropertyHasGetter | meta::MetaFlags::PropertyHasSetter , flags);
}

TEST (YamlTests, TestParseFlags_HasName_IsIosAppExtensionAvailable_PropertyHasGetter_PropertyHasSetter_MemberIsLocalJsNameDuplicate) {
    YAML::Node node = YAML::Load("- HasName\n"
            "- IsIosAppExtensionAvailable\n"
            "- PropertyHasGetter\n"
            "- PropertyHasSetter\n"
            "- MemberIsLocalJsNameDuplicate");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName | meta::MetaFlags::IsIosAppExtensionAvailable |
            meta::MetaFlags::PropertyHasGetter | meta::MetaFlags::PropertyHasSetter |
            meta::MetaFlags::MemberIsLocalJsNameDuplicate , flags);
}

TEST (YamlTests, TestParseFlags_HasName_IsIosAppExtensionAvailable_PropertyHasGetter_PropertyHasSetter_MemberIsLocalJsNameDuplicate_MemberHasJsNameDuplicateInHierarchy) {
    YAML::Node node = YAML::Load("- HasName\n"
            "- IsIosAppExtensionAvailable\n"
            "- PropertyHasGetter\n"
            "- PropertyHasSetter\n"
            "- MemberIsLocalJsNameDuplicate\n"
            "- MemberHasJsNameDuplicateInHierarchy");
    meta::MetaFlags flags = parseFlags(node);
    EXPECT_EQ(meta::MetaFlags::HasName | meta::MetaFlags::IsIosAppExtensionAvailable |
            meta::MetaFlags::PropertyHasGetter | meta::MetaFlags::PropertyHasSetter |
            meta::MetaFlags::MemberIsLocalJsNameDuplicate | meta::MetaFlags::MemberHasJsNameDuplicateInHierarchy , flags);
}

// type encoding

TEST (YamlTests, TestParseTypeEncoding_Unknown) {
    YAML::Node node = YAML::Load("Type: Unknown");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::UnknownEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_VaList) {
    YAML::Node node = YAML::Load("Type: VaList");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::VaListEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Protocol) {
    YAML::Node node = YAML::Load("Type: Protocol");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::ProtocolEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Void) {
    YAML::Node node = YAML::Load("Type: Void");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::VoidEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Bool) {
    YAML::Node node = YAML::Load("Type: Bool");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::BoolEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Short) {
    YAML::Node node = YAML::Load("Type: Short");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::ShortEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Ushort) {
    YAML::Node node = YAML::Load("Type: Ushort");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::UShortEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Int) {
    YAML::Node node = YAML::Load("Type: Int");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::IntEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_UInt) {
    YAML::Node node = YAML::Load("Type: UInt");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::UIntEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Long) {
    YAML::Node node = YAML::Load("Type: Long");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::LongEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_ULong) {
    YAML::Node node = YAML::Load("Type: ULong");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::ULongEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_LongLong) {
    YAML::Node node = YAML::Load("Type: LongLong");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::LongLongEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Char) {
    YAML::Node node = YAML::Load("Type: Char");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::SignedCharEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_UChar) {
    YAML::Node node = YAML::Load("Type: UChar");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::UnsignedCharEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Unichar) {
    YAML::Node node = YAML::Load("Type: Unichar");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::UnicharEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_CharS) {
    YAML::Node node = YAML::Load("Type: CharS");
    EXPECT_EXIT(parseTypeEncoding(node), ::testing::ExitedWithCode(202), "Error: unknown type encoding");
}

TEST (YamlTests, TestParseTypeEncoding_CString) {
    YAML::Node node = YAML::Load("Type: CString");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::CStringEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Float) {
    YAML::Node node = YAML::Load("Type: Float");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::FloatEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Double) {
    YAML::Node node = YAML::Load("Type: Double");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::DoubleEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Selector) {
    YAML::Node node = YAML::Load("Type: Selector");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::SelectorEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Class) {
    YAML::Node node = YAML::Load("Type: Class");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::ClassEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Instancetype) {
    YAML::Node node = YAML::Load("Type: Instancetype");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    EXPECT_TRUE(dynamic_cast<typeEncoding::InstancetypeEncoding*>(typeEncoding.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Id) {
    YAML::Node node = YAML::Load("Type: Id\n"
            "WithProtocols:\n"
            "- Module: CloudKit.CKRecord\n"
            "  Name: CKRecordValue");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    typeEncoding::IdEncoding* idEncoding = dynamic_cast<typeEncoding::IdEncoding*>(typeEncoding.get());
    EXPECT_TRUE(idEncoding);
    EXPECT_EQ(1, idEncoding->protocols.size());
    FQName f = idEncoding->protocols[0];
    EXPECT_EQ("CloudKit.CKRecord", f.module);
    EXPECT_EQ("CKRecordValue", f.name);
}

TEST (YamlTests, TestParseTypeEncoding_ConstantArray) {
    YAML::Node node = YAML::Load("Type: ConstantArray\n"
            "ArrayType:\n"
            "  Type: Float\n"
            "Size: 4");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    typeEncoding::ConstantArrayEncoding* constantArrayEncoding = dynamic_cast<typeEncoding::ConstantArrayEncoding*>(typeEncoding.get());
    EXPECT_TRUE(constantArrayEncoding);
    EXPECT_TRUE(dynamic_cast<typeEncoding::FloatEncoding*>(constantArrayEncoding->elementType.get()));
    EXPECT_EQ(4, constantArrayEncoding->size);
}

TEST (YamlTests, TestParseTypeEncoding_IncompleteArray) {
    YAML::Node node = YAML::Load("Type: IncompleteArray\n"
            "ArrayType:\n"
            "  Type: Float");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    typeEncoding::IncompleteArrayEncoding* incompleteArrayEncoding = dynamic_cast<typeEncoding::IncompleteArrayEncoding*>(typeEncoding.get());
    EXPECT_TRUE(incompleteArrayEncoding);
    EXPECT_TRUE(dynamic_cast<typeEncoding::FloatEncoding*>(incompleteArrayEncoding->elementType.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Interface) {
    YAML::Node node = YAML::Load("Type: Interface\n"
            "Module: ObjectiveC.NSObject\n"
            "Name: NSObject");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    typeEncoding::InterfaceEncoding* interfaceEncoding = dynamic_cast<typeEncoding::InterfaceEncoding*>(typeEncoding.get());
    EXPECT_TRUE(interfaceEncoding);
    FQName f = interfaceEncoding->name;
    EXPECT_EQ("ObjectiveC.NSObject", f.module);
    EXPECT_EQ("NSObject", f.name);
}

TEST (YamlTests, TestParseTypeEncoding_FunctionPointer) {
    YAML::Node node = YAML::Load("Type: FunctionPointer\n"
            "Signature:\n"
            "- Type: Pointer\n"
            "  PointerType:\n"
            "    Type: Float\n"
            "- Type: Pointer\n"
            "  PointerType:\n"
            "    Type: Float\n"
            "- Type: ULong\n"
            "- Type: Pointer\n"
            "  PointerType:\n"
            "    Type: Void");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    typeEncoding::FunctionEncoding* functionEncoding = dynamic_cast<typeEncoding::FunctionEncoding*>(typeEncoding.get());
    EXPECT_TRUE(functionEncoding);
    EXPECT_EQ(4, functionEncoding->functionCall.size());
    EXPECT_TRUE(dynamic_cast<typeEncoding::PointerEncoding*>(functionEncoding->functionCall[0].get()));
    EXPECT_TRUE(dynamic_cast<typeEncoding::PointerEncoding*>(functionEncoding->functionCall[1].get()));
    EXPECT_TRUE(dynamic_cast<typeEncoding::ULongEncoding*>(functionEncoding->functionCall[2].get()));
    EXPECT_TRUE(dynamic_cast<typeEncoding::PointerEncoding*>(functionEncoding->functionCall[3].get()));
}

TEST (YamlTests, TestParseTypeEncoding_Block) {
    YAML::Node node = YAML::Load("Type: Block\n"
            "Signature:\n"
            "- Type: Bool\n"
            "- Type: Interface\n"
            "  Module: Foundation.NSError\n"
            "  Name: NSError");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    typeEncoding::BlockEncoding* blockEncoding = dynamic_cast<typeEncoding::BlockEncoding*>(typeEncoding.get());
    EXPECT_TRUE(blockEncoding);
    EXPECT_EQ(2, blockEncoding->blockCall.size());
    EXPECT_TRUE(dynamic_cast<typeEncoding::BoolEncoding*>(blockEncoding->blockCall[0].get()));
    EXPECT_TRUE(dynamic_cast<typeEncoding::InterfaceEncoding*>(blockEncoding->blockCall[1].get()));
}

TEST (YamlTests, TestParseTypeEncoding_Pointer) {
    YAML::Node node = YAML::Load("Type: Pointer\n"
            "PointerType:\n"
            "  Type: Float");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    typeEncoding::PointerEncoding*pointerEncoding = dynamic_cast<typeEncoding::PointerEncoding*>(typeEncoding.get());
    EXPECT_TRUE(pointerEncoding);
    EXPECT_TRUE(dynamic_cast<typeEncoding::FloatEncoding*>(pointerEncoding->target.get()));
}

TEST (YamlTests, TestParseTypeEncoding_Struct) {
    YAML::Node node = YAML::Load("Type: Struct\n"
            "Module: Accelerate.vecLib.vDSP\n"
            "Name: DSPComplex");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    typeEncoding::StructEncoding* structEncoding = dynamic_cast<typeEncoding::StructEncoding*>(typeEncoding.get());
    EXPECT_TRUE(structEncoding);
    EXPECT_EQ("Accelerate.vecLib.vDSP", structEncoding->name.module);
    EXPECT_EQ("DSPComplex", structEncoding->name.name);
}

TEST (YamlTests, TestParseTypeEncoding_Union) {
    YAML::Node node = YAML::Load("Type: Union\n"
            "Module: Accelerate.vecLib.vDSP\n"
            "Name: DSPComplex");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    typeEncoding::UnionEncoding* unionEncoding = dynamic_cast<typeEncoding::UnionEncoding*>(typeEncoding.get());
    EXPECT_TRUE(unionEncoding);
    EXPECT_EQ("Accelerate.vecLib.vDSP", unionEncoding->name.module);
    EXPECT_EQ("DSPComplex", unionEncoding->name.name);
}

TEST (YamlTests, TestParseTypeEncoding_PureInterface) {
    YAML::Node node = YAML::Load("Type: PureInterface\n"
            "Module: Accelerate.vecLib.vDSP\n"
            "Name: DSPComplex");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    typeEncoding::InterfaceDeclarationEncoding* declarationEncoding = dynamic_cast<typeEncoding::InterfaceDeclarationEncoding*>(typeEncoding.get());
    EXPECT_TRUE(declarationEncoding);
    EXPECT_EQ("Accelerate.vecLib.vDSP", declarationEncoding->name.module);
    EXPECT_EQ("DSPComplex", declarationEncoding->name.name);
}

TEST (YamlTests, TestParseTypeEncoding_AnonymousStruct) {
    YAML::Node node = YAML::Load("Type: AnonymousStruct\n"
            "Fields:\n"
            "- Name: tqe_next\n"
            "  Signature:\n"
            "    Type: Pointer\n"
            "    PointerType:\n"
            "      Type: Struct\n"
            "      Module: Darwin.sys.ucred\n"
            "      Name: ucred\n"
            "- Name: tqe_prev\n"
            "  Signature:\n"
            "    Type: Pointer\n"
            "    PointerType:\n"
            "      Type: Pointer\n"
            "      PointerType:\n"
            "        Type: Struct\n"
            "        Module: Darwin.sys.ucred\n"
            "        Name: ucred");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    typeEncoding::AnonymousStructEncoding* structEncoding = dynamic_cast<typeEncoding::AnonymousStructEncoding*>(typeEncoding.get());
    EXPECT_TRUE(structEncoding);
    EXPECT_EQ(2, structEncoding->fieldNames.size());
    EXPECT_EQ(structEncoding->fieldNames.size(), structEncoding->fieldEncodings.size());
    EXPECT_EQ("tqe_next", structEncoding->fieldNames[0]);
    EXPECT_EQ("tqe_prev", structEncoding->fieldNames[1]);
    EXPECT_TRUE(dynamic_cast<typeEncoding::PointerEncoding*>(structEncoding->fieldEncodings[0].get()));
    EXPECT_TRUE(dynamic_cast<typeEncoding::PointerEncoding*>(structEncoding->fieldEncodings[1].get()));
}

TEST (YamlTests, TestParseTypeEncoding_AnonymousUnion) {
    YAML::Node node = YAML::Load("Type: AnonymousUnion\n"
            "Fields:\n"
            "- Name: tqe_next\n"
            "  Signature:\n"
            "    Type: Pointer\n"
            "    PointerType:\n"
            "      Type: Struct\n"
            "      Module: Darwin.sys.ucred\n"
            "      Name: ucred\n"
            "- Name: tqe_prev\n"
            "  Signature:\n"
            "    Type: Pointer\n"
            "    PointerType:\n"
            "      Type: Pointer\n"
            "      PointerType:\n"
            "        Type: Struct\n"
            "        Module: Darwin.sys.ucred\n"
            "        Name: ucred");
    unique_ptr<typeEncoding::TypeEncoding> typeEncoding = parseTypeEncoding(node);
    typeEncoding::AnonymousUnionEncoding* unionEncoding = dynamic_cast<typeEncoding::AnonymousUnionEncoding*>(typeEncoding.get());
    EXPECT_TRUE(unionEncoding);
    EXPECT_EQ(2, unionEncoding->fieldNames.size());
    EXPECT_EQ(unionEncoding->fieldNames.size(), unionEncoding->fieldEncodings.size());
    EXPECT_EQ("tqe_next", unionEncoding->fieldNames[0]);
    EXPECT_EQ("tqe_prev", unionEncoding->fieldNames[1]);
    EXPECT_TRUE(dynamic_cast<typeEncoding::PointerEncoding*>(unionEncoding->fieldEncodings[0].get()));
    EXPECT_TRUE(dynamic_cast<typeEncoding::PointerEncoding*>(unionEncoding->fieldEncodings[1].get()));
}

// meta

TEST (YamlTests, TestCreateMeta_Interface) {
    YAML::Node node = YAML::Load("Name: 'NSObject'\n"
            "JsName: 'NSObject'\n"
            "Module: 'ObjectiveC.NSObject'\n"
            "IntroducedIn: 2.0\n"
            "Flags:\n"
            "- IsIosAppExtensionAvailable\n"
            "InstanceMethods:\n"
            "- Name: 'copy'\n"
            "  JsName: 'copy'\n"
            "  Flags:\n"
            "  - IsIosAppExtensionAvailable\n"
            "  - MethodOwnsReturnedCocoaObject\n"
            "  Selector: 'copy'\n"
            "  Signature:\n"
            "  - Type: Id\n"
            "    WithProtocols: []\n"
            "  TypeEncoding: '@8@0:4'\n"
            "StaticMethods:\n"
            "- Name: 'alloc'\n"
            "  JsName: 'alloc'\n"
            "  Flags:\n"
            "  - IsIosAppExtensionAvailable\n"
            "  - MethodOwnsReturnedCocoaObject\n"
            "  Selector: 'alloc'\n"
            "  Signature:\n"
            "  - Type: Instancetype\n"
            "  TypeEncoding: '@8@0:4'\n"
            "Properties:\n"
            "- Name: 'accounts'\n"
            "  JsName: 'accounts'\n"
            "  Flags:\n"
            "  - IsIosAppExtensionAvailable\n"
            "  - PropertyHasGetter\n"
            "  Getter:\n"
            "    Name: 'boolValue'\n"
            "    JsName: 'boolValue'\n"
            "    Flags:\n"
            "    - IsIosAppExtensionAvailable\n"
            "    Selector: 'boolValue'\n"
            "    Signature:\n"
            "    - Type: Bool\n"
            "    TypeEncoding: 'c8@0:4'\n"
            "Protocols:\n"
            "- Module: ObjectiveC.NSObject\n"
            "  Name: NSObjectProtocol\n"
            "Base:\n"
            "  Module: ObjectiveC.NSObject\n"
            "  Name: NSObject\n"
            "Type: Interface");

    unique_ptr<meta::Meta> mm = createMeta(node);
    meta::InterfaceMeta* meta = dynamic_cast<meta::InterfaceMeta*>(mm.get());
    EXPECT_TRUE(meta);
    EXPECT_EQ("NSObject", meta->name);
    EXPECT_EQ("NSObject", meta->jsName);
    EXPECT_EQ("ObjectiveC.NSObject", meta->module);
    EXPECT_EQ("NSObject", meta->baseName.name);
    EXPECT_EQ("ObjectiveC.NSObject", meta->baseName.module);
    EXPECT_EQ(2, meta->introducedIn.Major);
    EXPECT_EQ(0, meta->introducedIn.Minor);
    EXPECT_EQ(-1, meta->introducedIn.SubMinor);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable, meta->flags);
    EXPECT_EQ(meta::SymbolType::Interface, meta->type);

    EXPECT_EQ(1, meta->instanceMethods.size());
    EXPECT_EQ("copy", meta->instanceMethods[0].name);
    EXPECT_EQ("copy", meta->instanceMethods[0].jsName);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable | meta::MetaFlags::MethodOwnsReturnedCocoaObject , meta->instanceMethods[0].flags);
    EXPECT_EQ("copy", meta->instanceMethods[0].selector);
    EXPECT_EQ(1, meta->instanceMethods[0].signature.size());
    EXPECT_TRUE(dynamic_cast<typeEncoding::IdEncoding*>(meta->instanceMethods[0].signature[0].get()));
    EXPECT_EQ("@8@0:4", meta->instanceMethods[0].typeEncoding);

    EXPECT_EQ(1, meta->staticMethods.size());
    EXPECT_EQ("alloc", meta->staticMethods[0].name);
    EXPECT_EQ("alloc", meta->staticMethods[0].jsName);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable | meta::MetaFlags::MethodOwnsReturnedCocoaObject , meta->staticMethods[0].flags);
    EXPECT_EQ("alloc", meta->staticMethods[0].selector);
    EXPECT_EQ(1, meta->staticMethods[0].signature.size());
    EXPECT_TRUE(dynamic_cast<typeEncoding::InstancetypeEncoding*>(meta->staticMethods[0].signature[0].get()));
    EXPECT_EQ("@8@0:4", meta->staticMethods[0].typeEncoding);

    EXPECT_EQ(1, meta->properties.size());
    EXPECT_EQ("accounts", meta->properties[0].name);
    EXPECT_EQ("accounts", meta->properties[0].jsName);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable | meta::MetaFlags::PropertyHasGetter , meta->properties[0].flags);
    EXPECT_TRUE(meta->properties[0].getter.get());
    EXPECT_EQ("boolValue", meta->properties[0].getter->name);
    EXPECT_EQ("boolValue", meta->properties[0].getter->jsName);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable, meta->properties[0].getter->flags);
    EXPECT_EQ("boolValue", meta->properties[0].getter->selector);
    EXPECT_EQ(1, meta->properties[0].getter->signature.size());
    EXPECT_TRUE(dynamic_cast<typeEncoding::BoolEncoding*>(meta->properties[0].getter->signature[0].get()));
    EXPECT_EQ("c8@0:4", meta->properties[0].getter->typeEncoding);

    EXPECT_EQ(1, meta->protocols.size());
    EXPECT_EQ("NSObjectProtocol", meta->protocols[0].name);
    EXPECT_EQ("ObjectiveC.NSObject", meta->protocols[0].module);
}

TEST (YamlTests, TestCreateMeta_Protocol) {
    YAML::Node node = YAML::Load("Name: 'NSObject'\n"
            "JsName: 'NSObjectProtocol'\n"
            "Module: 'ObjectiveC.NSObject'\n"
            "IntroducedIn: 2.0\n"
            "Flags:\n"
            "- IsIosAppExtensionAvailable\n"
            "InstanceMethods:\n"
            "- Name: 'copy'\n"
            "  JsName: 'copy'\n"
            "  Flags:\n"
            "  - IsIosAppExtensionAvailable\n"
            "  - MethodOwnsReturnedCocoaObject\n"
            "  Selector: 'copy'\n"
            "  Signature:\n"
            "  - Type: Id\n"
            "    WithProtocols: []\n"
            "  TypeEncoding: '@8@0:4'\n"
            "StaticMethods:\n"
            "- Name: 'alloc'\n"
            "  JsName: 'alloc'\n"
            "  Flags:\n"
            "  - IsIosAppExtensionAvailable\n"
            "  - MethodOwnsReturnedCocoaObject\n"
            "  Selector: 'alloc'\n"
            "  Signature:\n"
            "  - Type: Instancetype\n"
            "  TypeEncoding: '@8@0:4'\n"
            "Properties:\n"
            "- Name: 'accounts'\n"
            "  JsName: 'accounts'\n"
            "  Flags:\n"
            "  - IsIosAppExtensionAvailable\n"
            "  - PropertyHasGetter\n"
            "  Getter:\n"
            "    Name: 'boolValue'\n"
            "    JsName: 'boolValue'\n"
            "    Flags:\n"
            "    - IsIosAppExtensionAvailable\n"
            "    Selector: 'boolValue'\n"
            "    Signature:\n"
            "    - Type: Bool\n"
            "    TypeEncoding: 'c8@0:4'\n"
            "Protocols:\n"
            "- Module: ObjectiveC.NSObject\n"
            "  Name: NSObjectProtocol\n"
            "Type: Protocol");

    unique_ptr<meta::Meta> mm = createMeta(node);
    meta::ProtocolMeta* meta = dynamic_cast<meta::ProtocolMeta*>(mm.get());
    EXPECT_TRUE(meta);
    EXPECT_EQ("NSObject", meta->name);
    EXPECT_EQ("NSObjectProtocol", meta->jsName);
    EXPECT_EQ("ObjectiveC.NSObject", meta->module);
    EXPECT_EQ(2, meta->introducedIn.Major);
    EXPECT_EQ(0, meta->introducedIn.Minor);
    EXPECT_EQ(-1, meta->introducedIn.SubMinor);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable, meta->flags);
    EXPECT_EQ(meta::SymbolType::Protocol, meta->type);

    EXPECT_EQ(1, meta->instanceMethods.size());
    EXPECT_EQ("copy", meta->instanceMethods[0].name);
    EXPECT_EQ("copy", meta->instanceMethods[0].jsName);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable | meta::MetaFlags::MethodOwnsReturnedCocoaObject , meta->instanceMethods[0].flags);
    EXPECT_EQ("copy", meta->instanceMethods[0].selector);
    EXPECT_EQ(1, meta->instanceMethods[0].signature.size());
    EXPECT_TRUE(dynamic_cast<typeEncoding::IdEncoding*>(meta->instanceMethods[0].signature[0].get()));
    EXPECT_EQ("@8@0:4", meta->instanceMethods[0].typeEncoding);

    EXPECT_EQ(1, meta->staticMethods.size());
    EXPECT_EQ("alloc", meta->staticMethods[0].name);
    EXPECT_EQ("alloc", meta->staticMethods[0].jsName);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable | meta::MetaFlags::MethodOwnsReturnedCocoaObject , meta->staticMethods[0].flags);
    EXPECT_EQ("alloc", meta->staticMethods[0].selector);
    EXPECT_EQ(1, meta->staticMethods[0].signature.size());
    EXPECT_TRUE(dynamic_cast<typeEncoding::InstancetypeEncoding*>(meta->staticMethods[0].signature[0].get()));
    EXPECT_EQ("@8@0:4", meta->staticMethods[0].typeEncoding);

    EXPECT_EQ(1, meta->properties.size());
    EXPECT_EQ("accounts", meta->properties[0].name);
    EXPECT_EQ("accounts", meta->properties[0].jsName);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable | meta::MetaFlags::PropertyHasGetter , meta->properties[0].flags);
    EXPECT_TRUE(meta->properties[0].getter.get());
    EXPECT_EQ("boolValue", meta->properties[0].getter->name);
    EXPECT_EQ("boolValue", meta->properties[0].getter->jsName);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable, meta->properties[0].getter->flags);
    EXPECT_EQ("boolValue", meta->properties[0].getter->selector);
    EXPECT_EQ(1, meta->properties[0].getter->signature.size());
    EXPECT_TRUE(dynamic_cast<typeEncoding::BoolEncoding*>(meta->properties[0].getter->signature[0].get()));
    EXPECT_EQ("c8@0:4", meta->properties[0].getter->typeEncoding);

    EXPECT_EQ(1, meta->protocols.size());
    EXPECT_EQ("NSObjectProtocol", meta->protocols[0].name);
    EXPECT_EQ("ObjectiveC.NSObject", meta->protocols[0].module);
}

TEST (YamlTests, TestCreateMeta_Category) {
    YAML::Node node = YAML::Load("Name: 'NSObject'\n"
            "JsName: 'NSObjectProtocol'\n"
            "Module: 'ObjectiveC.NSObject'\n"
            "IntroducedIn: 2.0\n"
            "Flags:\n"
            "- IsIosAppExtensionAvailable\n"
            "InstanceMethods:\n"
            "- Name: 'copy'\n"
            "  JsName: 'copy'\n"
            "  Flags:\n"
            "  - IsIosAppExtensionAvailable\n"
            "  - MethodOwnsReturnedCocoaObject\n"
            "  Selector: 'copy'\n"
            "  Signature:\n"
            "  - Type: Id\n"
            "    WithProtocols: []\n"
            "  TypeEncoding: '@8@0:4'\n"
            "StaticMethods:\n"
            "- Name: 'alloc'\n"
            "  JsName: 'alloc'\n"
            "  Flags:\n"
            "  - IsIosAppExtensionAvailable\n"
            "  - MethodOwnsReturnedCocoaObject\n"
            "  Selector: 'alloc'\n"
            "  Signature:\n"
            "  - Type: Instancetype\n"
            "  TypeEncoding: '@8@0:4'\n"
            "Properties:\n"
            "- Name: 'accounts'\n"
            "  JsName: 'accounts'\n"
            "  Flags:\n"
            "  - IsIosAppExtensionAvailable\n"
            "  - PropertyHasGetter\n"
            "  Getter:\n"
            "    Name: 'boolValue'\n"
            "    JsName: 'boolValue'\n"
            "    Flags:\n"
            "    - IsIosAppExtensionAvailable\n"
            "    Selector: 'boolValue'\n"
            "    Signature:\n"
            "    - Type: Bool\n"
            "    TypeEncoding: 'c8@0:4'\n"
            "Protocols:\n"
            "- Module: ObjectiveC.NSObject\n"
            "  Name: NSObjectProtocol\n"
            "ExtendedInterface:\n"
            "  Module: Foundation.NSCoder\n"
            "  Name: NSCoder\n"
            "Type: Category");

    unique_ptr<meta::Meta> mm = createMeta(node);
    meta::CategoryMeta* meta = dynamic_cast<meta::CategoryMeta*>(mm.get());
    EXPECT_TRUE(meta);
    EXPECT_EQ("NSObject", meta->name);
    EXPECT_EQ("NSObjectProtocol", meta->jsName);
    EXPECT_EQ("ObjectiveC.NSObject", meta->module);
    EXPECT_EQ(2, meta->introducedIn.Major);
    EXPECT_EQ(0, meta->introducedIn.Minor);
    EXPECT_EQ(-1, meta->introducedIn.SubMinor);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable, meta->flags);
    EXPECT_EQ(meta::SymbolType::Category, meta->type);
    EXPECT_EQ("NSCoder", meta->extendedInterface.name);
    EXPECT_EQ("Foundation.NSCoder", meta->extendedInterface.module);

    EXPECT_EQ(1, meta->instanceMethods.size());
    EXPECT_EQ("copy", meta->instanceMethods[0].name);
    EXPECT_EQ("copy", meta->instanceMethods[0].jsName);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable | meta::MetaFlags::MethodOwnsReturnedCocoaObject , meta->instanceMethods[0].flags);
    EXPECT_EQ("copy", meta->instanceMethods[0].selector);
    EXPECT_EQ(1, meta->instanceMethods[0].signature.size());
    EXPECT_TRUE(dynamic_cast<typeEncoding::IdEncoding*>(meta->instanceMethods[0].signature[0].get()));
    EXPECT_EQ("@8@0:4", meta->instanceMethods[0].typeEncoding);

    EXPECT_EQ(1, meta->staticMethods.size());
    EXPECT_EQ("alloc", meta->staticMethods[0].name);
    EXPECT_EQ("alloc", meta->staticMethods[0].jsName);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable | meta::MetaFlags::MethodOwnsReturnedCocoaObject , meta->staticMethods[0].flags);
    EXPECT_EQ("alloc", meta->staticMethods[0].selector);
    EXPECT_EQ(1, meta->staticMethods[0].signature.size());
    EXPECT_TRUE(dynamic_cast<typeEncoding::InstancetypeEncoding*>(meta->staticMethods[0].signature[0].get()));
    EXPECT_EQ("@8@0:4", meta->staticMethods[0].typeEncoding);

    EXPECT_EQ(1, meta->properties.size());
    EXPECT_EQ("accounts", meta->properties[0].name);
    EXPECT_EQ("accounts", meta->properties[0].jsName);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable | meta::MetaFlags::PropertyHasGetter , meta->properties[0].flags);
    EXPECT_TRUE(meta->properties[0].getter.get());
    EXPECT_EQ("boolValue", meta->properties[0].getter->name);
    EXPECT_EQ("boolValue", meta->properties[0].getter->jsName);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable, meta->properties[0].getter->flags);
    EXPECT_EQ("boolValue", meta->properties[0].getter->selector);
    EXPECT_EQ(1, meta->properties[0].getter->signature.size());
    EXPECT_TRUE(dynamic_cast<typeEncoding::BoolEncoding*>(meta->properties[0].getter->signature[0].get()));
    EXPECT_EQ("c8@0:4", meta->properties[0].getter->typeEncoding);

    EXPECT_EQ(1, meta->protocols.size());
    EXPECT_EQ("NSObjectProtocol", meta->protocols[0].name);
    EXPECT_EQ("ObjectiveC.NSObject", meta->protocols[0].module);
}

TEST (YamlTests, TestCreateMeta_Struct) {
    YAML::Node node = YAML::Load("Name: 'DSPSplitComplex'\n"
            "JsName: 'DSPSplitComplex'\n"
            "Module: 'Accelerate.vecLib.vDSP'\n"
            "Flags:\n"
            "- IsIosAppExtensionAvailable\n"
            "Fields:\n"
            "- Name: realp\n"
            "  Signature:\n"
            "    Type: Pointer\n"
            "    PointerType:\n"
            "      Type: Float\n"
            "- Name: imagp\n"
            "  Signature:\n"
            "    Type: Pointer\n"
            "    PointerType:\n"
            "      Type: Float\n"
            "Type: Struct");

    unique_ptr<meta::Meta> mm = createMeta(node);
    meta::StructMeta* meta = dynamic_cast<meta::StructMeta*>(mm.get());
    EXPECT_TRUE(meta);
    EXPECT_EQ("DSPSplitComplex", meta->name);
    EXPECT_EQ("DSPSplitComplex", meta->jsName);
    EXPECT_EQ("Accelerate.vecLib.vDSP", meta->module);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable, meta->flags);
    EXPECT_EQ(meta::SymbolType::Struct, meta->type);
    EXPECT_EQ(2, meta->fields.size());
    EXPECT_EQ("realp", meta->fields[0].name);
    EXPECT_TRUE(dynamic_cast<typeEncoding::PointerEncoding*>(meta->fields[0].encoding.get()));
    EXPECT_EQ("imagp", meta->fields[1].name);
    EXPECT_TRUE(dynamic_cast<typeEncoding::PointerEncoding*>(meta->fields[1].encoding.get()));
}

TEST (YamlTests, TestCreateMeta_Union) {
    YAML::Node node = YAML::Load("Name: 'DSPDoubleSplitComplex'\n"
            "JsName: 'DSPDoubleSplitComplex'\n"
            "Module: 'Accelerate.vecLib.vDSP'\n"
            "Flags:\n"
            "- IsIosAppExtensionAvailable\n"
            "Fields:\n"
            "- Name: realp\n"
            "  Signature:\n"
            "    Type: Pointer\n"
            "    PointerType:\n"
            "      Type: Double\n"
            "- Name: imagp\n"
            "  Signature:\n"
            "    Type: Pointer\n"
            "    PointerType:\n"
            "      Type: Double\n"
            "Type: Union");

    unique_ptr<meta::Meta> mm = createMeta(node);
    meta::UnionMeta* meta = dynamic_cast<meta::UnionMeta*>(mm.get());
    EXPECT_TRUE(meta);
    EXPECT_EQ("DSPDoubleSplitComplex", meta->name);
    EXPECT_EQ("DSPDoubleSplitComplex", meta->jsName);
    EXPECT_EQ("Accelerate.vecLib.vDSP", meta->module);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable, meta->flags);
    EXPECT_EQ(meta::SymbolType::Union, meta->type);
    EXPECT_EQ(2, meta->fields.size());
    EXPECT_EQ("realp", meta->fields[0].name);
    EXPECT_TRUE(dynamic_cast<typeEncoding::PointerEncoding*>(meta->fields[0].encoding.get()));
    EXPECT_EQ("imagp", meta->fields[1].name);
    EXPECT_TRUE(dynamic_cast<typeEncoding::PointerEncoding*>(meta->fields[1].encoding.get()));
}

TEST (YamlTests, TestCreateMeta_JsCode) {
    YAML::Node node = YAML::Load("Name: '8'\n"
            "JsName: 'vDSP_DFT_Direction'\n"
            "Module: 'Accelerate.vecLib.vDSP'\n"
            "Flags:\n"
            "- IsIosAppExtensionAvailable\n"
            "Type: JsCode\n"
            "JsCode: __tsEnum({\"vDSP_DFT_FORWARD\":1,\"vDSP_DFT_INVERSE\":-1})");

    unique_ptr<meta::Meta> mm = createMeta(node);
    meta::JsCodeMeta* meta = dynamic_cast<meta::JsCodeMeta*>(mm.get());
    EXPECT_TRUE(meta);
    EXPECT_EQ("8", meta->name);
    EXPECT_EQ("vDSP_DFT_Direction", meta->jsName);
    EXPECT_EQ("Accelerate.vecLib.vDSP", meta->module);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable, meta->flags);
    EXPECT_EQ(meta::SymbolType::JsCode, meta->type);
    EXPECT_EQ("__tsEnum({\"vDSP_DFT_FORWARD\":1,\"vDSP_DFT_INVERSE\":-1})", meta->jsCode);
}

TEST (YamlTests, TestCreateMeta_Var) {
    YAML::Node node = YAML::Load("Name: 'kABPersonInstantMessageUsernameKey'\n"
            "JsName: 'kABPersonInstantMessageUsernameKey'\n"
            "Module: 'AddressBook.ABPerson'\n"
            "Flags:\n"
            "- IsIosAppExtensionAvailable\n"
            "Type: Var\n"
            "Signature:\n"
            "  Type: Interface\n"
            "  Module: CoreFoundation.CFBase\n"
            "  Name: NSString");

    unique_ptr<meta::Meta> mm = createMeta(node);
    meta::VarMeta* meta = dynamic_cast<meta::VarMeta*>(mm.get());
    EXPECT_TRUE(meta);
    EXPECT_EQ("kABPersonInstantMessageUsernameKey", meta->name);
    EXPECT_EQ("kABPersonInstantMessageUsernameKey", meta->jsName);
    EXPECT_EQ("AddressBook.ABPerson", meta->module);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable, meta->flags);
    EXPECT_EQ(meta::SymbolType::Var, meta->type);
    EXPECT_TRUE(dynamic_cast<typeEncoding::InterfaceEncoding*>(meta->signature.get()));
}

TEST (YamlTests, TestCreateMeta_Function) {
    YAML::Node node = YAML::Load("Name: 'NSClassFromString'\n"
            "JsName: 'NSClassFromString'\n"
            "Module: 'Foundation.NSObjCRuntime'\n"
            "IntroducedIn: 5.0\n"
            "Flags:\n"
            "- IsIosAppExtensionAvailable\n"
            "Type: Function\n"
            "Signature:\n"
            "- Type: Class\n"
            "- Type: Interface\n"
            "  Module: Foundation.NSString\n"
            "  Name: NSString");

    unique_ptr<meta::Meta> mm = createMeta(node);
    meta::FunctionMeta* meta = dynamic_cast<meta::FunctionMeta*>(mm.get());
    EXPECT_TRUE(meta);
    EXPECT_EQ("NSClassFromString", meta->name);
    EXPECT_EQ("NSClassFromString", meta->jsName);
    EXPECT_EQ("Foundation.NSObjCRuntime", meta->module);
    EXPECT_EQ(5, meta->introducedIn.Major);
    EXPECT_EQ(0, meta->introducedIn.Minor);
    EXPECT_EQ(-1, meta->introducedIn.SubMinor);
    EXPECT_EQ(meta::MetaFlags::IsIosAppExtensionAvailable, meta->flags);
    EXPECT_EQ(meta::SymbolType::Function, meta->type);
    EXPECT_EQ(2, meta->signature.size());
    EXPECT_TRUE(dynamic_cast<typeEncoding::ClassEncoding*>(meta->signature[0].get()));
    EXPECT_TRUE(dynamic_cast<typeEncoding::InterfaceEncoding*>(meta->signature[1].get()));
}
