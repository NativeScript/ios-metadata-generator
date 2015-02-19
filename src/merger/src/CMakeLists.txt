set(HEADER_FILES
    meta/meta.h
    utils/metaReader.h
    utils/metaContainer.h
    utils/serializer.h
    utils/StringHasher.h
    utils/stream.h
    utils/memoryStream.h
    utils/fileStream.h
    filters/metaFilter.h
    filters/mergeCategoriesFilter.h
    typeEncoding/typeEncoding.h
    binary/binaryStructures.h
    binary/binaryWriter.h
    binary/binaryReader.h
    binary/binaryHashtable.h
    binary/metaFile.h
    binary/binaryTypeEncodingSerializer.h
    binary/binarySerializer.h)

set(SOURCE_FILES
    meta/meta.cpp
    utils/metaReader.cpp
    utils/metaContainer.cpp
    utils/memoryStream.cpp
    utils/fileStream.cpp
    filters/mergeCategoriesFilter.cpp
    typeEncoding/typeEncoding.cpp
    binary/binaryStructures.cpp
    binary/binaryWriter.cpp
    binary/binaryReader.cpp
    binary/binaryHashtable.cpp
    binary/metaFile.cpp
    binary/binaryTypeEncodingSerializer.cpp
    binary/binarySerializer.cpp)

include_directories(${Boost_INCLUDE_DIR})
link_directories(${Boost_LIBRARY_DIR})

add_library(${PROJECT_NAME}Static ${SOURCE_FILES} ${HEADER_FILES})
add_executable(${PROJECT_NAME} main.cpp)
add_dependencies(${PROJECT_NAME}Static yaml-cpp)

target_link_libraries(${PROJECT_NAME}Static ${Boost_LIBRARIES})
target_link_libraries(${PROJECT_NAME}Static yaml-cpp.a)
target_link_libraries(${PROJECT_NAME} ${PROJECT_NAME}Static)
