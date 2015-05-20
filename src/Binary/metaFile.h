#pragma once

#include "binaryHashtable.h"
#include "binaryWriter.h"
#include "binaryReader.h"
#include "../Utils/memoryStream.h"
#include <vector>
#include <memory>

using namespace std;

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
        std::shared_ptr<utils::MemoryStream> _heap;

        // file properties
        int pointer_size = 4;
        int array_count_size = 4;
        MetaFileOffset globalTable_offset = 0;
        MetaFileOffset heap_offset = 0;
        MetaFileOffset _offset = 0;

    public:
        /*
         * \brief Constructs a \c MetaFile with the given size
         * \param size The number of meta objects this file will contain
         */
        MetaFile(int size) {
            this->_globalTableSymbols = std::unique_ptr<BinaryHashtable>(new BinaryHashtable(size));
            this->_heap = std::shared_ptr<utils::MemoryStream>(new utils::MemoryStream());
            this->_heap->push_byte(0); // mark heap
        }
        MetaFile() : MetaFile(10) { }

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
        void registerInGlobalTable(const std::string& jsName, MetaFileOffset offset);
        /*
         * \brief Returns the offset to which the specified jsName is mapped in the global table.
         * \param jsName The jsName of the element
         * \return The offset in the heap
         */
        MetaFileOffset getFromGlobalTable(const std::string& jsName);

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