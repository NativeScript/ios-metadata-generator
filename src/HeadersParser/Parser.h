#pragma once

#include <clang/Frontend/ASTUnit.h>
#include "ParserSettings.h"

namespace HeadersParser {
    class Parser {
    public:
        static std::unique_ptr<clang::ASTUnit> parse(ParserSettings& settings, std::string umbrellaFile);
    };
}