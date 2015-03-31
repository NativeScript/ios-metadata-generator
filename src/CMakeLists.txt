set(GENERATOR_SOURCES
    main.cpp
    HeadersParser/Parser.cpp
    Meta/DeclarationConverterVisitor.cpp
    Meta/MetaFactory.cpp
    Meta/MetaEntities.cpp
    Meta/TypeFactory.cpp
    Meta/TypeEntities.cpp
    Meta/IdentifierGenerator.cpp
)

add_executable(objc-metadata-generator ${GENERATOR_SOURCES})
target_link_libraries(objc-metadata-generator ${LLVM_LINKER_FLAGS})
target_link_libraries(objc-metadata-generator
    clangFrontend
    clangSerialization
    clangDriver
    clangParse
    clangSema
    clangAnalysis
    clangAST
    clangBasic
    clangEdit
    clangLex
    clangTooling
)