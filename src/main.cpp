#include "ModuleDiscovery.h"

int main(int argc, const char** argv) {
    std::vector<std::string> arguments;
    for (int i = 1; i < argc; i++) {
        arguments.push_back(argv[i]);
    }

    std::string sdkPath = "/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS.sdk";

    std::vector<std::string> clangArgs {
            "-v",
            "-x", "objective-c",
            "-arch", "armv7",
            "-target", "arm-apple-darwin",
            "-std=gnu99",
            "-fmodule-maps",
            "-miphoneos-version-min=7.0",
            "-isysroot", sdkPath
    };

    std::string headerContents;
    CreateUmbrellaHeaderForAmbientModules(clangArgs, &headerContents);

    printf("%s\n", headerContents.c_str());

    return 0;
}