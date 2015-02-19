set(test_SRCS
    test.h
    main.cpp
    yamlTests.cpp
    binaryWriterTests.cpp
    binarySerializerTests.cpp
    binaryHashtableTests.cpp
    binaryTypeEncodingSerializerTests.cpp
    binaryStructuresTests.cpp
    metaContainerTests.cpp)
set(test_BIN ${PROJECT_NAME}-test)

# googletest
ExternalProject_Add(googletest
  URL http://googletest.googlecode.com/files/gtest-1.7.0.zip
  INSTALL_COMMAND ""
  PREFIX ${CMAKE_CURRENT_BINARY_DIR}/gtest
  CMAKE_GENERATOR ${CMAKE_GENERATOR}
)

ExternalProject_Get_Property(googletest source_dir)
include_directories(${source_dir}/include)

get_property(GTEST_BINARY_DIR TARGET googletest PROPERTY _EP_BINARY_DIR)
link_directories(${GTEST_BINARY_DIR})

# boost
include_directories(${Boost_INCLUDE_DIR})
link_directories(${Boost_LIBRARY_DIR})

# MetaMerge-test
add_executable(${test_BIN} ${test_SRCS})
add_dependencies(${test_BIN} googletest)
target_link_libraries(${test_BIN} ${PROJECT_NAME}Static)
target_link_libraries(${test_BIN} ${CMAKE_FIND_LIBRARY_PREFIXES}gtest.a)
target_link_libraries(${test_BIN} ${Boost_LIBRARIES})
include_directories(${PROJECT_SOURCE_DIR}/src)
