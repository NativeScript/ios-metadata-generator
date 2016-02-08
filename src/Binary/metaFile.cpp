#include "Meta/MetaEntities.h"
#include "metaFile.h"
#include "Utils/fileStream.h"

unsigned int binary::MetaFile::size()
{
    return this->_globalTableSymbols->size();
}

void binary::MetaFile::registerInGlobalTables(const ::Meta::Meta* meta, binary::MetaFileOffset offset)
{
    this->_globalTableSymbols->add(meta->jsName, offset);
    std::map<const clang::Module*, binary::ModuleMeta>::iterator modulePair = this->_topLevelModules.find(meta->module->getTopLevelModule());
    modulePair->second._moduleTable.add(meta->jsName, offset);
}

void binary::MetaFile::registerModule(const clang::Module* module, binary::ModuleMeta& moduleBinaryStructure)
{
    this->_topLevelModules.insert(std::pair<const clang::Module*, binary::ModuleMeta>(module, moduleBinaryStructure));
}

binary::BinaryWriter binary::MetaFile::heap_writer()
{
    return binary::BinaryWriter(this->_heap);
}

binary::BinaryReader binary::MetaFile::heap_reader()
{
    return binary::BinaryReader(this->_heap);
}

void binary::MetaFile::save(string filename)
{
    std::shared_ptr<utils::FileStream> fileStream = utils::FileStream::open(filename, ios::out | ios::binary | ios::trunc);
    this->save(fileStream);
    fileStream->close();
}

void binary::MetaFile::save(std::shared_ptr<utils::Stream> stream)
{
    // dump global table
    BinaryWriter globalTableStreamWriter = BinaryWriter(stream);
    BinaryWriter heapWriter = this->heap_writer();
    std::vector<binary::MetaFileOffset> offsets = this->_globalTableSymbols->serialize(heapWriter);
    globalTableStreamWriter.push_binaryArray(offsets);

    std::vector<MetaFileOffset> modulesOffsets;
    for (std::pair<const clang::Module*, binary::ModuleMeta> pair : this->_topLevelModules)
        modulesOffsets.push_back(pair.second.save(heapWriter));
    globalTableStreamWriter.push_binaryArray(modulesOffsets);

    // dump heap
    for (auto byteIter = this->_heap->begin(); byteIter != this->_heap->end(); ++byteIter) {
        stream->push_byte(*byteIter);
    }
}
