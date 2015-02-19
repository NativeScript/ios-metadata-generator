#include <iostream>
#include "boost/filesystem/operations.hpp"
#include "utils/metaReader.h"
#include "filters/mergeCategoriesFilter.h"
#include "binary/binarySerializer.h"

using namespace std;

void readFile(const boost::filesystem::path& filepath, utils::MetaReader& reader, utils::MetaContainer& container)
{
    if (boost::filesystem::is_directory(filepath)) {
        boost::filesystem::directory_iterator endItr;
        for (boost::filesystem::directory_iterator itr(filepath); itr != endItr; ++itr) {
            readFile(itr->path(), reader, container);
        }
    }
    else {
        reader.readFile(filepath, container);
    }
}

void doMetaMerge(const string& yamlfile, const string& output)
{
    utils::MetaContainer container;

    cout << "Reading metadata files:" << endl;
    utils::MetaReader reader;
    readFile(yamlfile, reader, container);

    if (container.size() == 0)
        return;

    cout << "Merging..." << endl;
    filters::MergeCategoriesFilter filter;
    container.filter({ &filter });

    cout << "Generating binary metadata..." << endl;
    binary::MetaFile file(container.size());
    binary::BinarySerializer serializer(&file);
    container.serialize(&serializer);

    cout << "Writing binary metadata into " << output << endl;
    boost::filesystem::path outputPath(output);
    if (!outputPath.parent_path().empty() && !boost::filesystem::exists(outputPath.parent_path())) {
        boost::filesystem::create_directory(outputPath.parent_path());
    }
    file.save(output);
    cout << "Done!" << endl;
}

int main(int argc, char* argv[])
{
    if (argc > 1)
    {
        string output = "metadata.bin";
        if (argc > 2) {
            output = string(argv[2]);
        }
        doMetaMerge(string(argv[1]), output);
    }

    return 0;
}