#pragma once

#include <string>
#include <vector>

namespace HeadersParser {

    class ParserSettings {

    public:
        ParserSettings(std::string sdkPath, std::vector<std::string> headerSearchPaths, std::string architecture)
                : _sdkPath(sdkPath),
                  _headerSearchPaths(headerSearchPaths),
                  _architecture(architecture) {}

        std::string getSdkPath() {
            return this->_sdkPath;
        }

        std::vector<std::string> getHeaderSearchPaths() {
            return this->_headerSearchPaths;
        }

        std::string getArchitecture() {
            return this->_architecture;
        }

    private:
        std::string _sdkPath;
        std::vector<std::string> _headerSearchPaths;
        std::string _architecture;
    };
}