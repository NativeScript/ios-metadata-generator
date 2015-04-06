#include "Parser.h"

#include <clang/Frontend/ASTUnit.h>
#include <clang/Tooling/Tooling.h>
#include <clang/Lex/Preprocessor.h>
#include <clang/Lex/HeaderSearch.h>
#include <llvm/ADT/StringSwitch.h>

std::unique_ptr<clang::ASTUnit> HeadersParser::Parser::parse(ParserSettings& settings) {

    std::string umbrellaHeaderPath = settings.getUmbrellaHeader().substr(0, settings.getUmbrellaHeader().find_last_of("/\\"));

    std::vector<std::string> clangArgs {
            "-v",
            "-x", "objective-c",
            "-arch", settings.getArchitecture(),
            "-target", "arm-apple-darwin",
            "-std=gnu99",
            "-fno-objc-arc",
            "-miphoneos-version-min=7.0",
            "-fmodule-maps",
            "-resource-dir", "/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/lib/clang/6.0",
            // TODO: remove these lines
            std::string("-I") + umbrellaHeaderPath, // for resolving relative to the umbrella header includes
            //"-I/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/lib/clang/6.0/include",
            //std::string("-fmodule-map-file=") + settings.getSdkPath() + "/usr/include/module.map",
            //std::string("-fmodule-map-file=") + settings.getSdkPath() + "/usr/include/dispatch/module.map",
            //std::string("-I") + settings.getSdkPath() + "/usr/include/objc",
            "-isysroot", settings.getSdkPath()
    };

    if(llvm::ErrorOr<std::unique_ptr<llvm::MemoryBuffer>> umbrellaBuffer = llvm::MemoryBuffer::getFile(settings.getUmbrellaHeader())) {
        // Build and return the AST
        std::unique_ptr<clang::ASTUnit> ast = clang::tooling::buildASTFromCodeWithArgs(umbrellaBuffer.get()->getBuffer(), clangArgs, "umbrella.h");
        llvm::SmallVector<clang::Module*, 64> modules;
        ast->getPreprocessor().getHeaderSearchInfo().collectAllModules(modules);
        return ast;
    }
    throw "Can't open the umbrella header file.";
}