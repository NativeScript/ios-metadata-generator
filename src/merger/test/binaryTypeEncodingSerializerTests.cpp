#include "test.h"
#include "binary/binaryTypeEncodingSerializer.h"
#include "typeEncoding/typeEncoding.h"
#include "utils/memoryStream.h"
#include "binary/binaryWriter.h"

TEST(BinaryTypeEncodingSerializerTests, TestUnknown) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::UnknownEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::Unknown, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestVoidEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::VoidEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::Void, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestBoolEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::BoolEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::Bool, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestShortEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::ShortEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::Short, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestUShortEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::UShortEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::UShort, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestIntEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::IntEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestUIntEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::UIntEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::UInt, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestLongEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::LongEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::Long, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestULongEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::ULongEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::ULong, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestLongLongEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::LongLongEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::LongLong, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestULongLongEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::ULongLongEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::ULongLong, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestSignedCharEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::SignedCharEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::Char, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestUnsignedCharEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::UnsignedCharEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::UChar, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestUnicharEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::UnicharEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::Unichar, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestCStringEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::CStringEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::CString, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestFloatEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::FloatEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::Float, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestDoubleEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::DoubleEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::Double, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestVaListEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::VaListEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::VaList, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestSelectorEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::SelectorEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::Selector, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestInstancetypeEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::InstancetypeEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::InstanceType, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestClassEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::ClassEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::Class, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestProtocolEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::ProtocolEncoding target;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::Protocol, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestIdEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::IdEncoding target;
    target.protocols.push_back({ .name = "Test", .module = "TestModule" });

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);

    EXPECT_EQ(binary::BinaryTypeEncodingType::Id, result->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestConstantArrayEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::ConstantArrayEncoding target;
    target.elementType = std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::IntEncoding());
    target.size = 4;

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);
    binary::ConstantArrayEncoding* constantArrayEncoding = (binary::ConstantArrayEncoding*)result.get();

    EXPECT_EQ(binary::BinaryTypeEncodingType::ConstantArray, constantArrayEncoding->_type);
    EXPECT_EQ(4, constantArrayEncoding->_size);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, constantArrayEncoding->_elementType->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestIncompleteArrayEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::IncompleteArrayEncoding target;
    target.elementType = std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::IntEncoding());

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);
    binary::IncompleteArrayEncoding* incompleteArrayEncoding = (binary::IncompleteArrayEncoding*)result.get();

    EXPECT_EQ(binary::BinaryTypeEncodingType::IncompleteArray, incompleteArrayEncoding->_type);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, incompleteArrayEncoding->_elementType->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestInterfaceEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    ms->push_byte(0x10); // change the memory stream
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::InterfaceEncoding target;
    target.name = { .name = "Test", .module = "TestModule" };

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);
    binary::DeclarationReferenceEncoding* declarationReferenceEncoding = (binary::DeclarationReferenceEncoding*)result.get();

    EXPECT_EQ(binary::BinaryTypeEncodingType::InterfaceDeclarationReference, declarationReferenceEncoding->_type);
    EXPECT_EQ(1, declarationReferenceEncoding->_name);
}

TEST(BinaryTypeEncodingSerializerTests, TestPointerEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::PointerEncoding target;
    target.target = std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::VoidEncoding());

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);
    binary::PointerEncoding* pointerEncoding = (binary::PointerEncoding*)result.get();

    EXPECT_EQ(binary::BinaryTypeEncodingType::Pointer, pointerEncoding->_type);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Void, pointerEncoding->_target->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestBlockEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::BlockEncoding target;
    target.blockCall.push_back(std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::VoidEncoding()));
    target.blockCall.push_back(std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::IntEncoding()));
    target.blockCall.push_back(std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::FloatEncoding()));

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);
    binary::BlockEncoding* blockEncoding = (binary::BlockEncoding*)result.get();

    EXPECT_EQ(binary::BinaryTypeEncodingType::Block, blockEncoding->_type);
    EXPECT_EQ(target.blockCall.size(), blockEncoding->_encodingsCount);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Void, blockEncoding->_encodings[0]->_type);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, blockEncoding->_encodings[1]->_type);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Float, blockEncoding->_encodings[2]->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestFunctionEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::FunctionEncoding target;
    target.functionCall.push_back(std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::VoidEncoding()));
    target.functionCall.push_back(std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::IntEncoding()));
    target.functionCall.push_back(std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::FloatEncoding()));

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);
    binary::FunctionEncoding* functionEncoding = (binary::FunctionEncoding*)result.get();

    EXPECT_EQ(binary::BinaryTypeEncodingType::FunctionPointer, functionEncoding->_type);
    EXPECT_EQ(target.functionCall.size(), functionEncoding->_encodingsCount);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Void, functionEncoding->_encodings[0]->_type);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, functionEncoding->_encodings[1]->_type);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Float, functionEncoding->_encodings[2]->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestStructEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    ms->push_byte(0x10); // change the memory stream
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::StructEncoding target;
    target.name = { .name = "Test", .module = "TestModule" };

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);
    binary::DeclarationReferenceEncoding* declarationReferenceEncoding = (binary::DeclarationReferenceEncoding*)result.get();

    EXPECT_EQ(binary::BinaryTypeEncodingType::StructDeclarationReference, declarationReferenceEncoding->_type);
    EXPECT_EQ(1, declarationReferenceEncoding->_name);
}

TEST(BinaryTypeEncodingSerializerTests, TestUnionEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    ms->push_byte(0x10); // change the memory stream
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::UnionEncoding target;
    target.name = { .name = "Test", .module = "TestModule" };

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);
    binary::DeclarationReferenceEncoding* declarationReferenceEncoding = (binary::DeclarationReferenceEncoding*)result.get();

    EXPECT_EQ(binary::BinaryTypeEncodingType::UnionDeclarationReference, declarationReferenceEncoding->_type);
    EXPECT_EQ(1, declarationReferenceEncoding->_name);
}

TEST(BinaryTypeEncodingSerializerTests, TestInterfaceDeclarationEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    ms->push_byte(0x10); // change the memory stream
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::InterfaceDeclarationEncoding target;
    target.name = { .name = "Test", .module = "TestModule" };

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);
    binary::InterfaceDeclarationEncoding* declarationEncoding = (binary::InterfaceDeclarationEncoding*)result.get();

    EXPECT_EQ(binary::BinaryTypeEncodingType::InterfaceDeclaration, declarationEncoding->_type);
    EXPECT_EQ(1, declarationEncoding->_name);
}

TEST(BinaryTypeEncodingSerializerTests, TestAnonymousStructEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::AnonymousStructEncoding target;
    target.fieldNames.push_back("x1");
    target.fieldNames.push_back("x2");
    target.fieldEncodings.push_back(std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::IntEncoding()));
    target.fieldEncodings.push_back(std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::FloatEncoding()));

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);
    binary::AnonymousRecordEncoding* recordEncoding = (binary::AnonymousRecordEncoding*)result.get();

    EXPECT_EQ(binary::BinaryTypeEncodingType::AnonymousStruct, recordEncoding->_type);
    EXPECT_EQ(2, recordEncoding->_fieldsCount);
    EXPECT_EQ(0, recordEncoding->_fieldNames[0]);
    EXPECT_EQ(3, recordEncoding->_fieldNames[1]);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, recordEncoding->_fieldEncodings[0]->_type);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Float, recordEncoding->_fieldEncodings[1]->_type);
}

TEST(BinaryTypeEncodingSerializerTests, TestAnonymousUnionEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryTypeEncodingSerializer s(writer);
    typeEncoding::AnonymousUnionEncoding target;
    target.fieldNames.push_back("x1");
    target.fieldNames.push_back("x2");
    target.fieldEncodings.push_back(std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::IntEncoding()));
    target.fieldEncodings.push_back(std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::FloatEncoding()));

    std::unique_ptr<binary::TypeEncoding> result = target.serialize(&s);
    binary::AnonymousRecordEncoding* recordEncoding = (binary::AnonymousRecordEncoding*)result.get();

    EXPECT_EQ(binary::BinaryTypeEncodingType::AnonymousUnion, recordEncoding->_type);
    EXPECT_EQ(2, recordEncoding->_fieldsCount);
    EXPECT_EQ(0, recordEncoding->_fieldNames[0]);
    EXPECT_EQ(3, recordEncoding->_fieldNames[1]);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, recordEncoding->_fieldEncodings[0]->_type);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Float, recordEncoding->_fieldEncodings[1]->_type);
}
