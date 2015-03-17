#pragma once

#include <string>
#include <map>
#include <vector>
#include <clang/AST/DeclBase.h>
#include "MetaEntities.h"

namespace Meta {

    class JsNameGenerator {
    public:
        static std::map<clang::Decl::Kind, std::vector<string>>& getIosSdkNamesToRecalculate();

        JsNameGenerator(std::map<clang::Decl::Kind, std::vector<string>>& namesToRecalculate)
            : _namesToRecalculate(namesToRecalculate) {}

        std::string getJsName(clang::NamedDecl& decl) {
            std::string originalName = calculateOriginalName(decl);
            std::string jsName = calculateJsName(decl, originalName);
            vector<string> namesToCheck = _namesToRecalculate[decl.getKind()];
            if(std::find(namesToCheck.begin(), namesToCheck.end(), originalName) != namesToCheck.end()) {
                jsName = recalculateJsName(decl, jsName);
            }
            return jsName;
        }

    private:
        std::string calculateOriginalName(clang::NamedDecl& decl);

        std::string calculateJsName(clang::NamedDecl& decl, std::string originalName);

        std::string recalculateJsName(clang::NamedDecl& decl, std::string calculatedJsName);

        std::map<clang::Decl::Kind, std::vector<std::string>> _namesToRecalculate;
    };
}