#include "HeadersParser/Parser.h"
#include "Meta/DeclarationConverterVisitor.h"

int main(int argc, const char** argv) {
    std::vector<std::string> arguments;
    for (int i = 1; i < argc; i++) {
        arguments.push_back(argv[i]);
    }

    // Parse the AST
    HeadersParser::ParserSettings settings = HeadersParser::ParserSettings(
            "/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS.sdk", // sdk path
            "/Users/buhov/Desktop/NS/ios-runtime/build/ios-sdk-umbrella-headers/ios8.0.h", // umbrella header
            "armv7" // architecture
    );
    std::unique_ptr<clang::ASTUnit> ast = HeadersParser::Parser::parse(settings);

    // Convert declarations to Meta objects (by visiting the AST from DeclarationConverterVisitor)
    Meta::DeclarationConverterVisitor visitor(ast.get());
    std::vector<std::shared_ptr<Meta::Meta>> metaObjects;
    visitor.Traverse(metaObjects);
    std::cout << "All declarations: " << metaObjects.size() << std::endl;

    return 0;
}