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

binary::BinaryWriter binary::MetaFile::heap_writer() {
    return binary::BinaryWriter(this->_heap, this->pointer_size, this->array_count_size);
}

binary::BinaryReader binary::MetaFile::heap_reader() {
    return binary::BinaryReader(this->_heap, this->pointer_size, this->array_count_size);
}

void binary::MetaFile::save(string filename) {
    std::shared_ptr<utils::FileStream> fileStream = utils::FileStream::open(filename, ios::out | ios::binary | ios::trunc);
    this->save(fileStream);
    fileStream->close();
}

void binary::MetaFile::save(std::shared_ptr<utils::Stream> stream) {
    // dump global table
    BinaryWriter globalTableStreamWriter = BinaryWriter(stream, this->pointer_size, this->array_count_size);
    BinaryWriter heapWriter = this->heap_writer();
    std::vector<binary::MetaFileOffset> offsets = this->_globalTableSymbols->serialize(heapWriter);
    globalTableStreamWriter.push_binaryArray(offsets);

    // dump heap
    for (auto byteIter = this->_heap->begin(); byteIter != this->_heap->end(); ++byteIter) {
        stream->push_byte(*byteIter);
    }
}
