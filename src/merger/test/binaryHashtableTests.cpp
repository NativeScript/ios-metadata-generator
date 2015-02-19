#include "test.h"
#include "binary/binarySerializerPrivate.h"
#include "binary/binaryHashtable.h"
#include "utils/memoryStream.h"
#include "binary/binaryWriter.h"

TEST (BinaryHashTableTests, TestSize) {
    binary::BinaryHashtable target = binary::BinaryHashtable(10);
    EXPECT_EQ(13, target.size());
}

TEST (BinaryHashTableTests, TestFill) {
    binary::BinaryHashtable target = binary::BinaryHashtable(10);
    for (int i = 0; i < 10; i++) {
        std::stringstream ss;
        ss << i;
        target.add(ss.str(), i * 4);
    }
    EXPECT_EQ(13, target.size());
}

TEST (BinaryHashTableTests, TestSerialization_SizeShouldMatch) {
    binary::BinaryHashtable target = binary::BinaryHashtable(10);
    for (int i = 0; i < 10; i++) {
        std::stringstream ss;
        ss << i;
        target.add(ss.str(), i * 4);
    }

    std::shared_ptr<utils::MemoryStream> stream = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer = binary::BinaryWriter(stream, 4, 4);

    std::vector<binary::MetaFileOffset> globalTable = target.serialize(writer);
    EXPECT_EQ(target.size(), globalTable.size());
}

TEST (BinaryHashTableTests, TestRetrieval) {
    binary::BinaryHashtable target = binary::BinaryHashtable(10);
    for (int i = 0; i < 10; i++) {
        std::stringstream ss;
        ss << i;
        target.add(ss.str(), i * 4);
    }

    for (int i = 0; i < 10; i++) {
        std::stringstream ss;
        ss << i;
        EXPECT_EQ(i * 4, target.get(ss.str()));
    }
}

TEST (BinaryHashTableTests, TestSerialization_ArraysInHeap_2_1) {
    int pointer_size = 2;
    int array_count_size = 1;

    binary::BinaryHashtable target = binary::BinaryHashtable(10);
    for (int i = 0; i < 10; i++) {
        std::stringstream ss;
        ss << i;
        target.add(ss.str(), i * pointer_size);
    }

    std::shared_ptr<utils::MemoryStream> stream = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer = binary::BinaryWriter(stream, pointer_size, array_count_size);

    target.serialize(writer);

    /*
    Used slots:
    Index -> Count
    0 -> 1
    1 -> 2
    2 -> 1
    5 -> 3
    8 -> 2
    11 -> 1
    Total: 26 bytes (With 2 bytes for pointer size and 1 byte for array count size
     */

    EXPECT_EQ(26, stream->size());
}

TEST (BinaryHashTableTests, TestSerialization_ArraysInHeap_8_4) {
    int pointer_size = 8;
    int array_count_size = 4;

    binary::BinaryHashtable target = binary::BinaryHashtable(10);
    for (int i = 0; i < 10; i++) {
        std::stringstream ss;
        ss << i;
        target.add(ss.str(), i * pointer_size);
    }

    std::shared_ptr<utils::MemoryStream> stream = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer = binary::BinaryWriter(stream, pointer_size, array_count_size);

    target.serialize(writer);

    /*
    Used slots:
    Index -> Count
    0 -> 1
    1 -> 2
    2 -> 1
    5 -> 3
    8 -> 2
    11 -> 1
    Total: 104 bytes (With 8 bytes for pointer size and 4 bytes for array count size
     */

    EXPECT_EQ(104, stream->size());
}

TEST (BinaryHashTableTests, TestSerialization_Offsets) {
    int pointer_size = 4;
    int array_count_size = 4;

    binary::BinaryHashtable target = binary::BinaryHashtable(10);
    for (int i = 0; i < 10; i++) {
        std::stringstream ss;
        ss << i;
        target.add(ss.str(), i * pointer_size);
    }

    std::shared_ptr<utils::MemoryStream> stream = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
    binary::BinaryWriter writer = binary::BinaryWriter(stream, pointer_size, array_count_size);

    std::vector<binary::MetaFileOffset> offsets = target.serialize(writer);

    /*
    Used slots:
    Index -> Count
    0 -> 1
    1 -> 2
    2 -> 1
    5 -> 3
    8 -> 2
    11 -> 1
     */

    int heapOffset = 0;
    EXPECT_EQ(heapOffset, offsets[0]);
    EXPECT_EQ(heapOffset += (array_count_size + pointer_size), offsets[1]);
    EXPECT_EQ(heapOffset += (array_count_size + pointer_size * 2), offsets[2]);
    EXPECT_EQ(0, offsets[3]);
    EXPECT_EQ(0, offsets[4]);
    EXPECT_EQ(heapOffset += (array_count_size + pointer_size), offsets[5]);
    EXPECT_EQ(0, offsets[6]);
    EXPECT_EQ(0, offsets[7]);
    EXPECT_EQ(heapOffset += (array_count_size + pointer_size * 3), offsets[8]);
    EXPECT_EQ(0, offsets[9]);
    EXPECT_EQ(0, offsets[10]);
    EXPECT_EQ(heapOffset += (array_count_size + pointer_size * 2), offsets[11]);
    EXPECT_EQ(0, offsets[12]);
}
