#include "llvm/Support/CommandLine.h"
#include "HeadersParser/Parser.h"
#include "Meta/DeclarationConverterVisitor.h"
#include "Meta/Filters/RemoveDuplicateMembersFilter.h"
#include "Meta/Filters/HandleExceptionalMetasFilter.h"
#include "Yaml/YamlSerializer.h"
#include "Binary/binarySerializer.h"
#include <ctime>

// Command line parameters
llvm::cl::opt<string> cla_isysroot("isysroot", llvm::cl::Required, llvm::cl::desc("Specify the SDK directory"), llvm::cl::value_desc("dir"));
llvm::cl::opt<string> cla_arch("arch", llvm::cl::Required, llvm::cl::desc("Specify the architecture to the clang compiler instance"), llvm::cl::value_desc("arch"));
llvm::cl::opt<string> cla_iphoneOSVersionMin("iphoneos-version-min", llvm::cl::Required, llvm::cl::desc("Specify the earliest iPhone OS version on which this program will run"), llvm::cl::value_desc("version"));
llvm::cl::opt<string> cla_target("target", llvm::cl::init("arm-apple-darwin"), llvm::cl::desc("Specify the target triple to the clang compiler instance"), llvm::cl::value_desc("triple"));
llvm::cl::opt<string> cla_std("std", llvm::cl::init("gnu99"), llvm::cl::desc("Specify the language mode to the clang compiler instance"), llvm::cl::value_desc("std-name"));
llvm::cl::list<std::string> cla_headerSearchPaths("header-search-paths", llvm::cl::ZeroOrMore, llvm::cl::Positional, llvm::cl::desc("The paths in which clag searches for header files"));
llvm::cl::list<std::string> cla_frameworkSearchPaths("framework-search-paths", llvm::cl::ZeroOrMore, llvm::cl::Positional, llvm::cl::desc("The paths in which clag searches for frameworks"));
llvm::cl::opt<string> cla_outputYamlFolder("output-yaml", llvm::cl::desc("Specify the output yaml folder"), llvm::cl::value_desc("dir"));
llvm::cl::opt<string> cla_outputBinFile("output-bin", llvm::cl::desc("Specify the output binary metadata file"), llvm::cl::value_desc("file_path"));

int main(int argc, const char** argv) {
    std::clock_t begin = clock();
    llvm::cl::ParseCommandLineOptions(argc, argv);

    // Parse the AST
    HeadersParser::ParserSettings settings = HeadersParser::ParserSettings(
            cla_isysroot.getValue(), cla_arch.getValue(),
            cla_iphoneOSVersionMin.getValue(),
            cla_target.getValue(), cla_std.getValue(),
            cla_headerSearchPaths, cla_frameworkSearchPaths);

    std::unique_ptr<clang::ASTUnit> ast = HeadersParser::Parser::parse(settings);
    if(!ast) {
        return -1;
    }

    // Convert declarations to Meta objects (by visiting the AST from DeclarationConverterVisitor)
    Meta::DeclarationConverterVisitor visitor(ast.get());
    Meta::MetaContainer& metaContainer = visitor.Traverse();

    // Filter
    metaContainer.filter(Meta::HandleExceptionalMetasFilter());
    metaContainer.mergeCategoriesInInterfaces();
    metaContainer.filter(Meta::RemoveDuplicateMembersFilter());

    // Log statistic for parsed Meta objects
    std::cout << "Result: " << metaContainer.topLevelMetasCount() << " declarations from " << metaContainer.allModulesCount()
    << " (and " << metaContainer.topLevelModulesCount() << " top level)" << " modules" << std::endl;

    // Serialize Meta objects to Yaml
    if(!cla_outputYamlFolder.empty()) {
        for (Meta::MetaContainer::top_level_modules_iterator it = metaContainer.top_level_modules_begin(); it != metaContainer.top_level_modules_end(); ++it) {
            Yaml::YamlSerializer::serialize<Meta::Module>(std::string(cla_outputYamlFolder.getValue()) + "/" + it->getName() + ".yaml", *it);
        }
    }

    // Serialize Meta objects to binary metadata
    if(!cla_outputBinFile.empty()) {
        binary::MetaFile file(metaContainer.topLevelMetasCount());
        binary::BinarySerializer serializer(&file);
        serializer.serializeContainer(metaContainer);
        std::string output = cla_outputBinFile.getValue();
        file.save(output);
    }


    std::clock_t end = clock();
    double elapsed_secs = double(end - begin) / CLOCKS_PER_SEC;
    std::cout << "Done! Running time: " << elapsed_secs << " sec " << std::endl;

    return 0;
}