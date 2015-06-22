//
// Created by Ivan Buhov on 6/18/15.
//

#include "RemoveUnsupportedSyntaxAction.h"

void RemoveUnsupportedSyntaxActionPPCallbacks::FileChanged(clang::SourceLocation Loc, FileChangeReason Reason, clang::SrcMgr::CharacteristicKind FileType, clang::FileID PrevFID) {
        std::string newFile = action->preprocessor->getSourceManager().getFilename(Loc).str();
        if(Reason == FileChangeReason::EnterFile && action->filesMap.find(newFile) == action->filesMap.end()) {
                if(this->includeDirectives.size() > 0)
                        action->filesMap[action->currentFileName] << std::string("\n") << this->includeDirectives[this->includeDirectives.size() - 1];
                action->filesMap.insert(std::pair<std::string, std::stringstream>(newFile, std::stringstream()));
        }
        action->currentFileName = newFile;
}

void RemoveUnsupportedSyntaxActionPPCallbacks::InclusionDirective(clang::SourceLocation HashLoc, const clang::Token &IncludeTok, clang::StringRef FileName,
                                                                  bool IsAngled, clang::CharSourceRange FilenameRange, const clang::FileEntry *File,
                                                                  clang::StringRef SearchPath, clang::StringRef RelativePath, const clang::Module *Imported) {
        std::string openingSymbol = IsAngled ? "<" : "\"";
        std::string closingSymbol = IsAngled ? ">" : "\"";
        std::stringstream includeDirective;
        includeDirective << "#" << action->preprocessor->getSpelling(IncludeTok) << " " << openingSymbol << FileName.str() << closingSymbol;
        this->includeDirectives.push_back(includeDirective.str());
}

void RemoveUnsupportedSyntaxActionPPCallbacks::EndOfMainFile() {
        //printf("End of processing.");
}

bool RemoveUnsupportedSyntaxAction::BeginSourceFileAction(clang::CompilerInstance &CI, clang::StringRef Filename) {
        // setup preprocessor
        CI.getPreprocessor().addPPCallbacks(std::unique_ptr<clang::PPCallbacks>(new RemoveUnsupportedSyntaxActionPPCallbacks(this)));
        this->preprocessor = &CI.getPreprocessor();
        const clang::FileEntry *mainFileEntry = preprocessor->getFileManager().getFile(Filename);
        clang::FileID fileId = preprocessor->getSourceManager().createFileID(mainFileEntry, clang::SourceLocation(), clang::SrcMgr::CharacteristicKind::C_User);
        preprocessor->getSourceManager().setMainFileID(fileId);
        preprocessor->EnterMainSourceFile();

        // parse tokens
        clang::Token token;
        bool isTokenInvalid = false;
        do {
                preprocessor->Lex(token);
                if(preprocessor->getDiagnostics().hasErrorOccurred()) {
                        break;
                }

                std::string spelling = preprocessor->getSpelling(token, &isTokenInvalid);
                if(isTokenInvalid) {
                        preprocessor->DumpToken(token);
                }
                this->filesMap[currentFileName] << (token.isAtStartOfLine() ? "\n" : "") << (token.hasLeadingSpace() ? " " : "") << spelling;
        } while(token.isNot(clang::tok::eof));

        return true;
}