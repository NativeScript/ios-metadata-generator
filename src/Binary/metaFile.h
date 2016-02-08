#pragma once

#include <vector>
#include <memory>
#include <clang/Basic/Module.h>
#include "binaryHashtable.h"
#include "binaryWriter.h"
#include "binaryReader.h"
#include "Utils/memoryStream.h"

using namespace std;

namespace Meta {
class Meta;
}

namespace binary {
/*
     * \class MetaFile
     * \brief Represents a binary meta file
     *
     * A binary meta file contains a global table and heap in which meta objects are contained in
     * binary format.
     */
class MetaFile {
private:
    std::unique_ptr<BinaryHashtable> _globalTableSymbols;
    std::map<const clang::Module*, binary::ModuleMeta> _topLevelModules;
    std::shared_ptr<utils::MemoryStream> _heap;

public:
    /*
         * \brief Constructs a \c MetaFile with the given size
         * \param size The number of meta objects this file will contain
         */
    MetaFile(int size)
    {
        this->_globalTableSymbols = std::unique_ptr<BinaryHashtable>(new BinaryHashtable(size));
        this->_heap = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
        this->_heap->push_byte(0); // mark heap
    }
    MetaFile()
        : MetaFile(10)
    {
    }

    /*
         * \brief Returns the number of meta objects in this file.
         */
    unsigned int size();

    /// global table
    /*
         * \brief Adds an entry to the global table
         * \param jsName The jsName of the element
         * \param offset The offset in the heap
         */
    void registerInGlobalTables(const ::Meta::Meta* meta, binary::MetaFileOffset offset);

    /*
         * \brief Adds an entry to the modules table
         * \param jsName The jsName of the element
         * \param offset The offset in the heap
         */
    void registerModule(const clang::Module* module, binary::ModuleMeta& moduleBinaryStructure);

    /// heap
    /*
         * \brief Creates a \c BinaryWriter for this file heap
         */
    BinaryWriter heap_writer();
    /*
         * \brief Creates a \c BinaryReader for this file heap
         */
    BinaryReader heap_reader();

    /// I/O
    /*
         * \brief Writes this file to the filesystem with the specified name.
         * \param filename The filename of the output file
         */
    void save(string filename);
    /*
         * \brief Writes this file to a stream.
         * \param stream
         */
    void save(std::shared_ptr<utils::Stream> stream);
};
}