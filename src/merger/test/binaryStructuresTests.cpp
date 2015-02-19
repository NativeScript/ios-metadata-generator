#include "test.h"
#include "binary/binaryStructures.h"
#include "binary/binaryWriter.h"
#include "binary/binaryReader.h"
#include "utils/memoryStream.h"

TEST(BinaryStructuresTests, TestSave1TypeEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);

    binary::TypeEncoding target(binary::BinaryTypeEncodingType::Int);
    target.save(writer);

    EXPECT_EQ(1, ms->size());
}

TEST(BinaryStructuresTests, TestSave2TypeEncodings) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);

    binary::TypeEncoding target(binary::BinaryTypeEncodingType::Int);
    target.save(writer);

    binary::TypeEncoding target2(binary::BinaryTypeEncodingType::LongLong);
    target2.save(writer);

    EXPECT_EQ(2, ms->size());
}

TEST(BinaryStructuresTests, TestSave_TypeEncoding_IncompleteArray) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);

    binary::IncompleteArrayEncoding target;
    binary::PointerEncoding* p = new binary::PointerEncoding();
    p->_target = std::unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Void));
    target._elementType = std::unique_ptr<binary::TypeEncoding>(p);
    binary::MetaFileOffset offset = target.save(writer);

    EXPECT_EQ(0, offset);
    EXPECT_EQ(3, ms->size());
    ms->set_position(0);
    EXPECT_EQ(binary::BinaryTypeEncodingType::IncompleteArray, ms->read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Pointer, ms->read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Void, ms->read_byte());
    EXPECT_EQ(ms->position(), ms->size());
}

TEST(BinaryStructuresTests, TestSave_TypeEncoding_ConstantArrayEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer(ms, 4, 4);
    binary::BinaryReader reader(ms, 4, 4);

    binary::ConstantArrayEncoding target;
    binary::PointerEncoding* p = new binary::PointerEncoding();
    p->_target = std::unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Void));
    target._elementType = std::unique_ptr<binary::TypeEncoding>(p);
    target._size = 20;
    binary::MetaFileOffset offset = target.save(writer);

    EXPECT_EQ(0, offset);
    EXPECT_EQ(7, ms->size());
    ms->set_position(0);
    EXPECT_EQ(binary::BinaryTypeEncodingType::ConstantArray, ms->read_byte());
    EXPECT_EQ(20, reader.read_int());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Pointer, ms->read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Void, ms->read_byte());
    EXPECT_EQ(ms->position(), ms->size());
}

TEST(BinaryStructuresTests, TestSave_TypeEncoding_DeclarationReferenceEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    int pointerSize = 4;
    binary::BinaryWriter writer(ms, pointerSize, 4);
    binary::BinaryReader reader(ms, pointerSize, 4);

    binary::DeclarationReferenceEncoding target(binary::BinaryTypeEncodingType::StructDeclarationReference);
    std::string testStr = "Hello";
    target._name = writer.push_string(testStr);
    binary::MetaFileOffset offset = target.save(writer);

    EXPECT_EQ(testStr.size() + 1, offset);
    EXPECT_EQ(offset + pointerSize + 1, ms->size());
    ms->set_position(0);
    EXPECT_EQ(testStr, reader.read_string());
    EXPECT_EQ(offset, ms->position());
    EXPECT_EQ(binary::BinaryTypeEncodingType::StructDeclarationReference, reader.read_byte());
    EXPECT_EQ(0, reader.read_pointer());
    EXPECT_EQ(ms->position(), ms->size());
}

TEST(BinaryStructuresTests, TestSave_TypeEncoding_InterfaceDeclarationEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    int pointerSize = 4;
    binary::BinaryWriter writer(ms, pointerSize, 4);
    binary::BinaryReader reader(ms, pointerSize, 4);

    binary::InterfaceDeclarationEncoding target;
    std::string testStr = "NSArray";
    target._name = writer.push_string(testStr);
    binary::MetaFileOffset offset = target.save(writer);

    EXPECT_EQ(testStr.size() + 1, offset);
    EXPECT_EQ(offset + pointerSize + 1, ms->size());
    ms->set_position(0);
    EXPECT_EQ(testStr, reader.read_string());
    EXPECT_EQ(offset, ms->position());
    EXPECT_EQ(binary::BinaryTypeEncodingType::InterfaceDeclaration, reader.read_byte());
    EXPECT_EQ(0, reader.read_pointer());
    EXPECT_EQ(ms->position(), ms->size());
}

TEST(BinaryStructuresTests, TestSave_TypeEncoding_PointerEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    int pointerSize = 4;
    binary::BinaryWriter writer(ms, pointerSize, 4);

    binary::PointerEncoding target;
    target._target = std::unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Void));
    binary::MetaFileOffset offset = target.save(writer);

    EXPECT_EQ(0, offset);
    EXPECT_EQ(2, ms->size());
    ms->set_position(offset);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Pointer, ms->read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Void, ms->read_byte());
    EXPECT_EQ(ms->position(), ms->size());
}

TEST(BinaryStructuresTests, TestSave_TypeEncoding_BlockEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    int pointerSize = 4;
    binary::BinaryWriter writer(ms, pointerSize, 4);

    binary::BlockEncoding target;
    target._encodingsCount = 3;
    target._encodings.push_back(std::unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Void)));
    target._encodings.push_back(std::unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Int)));
    target._encodings.push_back(std::unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Double)));

    binary::MetaFileOffset offset = target.save(writer);

    EXPECT_EQ(0, offset);
    EXPECT_EQ(5, ms->size());
    ms->set_position(offset);
    EXPECT_EQ(binary::BinaryTypeEncodingType::Block, ms->read_byte());
    EXPECT_EQ(3, ms->read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Void, ms->read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, ms->read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Double, ms->read_byte());
    EXPECT_EQ(ms->position(), ms->size());
}

TEST(BinaryStructuresTests, TestSave_TypeEncoding_AnonymousRecordEncoding) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    int pointerSize = 4;
    binary::BinaryWriter writer(ms, pointerSize, 4);
    binary::BinaryReader reader(ms, pointerSize, 4);

    binary::AnonymousRecordEncoding target(binary::BinaryTypeEncodingType::AnonymousStruct);
    target._fieldsCount = 3;
    target._fieldNames.push_back(writer.push_string("x1"));
    target._fieldNames.push_back(writer.push_string("x2"));
    target._fieldNames.push_back(writer.push_string("x3"));
    target._fieldEncodings.push_back(std::unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::LongLong)));
    target._fieldEncodings.push_back(std::unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Int)));
    target._fieldEncodings.push_back(std::unique_ptr<binary::TypeEncoding>(new binary::TypeEncoding(binary::BinaryTypeEncodingType::Double)));

    binary::MetaFileOffset offset = target.save(writer);

    EXPECT_EQ(9, offset);
    EXPECT_EQ(26, ms->size());
    ms->set_position(offset);
    EXPECT_EQ(binary::BinaryTypeEncodingType::AnonymousStruct, reader.read_byte());
    EXPECT_EQ(3, reader.read_byte()); // _fieldsCount
    EXPECT_EQ(0, reader.read_pointer()); // _fieldNames[0]
    EXPECT_EQ(3, reader.read_pointer()); // _fieldNames[1]
    EXPECT_EQ(6, reader.read_pointer()); // _fieldNames[2]
    EXPECT_EQ(binary::BinaryTypeEncodingType::LongLong, reader.read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, reader.read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Double, reader.read_byte());
    EXPECT_EQ(ms->position(), ms->size());
}
