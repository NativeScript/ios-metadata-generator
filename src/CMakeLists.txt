set(GENERATOR_HEADERS
    Binary/binaryHashtable.h
    Binary/binaryOperation.h
    Binary/binaryReader.h
    Binary/binarySerializer.h
    Binary/binarySerializerPrivate.h
    Binary/binaryStructures.h
    Binary/binaryTypeEncodingSerializer.h
    Binary/binaryWriter.h
    Binary/metaFile.h
    HeadersParser/Parser.h
    Meta/CreationException.h
    Meta/DeclarationConverterVisitor.h
    Meta/Filters/HandleExceptionalMetasFilter.h
    Meta/Filters/MergeCategoriesFilter.h
    Meta/Filters/RemoveDuplicateMembersFilter.h
    Meta/Filters/ResolveGlobalNamesCollisionsFilter.h
    Meta/MetaEntities.h
    Meta/MetaFactory.h
    Meta/MetaVisitor.h
    Meta/TypeEntities.h
    Meta/TypeFactory.h
    Meta/TypeVisitor.h
    Meta/Utils.h
    TypeScript/DefinitionWriter.h
    Utils/fileStream.h
    Utils/memoryStream.h
    Utils/stream.h
    Utils/StringHasher.h
    Utils/Noncopyable.h
    Yaml/MetaYamlTraits.h
    Yaml/YamlSerializer.h
)

set(GENERATOR_SOURCES
    main.cpp
    HeadersParser/Parser.cpp
    Meta/DeclarationConverterVisitor.cpp
    Meta/MetaFactory.cpp
    Meta/MetaEntities.cpp
    Meta/TypeFactory.cpp
    Meta/Utils.cpp
    Meta/Filters/HandleExceptionalMetasFilter.cpp
    Meta/Filters/MergeCategoriesFilter.cpp
    Meta/Filters/RemoveDuplicateMembersFilter.cpp
    Meta/Filters/ResolveGlobalNamesCollisionsFilter.cpp
    Utils/fileStream.cpp
    Utils/memoryStream.cpp
    Binary/binaryHashtable.cpp
    Binary/binaryReader.cpp
    Binary/binarySerializer.cpp
    Binary/binaryStructures.cpp
    Binary/binaryTypeEncodingSerializer.cpp
    Binary/binaryWriter.cpp
    Binary/metaFile.cpp
    TypeScript/DefinitionWriter.cpp
)

include_directories(${CMAKE_CURRENT_SOURCE_DIR})

add_executable(objc-metadata-generator ${GENERATOR_HEADERS} ${GENERATOR_SOURCES} Meta/Filters/ResolveGlobalNamesCollisionsFilter.cpp Meta/Filters/ResolveGlobalNamesCollisionsFilter.h)
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

set_target_properties(objc-metadata-generator PROPERTIES
    COMPILE_FLAGS "-Werror -Wall -Wextra -Wno-unused-parameter"
)

add_custom_command(TARGET objc-metadata-generator
                    POST_BUILD
                    COMMAND ${CMAKE_COMMAND} -E copy_directory ${LLVM_LIBDIR}/clang ${CMAKE_LIBRARY_OUTPUT_DIRECTORY}/clang)

install(TARGETS objc-metadata-generator
        RUNTIME DESTINATION bin)

install(DIRECTORY ${CMAKE_LIBRARY_OUTPUT_DIRECTORY}/clang DESTINATION lib)

get_target_property(_target_sources objc-metadata-generator SOURCES)
foreach(child ${_target_sources})
    get_filename_component(child_directory ${child} DIRECTORY)
    string(REPLACE "/" "\\" groupName "${child_directory}")
    source_group("${groupName}" FILES ${child})
endforeach()
