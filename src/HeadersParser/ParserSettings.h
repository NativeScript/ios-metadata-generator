#pragma once

#include <string>
#include <vector>

namespace HeadersParser {

    class ParserSettings {

    public:
        ParserSettings(std::string sdkPath, std::string architecture)
                : sdkPath(sdkPath),
                  architecture(architecture) {}

        std::string& getSdkPath() {
            return this->sdkPath;
        }

        std::string& getArchitecture() {
            return this->architecture;
        }

    private:
        std::string sdkPath;
        std::string architecture;
    };
}