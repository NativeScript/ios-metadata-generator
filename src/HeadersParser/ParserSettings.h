#pragma once

#include <vector>

static std::vector<std::string> parsePaths(std::string paths) {
    std::vector<std::string> result;
    char buffer[paths.size() + 1];
    int bufferSize = 0;
    bool inQuote = false;
    for(char& c : paths) {
        if (c == ' ' && !inQuote) {
            if (bufferSize != 0) {
                buffer[bufferSize] = '\0';
                result.push_back(std::string(buffer));
                bufferSize = 0;
            }
            continue;
        }

        if(c == '\"') {
            if(inQuote) {
                buffer[bufferSize] = '\0';
                result.push_back(std::string(buffer));
            }
            inQuote = !inQuote;
            bufferSize = 0;
            continue;
        }

        buffer[bufferSize] = c;
        bufferSize++;
    }
    if (bufferSize != 0) {
        buffer[bufferSize] = '\0';
        result.push_back(std::string(buffer));
        bufferSize = 0;
    }
    return result;
}

namespace HeadersParser {

    class ParserSettings {

    public:
        ParserSettings(std::string sysroot, std::string arch, std::string iPhoneOsVersionMin, std::string target,
                       std::string std, std::string headerSearchPaths, std::string frameworkSearchPaths)
                : _sysroot(sysroot),
                  _arch(arch),
                  _iPhoneOsVersionMin(iPhoneOsVersionMin),
                  _target(target),
                  _std(std),
                  _headerSearchPaths(parsePaths(headerSearchPaths)),
                  _frameworkSearchPaths(parsePaths(frameworkSearchPaths)) { }

        std::string getSysRoot() {
            return this->_sysroot;
        }

        std::string getArch() {
            return this->_arch;
        }

        std::string getIPhoneOsVersionMin() {
            return this->_iPhoneOsVersionMin;
        }

        std::string getTarget() {
            return this->_target;
        }

        std::string getStd() {
            return this->_std;
        }

        std::vector<std::string> getHeaderSearchPaths() {
            return this->_headerSearchPaths;
        }

        std::vector<std::string> getFrameworkSearchPaths() {
            return this->_frameworkSearchPaths;
        }

    private:
        std::string _sysroot;
        std::string _arch;
        std::string _iPhoneOsVersionMin;
        std::string _target;
        std::string _std;
        std::vector<std::string> _headerSearchPaths;
        std::vector<std::string> _frameworkSearchPaths;
    };
}