project(MetadataGenerator)
cmake_minimum_required(VERSION 2.8)

find_program(LLVM_CONFIG_EXE
             NAMES llvm-config-3.7 llvm-config)
if (NOT EXISTS ${LLVM_CONFIG_EXE})
    message(FATAL_ERROR "llvm-config not found")
endif ()
macro(llvm_config option var)
    execute_process(
        COMMAND ${LLVM_CONFIG_EXE} --${option}
        OUTPUT_VARIABLE ${var}
        OUTPUT_STRIP_TRAILING_WHITESPACE
    )
endmacro()

llvm_config(system-libs LLVM_SYSTEM_LIBS)
llvm_config(cppflags LLVM_PREPROCESSOR_FLAGS)
llvm_config(libs LLVM_LIBS)
llvm_config(ldflags LLVM_LINKER_FLAGS)
llvm_config(libdir LLVM_LIBDIR)

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11 ${LLVM_PREPROCESSOR_FLAGS}")
set(LLVM_LINKER_FLAGS "${LLVM_LINKER_FLAGS} ${LLVM_SYSTEM_LIBS} ${LLVM_LIBS}")

set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)

add_subdirectory(src)