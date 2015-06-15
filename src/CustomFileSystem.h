#pragma once
#include <clang/Basic/VirtualFileSystem.h>

class CustomFile : public clang::vfs::File {
    std::unique_ptr<clang::vfs::File> file;

public:
    CustomFile(std::unique_ptr<clang::vfs::File> file) : file(std::move(file)) {}

    llvm::ErrorOr<clang::vfs::Status> status() override {
        return file->status();
    };

    llvm::ErrorOr<std::unique_ptr<llvm::MemoryBuffer>> getBuffer(const clang::Twine &Name, int64_t FileSize = -1, bool RequiresNullTerminator = true, bool IsVolatile = false) override {
        return file->getBuffer(Name, FileSize, RequiresNullTerminator, IsVolatile);
    };

    std::error_code close() override {
        return file->close();
    };

    void setName(clang::StringRef Name) override {
        return file->setName(Name);
    };
};

class CustomFileSystem : public clang::vfs::FileSystem {
private:
    clang::vfs::FileSystem *fileSystem;

public:
    CustomFileSystem(clang::vfs::FileSystem *fileSystem)
            : fileSystem(fileSystem) {}

    llvm::ErrorOr<clang::vfs::Status> status(const clang::Twine &Path) override {
        return fileSystem->status(Path);
    };

    llvm::ErrorOr<std::unique_ptr<clang::vfs::File>> openFileForRead(const clang::Twine &Path) override {
        llvm::ErrorOr<std::unique_ptr<clang::vfs::File>> file = fileSystem->openFileForRead(Path);
        if(file) {
            return std::unique_ptr<clang::vfs::File>(new CustomFile(std::move(file.get())));
        }
        return file;
    };

    clang::vfs::directory_iterator dir_begin(const clang::Twine &Dir, std::error_code &EC) override {
        return fileSystem->dir_begin(Dir, EC);
    };
};