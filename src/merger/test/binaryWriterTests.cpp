#include "test.h"
#include "utils/memoryStream.h"
#include "binary/binaryWriter.h"

TEST (BinaryWriterTests, TestPushString) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, 4, 4);

    std::string testStr = "test string";
    target.push_string(testStr);
    EXPECT_EQ(testStr.size() + 1, ms->position());
}

TEST (BinaryWriterTests, TestPush2DifferentStrings_NoIntern) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, 4, 4);

    std::string testStr = "test string";
    std::string testStr2 = "test string2";
    target.push_string(testStr, false);
    target.push_string(testStr2, false);
    EXPECT_EQ(testStr.size() + 1 + testStr2.size() + 1, ms->position());
}

TEST (BinaryWriterTests, TestPush2DifferentStrings_Intern) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, 4, 4);

    std::string testStr = "test string";
    std::string testStr2 = "test string2";
    target.push_string(testStr);
    target.push_string(testStr2);
    EXPECT_EQ(testStr.size() + 1 + testStr2.size() + 1, ms->position());
}

TEST (BinaryWriterTests, TestPush2SameStrings_NoIntern) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, 4, 4);

    std::string testStr = "test string";
    target.push_string(testStr, false);
    target.push_string(testStr, false);
    EXPECT_EQ(2 * (testStr.size() + 1), ms->position());
}

TEST (BinaryWriterTests, TestPush2SameStrings_Intern) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, 4, 4);

    std::string testStr = "test string";
    target.push_string(testStr);
    target.push_string(testStr);
    EXPECT_EQ(testStr.size() + 1, ms->position());
}

TEST (BinaryWriterTests, TestPushByte) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, 4, 4);

    target.push_byte(0x17);
    EXPECT_EQ(1, ms->position());
}

TEST (BinaryWriterTests, TestPushShort) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, 4, 4);

    target.push_short(2015);
    EXPECT_EQ(2, ms->position());
}

TEST (BinaryWriterTests, TestPushInt) {
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, 4, 4);

    target.push_int(20152015);
    EXPECT_EQ(4, ms->position());
}

TEST (BinaryWriterTests, TestPushPointer_4) {
    const int pointerSize = 4;
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, pointerSize, 4);

    target.push_pointer(0x1012);
    EXPECT_EQ(pointerSize, ms->position());
}

TEST (BinaryWriterTests, TestPushPointer_2) {
    const int pointerSize = 2;
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, pointerSize, 4);

    target.push_pointer(0x1012);
    EXPECT_EQ(pointerSize, ms->position());
}

TEST (BinaryWriterTests, TestPushEmptyBinaryArray) {
    const int pointerSize = 4;
    const int arrayCountSize = 4;
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, pointerSize, arrayCountSize);

    std::vector<binary::MetaFileOffset> vector; // empty vector
    target.push_binaryArray(vector);
    EXPECT_EQ(arrayCountSize, ms->position());
}

TEST (BinaryWriterTests, TestPushEmptyBinaryArray2) {
    const int pointerSize = 4;
    const int arrayCountSize = 4;
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, pointerSize, arrayCountSize);
    ms->push_byte(0x10);
    ms->push_byte(0x12);

    std::vector<binary::MetaFileOffset> vector; // empty vector
    uint8_t offset = target.push_binaryArray(vector);
    EXPECT_EQ(2, offset);
    EXPECT_EQ(2 + arrayCountSize, ms->position());
}

TEST (BinaryWriterTests, TestPushBinaryArray_4_4) {
    const int pointerSize = 4;
    const int arrayCountSize = 4;
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, pointerSize, arrayCountSize);

    std::vector<binary::MetaFileOffset> vector = { 0x1000, 0x1001, 0x1002 };
    target.push_binaryArray(vector);
    EXPECT_EQ(arrayCountSize + (vector.size() * pointerSize), ms->position());
}

TEST (BinaryWriterTests, TestPushBinaryArray_4_1) {
    const int pointerSize = 4;
    const int arrayCountSize = 1;
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, pointerSize, arrayCountSize);

    std::vector<binary::MetaFileOffset> vector = { 0x1000, 0x1001, 0x1002 };
    target.push_binaryArray(vector);
    EXPECT_EQ(arrayCountSize + (vector.size() * pointerSize), ms->position());
}

TEST (BinaryWriterTests, TestPushBinaryArray_2_1) {
    const int pointerSize = 2;
    const int arrayCountSize = 1;
    std::shared_ptr<utils::MemoryStream> ms = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter target(ms, pointerSize, arrayCountSize);

    std::vector<binary::MetaFileOffset> vector = { 0x1000, 0x1001, 0x1002 };
    target.push_binaryArray(vector);
    EXPECT_EQ(arrayCountSize + (vector.size() * pointerSize), ms->position());
}
