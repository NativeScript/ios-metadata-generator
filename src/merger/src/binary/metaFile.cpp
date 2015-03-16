#include "metaFile.h"
#include "../utils/fileStream.h"

unsigned int binary::MetaFile::size() {
    return (unsigned int)this->_heap->size();
}

void binary::MetaFile::registerInGlobalTable(const std::string& jsName, binary::MetaFileOffset offset) {
    this->_globalTableSymbols->add(jsName, offset);
}

binary::MetaFileOffset binary::MetaFile::getFromGlobalTable(const std::string& jsName) {
    return this->_globalTableSymbols->get(jsName);
}

void binary::MetaFile::registerTopLevelModules(std::vector<std::string> &topLevelModules) {
    for (auto& topLevelModule : topLevelModules) {
        this->_topLevelModulesOffset.push_back(this->_heapWriter->push_string(topLevelModule));
    }
}

void binary::MetaFile::save(string filename) {
    std::shared_ptr<utils::FileStream> fileStream = utils::FileStream::open(filename, ios::out | ios::binary | ios::trunc);
    this->save(fileStream);
    fileStream->close();
}

void binary::MetaFile::save(std::shared_ptr<utils::Stream> stream) {
    const int HEAD_SECTION_SIZE = 2 + (this->pointer_size * 3);
    stream->set_position(HEAD_SECTION_SIZE);

    BinaryWriter writer = BinaryWriter(stream, this->pointer_size, this->array_count_size);

    // dump modules table
    this->modules_offset = writer.push_binaryArray(this->_topLevelModulesOffset);

    // dump global table
    std::vector<binary::MetaFileOffset> offsets = this->_globalTableSymbols->serialize(this->_heapWriter.get());
    this->globalTable_offset = writer.push_binaryArray(offsets);

    // dump heap
    this->heap_offset = stream->position();
    for (auto byteIter = this->_heap->begin(); byteIter != this->_heap->end(); ++byteIter) {
        stream->push_byte(*byteIter);
    }

    // Head section
    stream->set_position(0);
    writer.push_byte(this->pointer_size);
    writer.push_byte(this->array_count_size);
    writer.push_pointer(this->modules_offset);
    writer.push_pointer(this->globalTable_offset);
    writer.push_pointer(this->heap_offset);
}
