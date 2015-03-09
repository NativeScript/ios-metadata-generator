set(GENERATOR_SOURCES
    main.cpp
    ModuleDiscovery.cpp
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