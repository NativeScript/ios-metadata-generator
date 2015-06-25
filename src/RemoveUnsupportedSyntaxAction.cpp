//
// Created by Ivan Buhov on 6/18/15.
//

#include "RemoveUnsupportedSyntaxAction.h"

std::string GenericDeclarationsProcessor::process(clang::Token& token) {
    switch(stateInfo.state) {
        case State::None : {
            if(token.is(clang::tok::TokenKind::at))
                stateInfo.state = State::RecognitionPhase1;
            else
                stateInfo.clear();
            return next(token);
        }
        case State::RecognitionPhase1 : {
            if(token.is(clang::tok::TokenKind::identifier) && preprocessor.getSpelling(token) == "interface")
                stateInfo.state = State::RecognitionPhase2;
            else
                stateInfo.clear();
            return next(token);
        }
        case State::RecognitionPhase2 : {
            if(token.is(clang::tok::TokenKind::identifier)) {
                stateInfo.genericInterfaceName = preprocessor.getSpelling(token);
                stateInfo.state = State::RecognitionPhase3;
            } else {
                stateInfo.clear();
            }
            return next(token);
        }
        case State::RecognitionPhase3 : {
            if(token.is(clang::tok::TokenKind::less)) {
                std::vector<std::string>* genericDeclarations = getFromContext<std::vector<std::string>>(GenericsUsagesProcessor::GenericInterfacesKey);
                if(genericDeclarations != nullptr)
                    genericDeclarations->push_back(stateInfo.genericInterfaceName);
                stateInfo.state = State::InAngleBrackets;
                return " ";
            } else {
                stateInfo.clear();
                return next(token);
            }
        }
        case State::InAngleBrackets : {
            if(token.is(clang::tok::TokenKind::less))
                stateInfo.angleBracketsLevel++;
            else if(token.is(clang::tok::TokenKind::greater))
                stateInfo.angleBracketsLevel--;
            else if(token.is(clang::tok::TokenKind::lessless))
                stateInfo.angleBracketsLevel += 2;
            else if(token.is(clang::tok::TokenKind::greatergreater))
                stateInfo.angleBracketsLevel -= 2;
            else if(token.is(clang::tok::TokenKind::identifier))
                stateInfo.genericParameterName = preprocessor.getSpelling(token);
            if(stateInfo.angleBracketsLevel == -1) {
                stateInfo.angleBracketsLevel = 0;
                stateInfo.state = State::InDeclaration;
            }
//            static int i(0);
//            i++;
//            printf("%s [%s]\n", preprocessor.getSpelling(token).c_str(), token.getLocation().printToString(preprocessor.getSourceManager()).c_str());
//            if(i % 10 == 0) {
//                printf("breakpoint\n");
//            }
            return "";
        }
        case State::InDeclaration : {
            if(token.is(clang::tok::TokenKind::at))
                stateInfo.state = State::EndRecognitionPhase1;
            else if(token.is(clang::tok::TokenKind::identifier) && preprocessor.getSpelling(token) == stateInfo.genericParameterName) {
                std::string result = next(token);
                return (result.empty()) ? result : "id";
            }
            return next(token);
        }
        case State::EndRecognitionPhase1 : {
            if(token.is(clang::tok::TokenKind::identifier) && preprocessor.getSpelling(token) == "end")
                stateInfo.clear();
            else
                stateInfo.state = State::InDeclaration;
            return next(token);
        }
    }
}

const std::string GenericsUsagesProcessor::GenericInterfacesKey("GenericsUsagesProcessor.GenericInterfacesKey");

std::string GenericsUsagesProcessor::process(clang::Token& token) {
    switch(stateInfo.state) {
        case State::None : {
            if(token.is(clang::tok::TokenKind::identifier)) {
                std::string tokenSpelling = preprocessor.getSpelling(token);
                std::vector<std::string>* genericInterfaces = getFromContext<std::vector<std::string>>(GenericsUsagesProcessor::GenericInterfacesKey);
                if(genericInterfaces != nullptr && std::find(genericInterfaces->begin(), genericInterfaces->end(), tokenSpelling) != genericInterfaces->end())
                    stateInfo.state = State::RecognitionPhase1;
            }
            else if(token.is(clang::tok::TokenKind::colon)) {
                stateInfo.state = State::AfterColon;
            }
            return next(token);
        }
        case State::AfterColon : {
            if(!token.is(clang::tok::TokenKind::colon))
                stateInfo.state = State::None;
            return next(token);
        }
        case State::RecognitionPhase1 : {
            if(token.is(clang::tok::TokenKind::less)) {
                stateInfo.state = State::InAngleBrackets;
                return " ";
            } else {
                stateInfo.clear();
                return next(token);
            }
        }
        case State::InAngleBrackets : {
            if(token.is(clang::tok::TokenKind::less))
                stateInfo.angleBracketsLevel++;
            else if(token.is(clang::tok::TokenKind::greater))
                stateInfo.angleBracketsLevel--;
            else if(token.is(clang::tok::TokenKind::lessless))
                stateInfo.angleBracketsLevel += 2;
            else if(token.is(clang::tok::TokenKind::greatergreater))
                stateInfo.angleBracketsLevel -= 2;
            if(stateInfo.angleBracketsLevel == -1) {
                stateInfo.clear();
            }
            return "";
        }
    }
}

std::string GenericsForwardDeclarationsProcessor::process(clang::Token& token) {
    switch(stateInfo.state) {
        case State::None : {
            if(token.is(clang::tok::TokenKind::at))
                stateInfo.state = State::RecognitionPhase1;
            else
                stateInfo.clear();
            return next(token);
        }
        case State::RecognitionPhase1 : {
            if(token.is(clang::tok::TokenKind::identifier) && preprocessor.getSpelling(token) == "class")
                stateInfo.state = State::RecognitionPhase2;
            else
                stateInfo.clear();
            return next(token);
        }
        case State::RecognitionPhase2 : {
            if(token.is(clang::tok::TokenKind::identifier)) {
                stateInfo.state = State::RecognitionPhase3;
                stateInfo.genericInterfaceName = preprocessor.getSpelling(token);
            } else {
                stateInfo.clear();
            }
            return next(token);
        }
        case State::RecognitionPhase3 : {
            if(token.is(clang::tok::TokenKind::less)) {
                stateInfo.state = State::InAngleBrackets;
                std::vector<std::string> *genericsNames = getFromContext<std::vector<std::string>>(GenericsUsagesProcessor::GenericInterfacesKey);
                if(genericsNames != nullptr && std::find(genericsNames->begin(), genericsNames->end(), stateInfo.genericInterfaceName) == genericsNames->end()) {
                    genericsNames->push_back(stateInfo.genericInterfaceName);
                }
                return "";
            } else {
                stateInfo.clear();
                return next(token);
            }
        }
        case State::InAngleBrackets : {
            if(token.is(clang::tok::TokenKind::less))
                stateInfo.angleBracketsLevel++;
            else if(token.is(clang::tok::TokenKind::greater))
                stateInfo.angleBracketsLevel--;
            else if(token.is(clang::tok::TokenKind::lessless))
                stateInfo.angleBracketsLevel += 2;
            else if(token.is(clang::tok::TokenKind::greatergreater))
                stateInfo.angleBracketsLevel -= 2;
            if(stateInfo.angleBracketsLevel == -1) {
                stateInfo.clear();
            }
            return "";
        }
    }
}

std::string NullabilityModifiersProcessor::process(clang::Token& token) {
    switch(stateInfo.state) {
        case State::None : {
            if(token.is(clang::tok::TokenKind::identifier)) {
                std::string spelling = preprocessor.getSpelling(token);
                if(spelling == "nonnull" || spelling == "nullable" || spelling == "__null_unspecified") {
                    stateInfo.state = State::AfterNullabilityModifier;
                    return "";
                }
            }
            return next(token);
        }
        case State::AfterNullabilityModifier : {
            stateInfo.state = State::None;
            return token.is(clang::tok::TokenKind::comma) ? "" : next(token);
        }
    }



    return next(token);
}

std::string DefaultProcessor::process(clang::Token& token) {
    bool isTokenInvalid = false;
    std::string spelling = preprocessor.getSpelling(token, &isTokenInvalid);
    return isTokenInvalid ? "" : (std::string(token.isAtStartOfLine() ? "\n" : "") + (token.hasLeadingSpace() ? " " : "") + spelling);
}

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

    std::map<std::string, void*> context;
    GenericDeclarationsProcessor tokensProcessor(*preprocessor, context,
    std::make_shared<NullabilityModifiersProcessor>(*preprocessor, context,
    std::make_shared<GenericsUsagesProcessor>(*preprocessor, context,
    std::make_shared<GenericsForwardDeclarationsProcessor>(*preprocessor, context,
    std::make_shared<DefaultProcessor>(*preprocessor, context, nullptr)))));
    //GenericDeclarationsProcessor tokensProcessor(*preprocessor, context, std::make_shared<DefaultProcessor>(*preprocessor, context, nullptr));

    // parse tokens
    clang::Token token;
    do {
        preprocessor->Lex(token);
        if(preprocessor->getDiagnostics().hasErrorOccurred())
            break;
        this->filesMap[currentFileName] << tokensProcessor.process(token);
    } while(token.isNot(clang::tok::eof));

    return true;
}