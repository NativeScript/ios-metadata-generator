#include "HeadersParser/Parser.h"
#include "Meta/DeclarationConverterVisitor.h"
#include "Yaml/YamlSerializer.h"
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
            "/Users/buhov/Desktop/NS/ios-runtime/build/ios-sdk-umbrella-headers/ios8.0.h", // umbrella header
            "armv7" // architecture
    );
    std::unique_ptr<clang::ASTUnit> ast = HeadersParser::Parser::parse(settings);

    // Convert declarations to Meta objects (by visiting the AST from DeclarationConverterVisitor)
    Meta::DeclarationConverterVisitor visitor(ast.get());
    std::map<std::string, std::shared_ptr<Meta::Module>> topLevelModules;
    visitor.Traverse(topLevelModules);

    // Log statistic for parsed Meta objects
    int totalCount = 0;
    std::map<Meta::MetaType, int> countByTypes;
    for (std::map<std::string, std::shared_ptr<Meta::Module>>::const_iterator it = topLevelModules.begin(); it != topLevelModules.end(); ++it) {
        std::cout << it->second->getName() << " -> " << it->second->size() << std::endl;
        totalCount += it->second->size();
        for(Meta::Module::iterator i = it->second->begin(); i != it->second->end(); ++i) {
            countByTypes[(*i)->type] += 1;
        }
    }
    for (std::map<Meta::MetaType, int>::const_iterator it = countByTypes.begin(); it != countByTypes.end(); ++it) {
        std::cout << it->first << " -> " <<it->second << std::endl;
    }
    std::cout << "All declarations: " << totalCount << " from " << topLevelModules.size() << " modules" << std::endl;

    // Serialize Meta objects to Yaml
    for (std::map<std::string, std::shared_ptr<Meta::Module>>::const_iterator it = topLevelModules.begin(); it != topLevelModules.end(); ++it) {
        std::string topLevelModuleName = it->second->getName();
        Yaml::YamlSerializer::serialize<Meta::Module>(std::string("/Users/buhov/Desktop/new-generator-yaml/") + topLevelModuleName + ".yaml", *it->second.get());
    }

    std::clock_t end = clock();
    double elapsed_secs = double(end - begin) / CLOCKS_PER_SEC;
    printf("Running time: %f sec", elapsed_secs);

    return 0;
}