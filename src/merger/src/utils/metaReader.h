#ifndef METAREADER_H
#define METAREADER_H

#include <string>
#include "boost/filesystem/path.hpp"
#include "metaContainer.h"

namespace utils {
    /*
     * \class MetaReader
     * \brief Provides tools for reading yaml files and parsing its contents.
     */
    class MetaReader {
    public:
        /*
         * \brief Read and parse a yaml file, storing its contents in a container.
         * \param filepath A file path to a yaml file
         * \param container Reference to a \c MetaContainer object in which meta objects will be added
         */
        void readFile(const boost::filesystem::path& filepath, MetaContainer& container);
    };
}

#endif