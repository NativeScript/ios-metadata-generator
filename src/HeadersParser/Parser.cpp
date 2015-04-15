#include "Parser.h"

#include <clang/Frontend/ASTUnit.h>
#include <clang/Tooling/Tooling.h>
#include <clang/Lex/Preprocessor.h>
#include <clang/Lex/HeaderSearch.h>
#include <llvm/ADT/StringSwitch.h>
#include <iostream>

using namespace clang;
namespace path = llvm::sys::path;
namespace fs = llvm::sys::fs;

static SmallVectorImpl<char>& operator+=(SmallVectorImpl<char>& includes, StringRef rhs) {
    includes.append(rhs.begin(), rhs.end());
    return includes;
}

static std::error_code addHeaderInclude(StringRef headerName, SmallVectorImpl<char>& includes) {
    includes += "#import \"";

    // Use an absolute path for the include; there's no reason to think whether a relative path will
    // work (. might not be on our include path) or that it will find the same file.
    if (path::is_absolute(headerName)) {
        includes += headerName;
    } else {
        SmallString<256> header = headerName;
        if (std::error_code err = fs::make_absolute(header))
            return err;
        includes += header;
    }

    includes += "\"\n";

    return std::error_code();
}

static std::error_code addHeaderInclude(const FileEntry* header, SmallVectorImpl<char>& includes) {
    return addHeaderInclude(header->getName(), includes);
}

static std::error_code collectModuleHeaderIncludes(FileManager& fileMgr, ModuleMap& modMap, const Module* module, SmallVectorImpl<char>& includes) {
    // Don't collect any headers for unavailable modules.
    if (!module->isAvailable())
        return std::error_code();

    for (auto header : module->Headers[Module::HK_Normal]) {
        if (auto err = addHeaderInclude(header.Entry, includes))
            return err;
    }

    if (auto umbrellaHeader = module->getUmbrellaHeader()) {
        if (std::error_code err = addHeaderInclude(umbrellaHeader, includes))
            return err;
    } else if (const DirectoryEntry* umbrellaDir = module->getUmbrellaDir()) {
        // Add all of the headers we find in this subdirectory.
        std::error_code ec;
        SmallString<128> dirNative;
        path::native(umbrellaDir->getName(), dirNative);
        for (fs::recursive_directory_iterator dir(dirNative.str(), ec), dirEnd; dir != dirEnd && !ec; dir.increment(ec)) {
            // Check whether this entry has an extension typically associated with headers.
            if (!llvm::StringSwitch<bool>(path::extension(dir->path()))
                    .Cases(".h", ".H", true)
                    .Default(false))
                continue;

            // If this header is marked 'unavailable' in this module, don't include it.
            if (const FileEntry *header = fileMgr.getFile(dir->path())) {
                if (modMap.isHeaderUnavailableInModule(header, module))
                    continue;

                addHeaderInclude(header, includes);
            }

            // Include this header as part of the umbrella directory.
            if (auto err = addHeaderInclude(dir->path(), includes))
                return err;
        }

        if (ec)
            return ec;
    }

    return std::error_code();
}

std::error_code CreateUmbrellaHeaderForAmbientModules(const std::vector<std::string>& args, std::string* umbrellaHeaderContents, const std::vector<std::string>& moduleBlacklist) {
    std::unique_ptr<clang::ASTUnit> ast = clang::tooling::buildASTFromCodeWithArgs("", args, "umbrella.h");
    if (!ast)
        return std::error_code(-1, std::generic_category());

    clang::SmallVector<clang::Module*, 64> modules;
    HeaderSearch& headerSearch = ast->getPreprocessor().getHeaderSearchInfo();
    headerSearch.collectAllModules(modules);

    ModuleMap& moduleMap = headerSearch.getModuleMap();
    FileManager& fileManager = ast->getFileManager();

    SmallString<256> headerContents;
    std::function<void (const Module*)> collector = [&](const Module* module) {
        if (std::find(moduleBlacklist.begin(), moduleBlacklist.end(), module->getFullModuleName()) != moduleBlacklist.end())
            return;
        collectModuleHeaderIncludes(fileManager, moduleMap, module, headerContents);
        std::for_each(module->submodule_begin(), module->submodule_end(), collector);
    };

    std::for_each(modules.begin(), modules.end(), collector);

    if (umbrellaHeaderContents)
        *umbrellaHeaderContents = headerContents.str();

    return std::error_code();
}

std::unique_ptr<ASTUnit> HeadersParser::Parser::parse(ParserSettings& settings) {

    std::vector<std::string> clangArgs {
            //"-v",
            "-x", "objective-c",
            "-fno-objc-arc",
            "-fmodule-maps",
            "-isysroot", settings.getSysRoot(),
            "-arch", settings.getArch(),
            "-target", settings.getTarget(),
            std::string("-std=") + settings.getStd(),
            std::string("-miphoneos-version-min=") + settings.getIPhoneOsVersionMin(),
    };

    // add header search paths
    std::vector<std::string> headersSearchPaths = settings.getHeaderSearchPaths();
    for(auto it = headersSearchPaths.begin(); it != headersSearchPaths.end(); ++it) {
        clangArgs.push_back(std::string("-I") + *it);
    }

    // add framework search paths
    std::vector<std::string> frameworksSearchPaths = settings.getFrameworkSearchPaths();
    for(auto it = frameworksSearchPaths.begin(); it != frameworksSearchPaths.end(); ++it) {
        clangArgs.push_back(std::string("-F") + *it);
    }

    // log clang parameters
    std::cout << "Clang parameters: ";
    for(std::vector<std::string>::iterator it = clangArgs.begin(); it != clangArgs.end(); ++it) {
        std::cout << *it << " ";
    }
    std::cout << std::endl;


    std::string umbrellaHeaderContents;
    std::vector<std::string> moduleBlacklist;

    // Generate umbrella header for all modules from the sdk
    CreateUmbrellaHeaderForAmbientModules(clangArgs, &umbrellaHeaderContents, moduleBlacklist);

    // Build and return the AST
    std::unique_ptr<clang::ASTUnit> ast = clang::tooling::buildASTFromCodeWithArgs(umbrellaHeaderContents, clangArgs, "umbrella.h");
    if(!ast) {
        return nullptr;
    }
    SmallVector<Module*, 64> modules;
    ast->getPreprocessor().getHeaderSearchInfo().collectAllModules(modules);
    return ast;
}