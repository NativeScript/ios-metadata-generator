#include "HeadersParser/Parser.h"
#include "Meta/DeclarationConverterVisitor.h"
#include "Meta/Filters/RemoveDuplicateMembersFilter.h"
#include "Meta/Filters/HandleExceptionalMetasFilter.h"
#include "Yaml/YamlSerializer.h"
#include "Binary/binarySerializer.h"
#include <ctime>

int main(int argc, const char** argv) {
    std::clock_t begin = clock();

    // TODO: Use the command line arguments
    std::vector<std::string> arguments;
    for (int i = 1; i < argc; i++) {
        arguments.push_back(argv[i]);
    }

    // Parse the AST
    HeadersParser::ParserSettings settings = HeadersParser::ParserSettings(
            "/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS.sdk", // sdk path
            //"/Users/buhov/Desktop/NS/ios-runtime/tests/NativeScriptTests/NativeScriptTests/TNSTestCases.h", // umbrella header
            "/Users/buhov/Desktop/NS/ios-runtime/build/ios-sdk-umbrella-headers/ios8.0.h", // umbrella header
            "armv7" // architecture
    );
    std::unique_ptr<clang::ASTUnit> ast = HeadersParser::Parser::parse(settings);

    // Convert declarations to Meta objects (by visiting the AST from DeclarationConverterVisitor)
    Meta::DeclarationConverterVisitor visitor(ast.get());
    Meta::MetaContainer& metaContainer = visitor.Traverse();
    metaContainer.mergeCategoriesInInterfaces();

    // Filter
    metaContainer.filter(Meta::RemoveDuplicateMembersFilter());
    metaContainer.filter(Meta::HandleExceptionalMetasFilter());

    // Log statistic for parsed Meta objects
    int totalCount = 0;
    std::map<Meta::MetaType, int> countByTypes;
    for (Meta::MetaContainer::const_top_level_modules_iterator it = metaContainer.top_level_modules_begin(); it != metaContainer.top_level_modules_end(); ++it) {
        std::cout << it->getName() << " -> " << it->size() << std::endl;
        totalCount += it->size();
        for(Meta::Module::const_iterator i = it->begin(); i != it->end(); ++i) {
            countByTypes[i->second->type] += 1;
        }
    }
    for (std::map<Meta::MetaType, int>::const_iterator it = countByTypes.begin(); it != countByTypes.end(); ++it) {
        std::cout << it->first << " -> " <<it->second << std::endl;
    }
    std::cout << "All declarations: " << metaContainer.topLevelMetasCount() << " from " << metaContainer.topLevelModulesCount() << " modules" << std::endl;

    // Serialize Meta objects to Yaml
    for (Meta::MetaContainer::top_level_modules_iterator it = metaContainer.top_level_modules_begin(); it != metaContainer.top_level_modules_end(); ++it) {
        Yaml::YamlSerializer::serialize<Meta::Module>(std::string("/Users/buhov/Desktop/new-generator-yaml/") + it->getName() + ".yaml", *it);
    }

    // Serialize Meta objects to binary metadata
    binary::MetaFile file(metaContainer.topLevelMetasCount());
    binary::BinarySerializer serializer(&file);
    serializer.serializeContainer(metaContainer);
    std::string output = "/Users/buhov/Desktop/binMetadata/metadata.bin";
    file.save(output);
    std::cout << "Done!" << std::endl;


    std::clock_t end = clock();
    double elapsed_secs = double(end - begin) / CLOCKS_PER_SEC;
    printf("Running time: %f sec", elapsed_secs);

    return 0;
}