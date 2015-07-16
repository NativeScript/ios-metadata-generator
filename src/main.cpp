#include <llvm/Support/CommandLine.h>
#include <llvm/Support/CommandLine.h>
#include <llvm/Support/FileSystem.h>
#include <llvm/Support/Path.h>
#include <clang/Tooling/Tooling.h>
#include <clang/Frontend/CompilerInstance.h>
#include "RemoveUnsupportedSyntaxAction.h"
#include "HeadersParser/Parser.h"
#include "Meta/DeclarationConverterVisitor.h"
#include "Meta/Filters/RemoveDuplicateMembersFilter.h"
#include "Meta/Filters/HandleExceptionalMetasFilter.h"
#include "Yaml/YamlSerializer.h"
#include "Binary/binarySerializer.h"
#include "TypeScript/DefinitionWriter.h"
#include <ctime>
#include <sstream>
#include <fstream>

// Command line parameters
llvm::cl::opt<string> cla_isysroot("isysroot", llvm::cl::Required, llvm::cl::desc("Specify the SDK directory"), llvm::cl::value_desc("dir"));
llvm::cl::opt<string> cla_arch("arch", llvm::cl::Required, llvm::cl::desc("Specify the architecture to the clang compiler instance"), llvm::cl::value_desc("arch"));
llvm::cl::opt<string> cla_iphoneOSVersionMin("iphoneos-version-min", llvm::cl::Required, llvm::cl::desc("Specify the earliest iPhone OS version on which this program will run"), llvm::cl::value_desc("version"));
llvm::cl::opt<string> cla_target("target", llvm::cl::init("arm-apple-darwin"), llvm::cl::desc("Specify the target triple to the clang compiler instance"), llvm::cl::value_desc("triple"));
llvm::cl::opt<string> cla_std("std", llvm::cl::init("gnu99"), llvm::cl::desc("Specify the language mode to the clang compiler instance"), llvm::cl::value_desc("std-name"));
llvm::cl::opt<string> cla_headerSearchPaths("header-search-paths", llvm::cl::desc("The paths in which clag searches for header files separated by space. To escape a space in a path, surround the path with quotes."), llvm::cl::value_desc("paths"));
llvm::cl::opt<string> cla_frameworkSearchPaths("framework-search-paths", llvm::cl::desc("The paths in which clag searches for frameworks separated by space. To escape a space in a path, surround the path with quotes."), llvm::cl::value_desc("paths"));
llvm::cl::opt<bool> cla_enableHeaderPreprocessingIfNeeded("enable-header-preprocessing-if-needed", llvm::cl::desc("Whether to preprocess and remove all new ObjC features (generics, nullability specifiers etc.) from headers. The preprocessing happens only if a concrete SDK is detected."));
llvm::cl::opt<string> cla_outputUmbrellaHeaderFile("output-umbrella", llvm::cl::desc("Specify the output umbrella header file"), llvm::cl::value_desc("file_path"));
llvm::cl::opt<string> cla_outputIntermediateHeadersPath("output-intermediate-headers", llvm::cl::desc("Specify the output headers folder"), llvm::cl::value_desc("folder_path"));
llvm::cl::opt<string> cla_outputYamlFolder("output-yaml", llvm::cl::desc("Specify the output yaml folder"), llvm::cl::value_desc("dir"));
llvm::cl::opt<string> cla_outputBinFile("output-bin", llvm::cl::desc("Specify the output binary metadata file"), llvm::cl::value_desc("file_path"));
llvm::cl::opt<string> cla_outputDtsFolder("output-typescript", llvm::cl::desc("Specify the output .d.ts folder"), llvm::cl::value_desc("dir"));

class MetaGenerationConsumer : public clang::ASTConsumer {
public:
    explicit MetaGenerationConsumer(clang::SourceManager& sourceManager, clang::HeaderSearch& headerSearch)
        : _headerSearch(headerSearch)
        , _visitor(sourceManager, headerSearch)
    {
    }

    virtual void HandleTranslationUnit(clang::ASTContext& Context)
    {
        llvm::SmallVector<clang::Module*, 64> modules;
        _headerSearch.collectAllModules(modules);
        _visitor.TraverseDecl(Context.getTranslationUnitDecl());
        _visitor.resolveUnresolvedBridgedInterfaces();

        Meta::MetaContainer& metaContainer = _visitor.getMetaContainer();

        // Filter
        metaContainer.filter(Meta::HandleExceptionalMetasFilter());
        metaContainer.mergeCategoriesInInterfaces();
        metaContainer.filter(Meta::RemoveDuplicateMembersFilter());

        // Log statistic for parsed Meta objects
        std::cout << "Result: " << metaContainer.topLevelMetasCount() << " declarations from " << metaContainer.topLevelModulesCount() << " top level modules" << std::endl;

        // Serialize Meta objects to Yaml
        if (!cla_outputYamlFolder.empty()) {
            llvm::sys::fs::create_directories(cla_outputYamlFolder);
            for (Meta::MetaContainer::top_level_modules_iterator it = metaContainer.top_level_modules_begin(); it != metaContainer.top_level_modules_end(); ++it) {
                Yaml::YamlSerializer::serialize<Meta::ModuleMeta>(std::string(cla_outputYamlFolder.getValue()) + "/" + it->getFullName() + ".yaml", *it);
            }
        }

        // Serialize Meta objects to binary metadata
        if (!cla_outputBinFile.empty()) {
            binary::MetaFile file(metaContainer.topLevelMetasCount());
            binary::BinarySerializer serializer(&file);
            serializer.serializeContainer(metaContainer);
            std::string output = cla_outputBinFile.getValue();
            file.save(output);
        }

        // Generate TypeScript definitions
        if (!cla_outputDtsFolder.empty()) {
            llvm::sys::fs::create_directories(cla_outputDtsFolder);
            for (auto& module : metaContainer.top_level_modules()) {
                TypeScript::DefinitionWriter definitionWriter(&module, metaContainer);

                llvm::SmallString<128> path;
                llvm::sys::path::append(path, cla_outputDtsFolder, "objc!" + module.getFullName() + ".d.ts");

                std::error_code error;
                llvm::raw_fd_ostream file(path.str(), error, llvm::sys::fs::F_Text);
                if (error) {
                    std::cout << error.message();
                    return;
                }

                file << definitionWriter.write();
                file.close();
            }
        }
    }

private:
    clang::HeaderSearch& _headerSearch;
    Meta::DeclarationConverterVisitor _visitor;
};

class MetaGenerationFrontendAction : public clang::ASTFrontendAction {
public:
    MetaGenerationFrontendAction() {}

    virtual std::unique_ptr<clang::ASTConsumer> CreateASTConsumer(clang::CompilerInstance& Compiler, llvm::StringRef InFile)
    {
        return std::unique_ptr<clang::ASTConsumer>(new MetaGenerationConsumer(Compiler.getASTContext().getSourceManager(), Compiler.getPreprocessor().getHeaderSearchInfo()));
    }
};

std::string replaceString(std::string subject, const std::string& search, const std::string& replace)
{
    size_t pos = 0;
    while ((pos = subject.find(search, pos)) != std::string::npos) {
        subject.replace(pos, search.length(), replace);
        pos += replace.length();
    }
    return subject;
}

int main(int argc, const char** argv)
{
    std::clock_t begin = clock();

    // Parse clang arguments from command line
    llvm::cl::ParseCommandLineOptions(argc, argv);

    std::vector<std::string> clangArgs{
        "-v",
        "-x", "objective-c",
        "-fno-objc-arc",
        "-fmodule-maps",
        "-isysroot", cla_isysroot.getValue(),
        "-arch", cla_arch.getValue(),
        "-target", cla_target.getValue(),
        std::string("-std=") + cla_std.getValue(),
        std::string("-miphoneos-version-min=") + cla_iphoneOSVersionMin.getValue(),
        "-Wno-unknown-pragmas", "-Wno-ignored-attributes", "-ferror-limit=0"
    };

    printf("Parsed header search paths:\n");
    for (std::string path : parsePaths(cla_headerSearchPaths.getValue())) {
        printf("\"%s\"\n", path.c_str());
        clangArgs.push_back(std::string("-I") + path);
    }
    printf("Parsed framework search paths:\n");
    for (std::string path : parsePaths(cla_frameworkSearchPaths.getValue())) {
        printf("\"%s\"\n", path.c_str());
        clangArgs.push_back(std::string("-F") + path);
    }

    // log compiler settings
    std::cout << "Clang parameters: ";
    for (std::vector<std::string>::iterator it = clangArgs.begin(); it != clangArgs.end(); ++it) {
        std::cout << *it << " ";
    }
    std::cout << std::endl;

    // Create umbrella header
    std::string umbrellaContent = CreateUmbrellaHeader(clangArgs);

    // Save the umbrella file
    std::string umbrellaFile = cla_outputUmbrellaHeaderFile.getValue();
    if (!umbrellaFile.empty()) {
        std::error_code errorCode;
        llvm::raw_fd_ostream umbrellaFileStream(umbrellaFile, errorCode, llvm::sys::fs::OpenFlags::F_None);
        if (!errorCode) {
            umbrellaFileStream << umbrellaContent;
            umbrellaFileStream.close();
        }
    }

    clang::tooling::FileContentMappings filesMappings;
    // Remove not supported syntax in headers
    if (cla_enableHeaderPreprocessingIfNeeded.getValue()) {
        // Extract SDK version form isysroot path. This depends on specific naming convention of SDK folders.
        std::string isysroot = cla_isysroot.getValue();
        std::size_t lastPathComponentIndex = isysroot.find_last_of("/");
        std::size_t sdkVersionDigit = isysroot.find_first_of("0123456789", lastPathComponentIndex);
        bool isLowerThaniOS9 = (isysroot[sdkVersionDigit] - '0' < 9 && isysroot[sdkVersionDigit + 1] == '.');

        if (!isLowerThaniOS9) {
            std::map<std::string, std::stringstream> filesMap;
            clang::tooling::runToolOnCodeWithArgs(new RemoveUnsupportedSyntaxAction(filesMap), umbrellaContent, clangArgs, "umbrella.h");

            // save the output header file on the file system (optional)
            std::string sdkHeaderOutputFolder = cla_outputIntermediateHeadersPath.getValue();
            if (!sdkHeaderOutputFolder.empty()) {
                for (auto& pair : filesMap) {
                    if (!pair.first.empty()) {
                        std::error_code errorCode;
                        llvm::raw_fd_ostream outputFileStream(sdkHeaderOutputFolder + replaceString(pair.first, "/", "|"), errorCode, llvm::sys::fs::OpenFlags::F_None);
                        if (!errorCode) {
                            outputFileStream << pair.second.str();
                            outputFileStream.close();
                        }
                    }
                }
            }

            for (auto& pair : filesMap) {
                if (!pair.first.empty())
                    filesMappings.push_back(std::pair<std::string, std::string>(pair.first, pair.second.str()));
            }
        }
    }

    // generate metadata for the intermediate sdk header
    clang::tooling::runToolOnCodeWithArgs(new MetaGenerationFrontendAction(), umbrellaContent, clangArgs, "umbrella.h", filesMappings);

    std::clock_t end = clock();
    double elapsed_secs = double(end - begin) / CLOCKS_PER_SEC;
    std::cout << "Done! Running time: " << elapsed_secs << " sec " << std::endl;

    return 0;
}