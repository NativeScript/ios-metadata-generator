#pragma once

#include <string>
#include <vector>

namespace HeadersParser {

    class ParserSettings {

    public:
        ParserSettings(std::string sdkPath, std::string umbrellaHeader, std::string architecture)
                : _sdkPath(sdkPath),
                  _umbrellaHeader(umbrellaHeader),
                  _architecture(architecture) {}

        std::string& getSdkPath() {
            return this->_sdkPath;
        }

        std::string& getUmbrellaHeader() {
            return this->_umbrellaHeader;
        }

        std::string& getArchitecture() {
            return this->_architecture;
        }

    private:
        std::string _sdkPath;
        std::string _umbrellaHeader;
        std::string _architecture;
    };
}