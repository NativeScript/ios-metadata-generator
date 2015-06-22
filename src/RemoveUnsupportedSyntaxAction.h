//
// Created by Ivan Buhov on 6/18/15.
//

#pragma once

#include <sstream>
#include <clang/Lex/Preprocessor.h>
#include <clang/Frontend/FrontendActions.h>
#include <clang/Frontend/CompilerInstance.h>

class RemoveUnsupportedSyntaxAction;

class RemoveUnsupportedSyntaxActionPPCallbacks : public clang::PPCallbacks {
private:
    RemoveUnsupportedSyntaxAction *action;
    std::vector<std::string> includeDirectives;
public:
    RemoveUnsupportedSyntaxActionPPCallbacks(RemoveUnsupportedSyntaxAction *action)
            : action(action),
              includeDirectives() {}

    virtual void FileChanged(clang::SourceLocation Loc, FileChangeReason Reason, clang::SrcMgr::CharacteristicKind FileType, clang::FileID PrevFID) override;

    virtual void InclusionDirective(clang::SourceLocation HashLoc, const clang::Token &IncludeTok, clang::StringRef FileName, bool IsAngled, clang::CharSourceRange FilenameRange,
                                    const clang::FileEntry *File, clang::StringRef SearchPath, clang::StringRef RelativePath, const clang::Module *Imported) override;

    virtual void EndOfMainFile() override;
};

class RemoveUnsupportedSyntaxAction : public clang::PreprocessorFrontendAction {
private:
    friend class RemoveUnsupportedSyntaxActionPPCallbacks;
    clang::Preprocessor *preprocessor;
    std::map<std::string, std::stringstream>& filesMap;
    std::string currentFileName;

public:
    RemoveUnsupportedSyntaxAction(std::map<std::string, std::stringstream>& filesMap)
            : preprocessor(nullptr),
              filesMap(filesMap) {}

    virtual void ExecuteAction() override {}

    virtual bool BeginSourceFileAction(clang::CompilerInstance &CI, clang::StringRef Filename) override;
};

