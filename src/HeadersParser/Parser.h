#pragma once

#include <string>
#include <vector>
#include <iostream>
#include <clang/AST/RecursiveASTVisitor.h>
#include <clang/Frontend/ASTUnit.h>
#include <clang/Tooling/Tooling.h>
#include "ParserSettings.h"

std::error_code
CreateUmbrellaHeaderForAmbientModules(const std::vector<std::string>& args,
        std::string* umbrellaHeaderContents,
        const std::vector<std::string>& moduleBlacklist);

namespace HeadersParser {
    class Parser {
    public:
        static std::unique_ptr<clang::ASTUnit> parse(ParserSettings& settings);
    };
}